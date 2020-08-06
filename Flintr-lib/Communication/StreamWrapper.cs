using Flintr_lib.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Flintr_lib.Communication
{
    public class StreamWrapper
    {
        private NetworkStream stream;
        private SerializerFactory serializerFactory;

        /// <summary>
        /// Default constructor for StreamWrapper class. Binds to the specified network stream.
        /// </summary>
        /// <param name="networkStream">Network stream to bind to.</param>
        public StreamWrapper(NetworkStream networkStream)
        {
            serializerFactory = new SerializerFactory();
            stream = networkStream;
        }

        ~StreamWrapper()
        {
            stream.Close();
        }

        /// <summary>
        /// Send an object of type T through the network using TCP. Object obj must
        /// be serializable.
        /// </summary>
        /// <typeparam name="T">Base type of object to transmit.</typeparam>
        /// <param name="obj">Object to transmit.</param>
        /// <exception cref="ArgumentException">When the provided Type T or object to serialize are invalid.</exception>
        /// <exception cref="IOException">When the serialization or transmission of object obj fails.</exception>
        public void SendObject<T>(T obj)
        {
            // Create a MemoryStream so we can get the message length for the header information.
            MemoryStream ms = serializerFactory.SerializeToStream<T>(obj);

            // Write the bytes to the stream with automated header construction.
            try
            {
                writeBytesToStream(ms.ToArray());
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("An error occurred while parsing the serialized object.", e);
            }
            catch (IOException e)
            {
                throw new IOException("An error occurred while writing to the underlying NetworkStream object.", e);
            }
        }

        /// <summary>
        /// Receive a deserialized object from the network. This method is blocking until a complete object is received.
        /// </summary>
        /// <typeparam name="T">Base type of object to deserialize.</typeparam>
        /// <returns>Deserialized object transmitted through network.</returns>
        /// <exception cref="ArgumentException">When an invalid type is passed as an argument.</exception>
        /// <exception cref="IOException">When an error occurs with an underlying NetworkStream read or deserialization operation.</exception>
        public T ReceiveObject<T>()
        {
            return serializerFactory.Deserialize<T>(readMessageFromStream());
        }

        /// <summary>
        /// Checks if data is available to be read from the incoming stream.
        /// </summary>
        /// <returns>If at least one byte of data is ready to be read on the underlying NetworkStream.</returns>
        /// <exception cref="IOException">When an error occurs with a corrupted, missing, or closed NetworkStream.</exception>
        public bool MessageIsAvailable()
        {
            bool result;

            try
            {
                result = stream.DataAvailable;
            }
            catch (ObjectDisposedException e)
            {
                // TODO: Attempt to re-open the stream and try again if this happens.
                throw new IOException("The underlying NetworkStream was unexpectedly closed.", e);
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while probing the network socket for pending data.", e);
            }

            return result;
        }

        /// <summary>
        /// Builds a header for a given transmission buffer, then sends the header and body across the network.
        /// </summary>
        /// <param name="buffer">Pre-encoded message body to transmit.</param>
        /// <exception cref="ArgumentException">When an invalid message is passed or was parsed incorrectly.</exception>
        /// <exception cref="IOException">When an error occurs while accessing or writing to the underlying NetworkStream.</exception>
        private void writeBytesToStream(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) throw new ArgumentException("Raw TCP message cannot be empty.");

            byte[] header = ApplicationLayer.GenerateMessageHeader(buffer.Length);

            lock (stream)
            {
                try
                {
                    stream.Write(header, 0, header.Length);
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (ObjectDisposedException e)
                {
                    throw new IOException("The underlying NetworkStream object was unexpectedly closed.", e);
                    // TODO: Attempt to reconnect and try to send again.
                }
                catch (Exception e)
                {
                    throw new IOException("An error occurred while writing to the NetworkStream.", e);
                }
            }
        }

        /// <summary>
        /// Reads a byte-stream from the network. Handles header parsing to ensure entire message is received
        /// before returning byte[] array of message body.
        /// </summary>
        /// <returns>Byte[] array representing message body.</returns>
        /// <exception cref="IOException">When an error occurs while reading the header or body of an incoming TCP network message.</exception>
        private MemoryStream readMessageFromStream()
        {
            lock (stream)
            {
                waitForData();

                int msgLength = ApplicationLayer.GetMessageLengthFromHeader(stream);

                byte[] msgBuffer = ApplicationLayer.GetMessageBody(stream, msgLength);

                MemoryStream ms = new MemoryStream(msgBuffer);
                return ms;
            }
        }

        /// <summary>
        /// Blocks the current thread until data is available.
        /// </summary>
        /// <exception cref="TimeoutException">When no data is recieved in a timespan greater than the maximum timeout specified in the application configuration.</exception>
        private void waitForData()
        {
            int waitTime = 0;

            while (!stream.DataAvailable)
            {
                Thread.Sleep(100);
                waitTime += 100;
                if(waitTime > ApplicationLayer.DataWaitTimeout)
                {
                    throw new TimeoutException($"No data recieved for over {waitTime / 100} seconds.");
                }
            }
        }
    }
}
