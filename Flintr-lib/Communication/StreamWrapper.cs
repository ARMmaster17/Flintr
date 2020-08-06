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

        private static readonly byte[] validMsgHeader = Encoding.ASCII.GetBytes("MSG|");
        private static readonly int validMsgHeaderBlockLength = validMsgHeader.Length;
        private static readonly int dataWaitTimeout = 10000;

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
            return serializerFactory.Deserialize<T>(readBytesFromStream());
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
            byte[] header = Encoding.ASCII.GetBytes($"MSG|{buffer.Length}\n");
            lock (stream)
            {
                try
                {
                    stream.Write(header, 0, header.Length);
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (ArgumentNullException e)
                {
                    throw new ArgumentException("Raw TCP message cannot be empty.", e);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    throw new ArgumentException("Mismatch between stated message length and actual message length.", e);
                }
                catch (ObjectDisposedException e)
                {
                    throw new IOException("The underlying NetworkStream object was unexpectedly closed.", e);
                }
                catch (InvalidOperationException e)
                {
                    throw new IOException("An error occurred while writing to the NetworkStream.", e);
                }
                catch (IOException e)
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
        private MemoryStream readBytesFromStream()
        {
            lock (stream)
            {
                waitForData();

                int msgLength = 0;
                try
                {
                    msgLength = readMsgHeader();
                }
                catch (IOException e)
                {
                    throw new IOException("An error occurred while reading a message header from the underlying NetworkStream", e);
                }

                byte[] msgBuffer = new byte[msgLength];
                try
                {
                    readNBytesFromStream(ref msgBuffer, msgLength);
                }
                catch (Exception e)
                {
                    throw new IOException("An error occurred while reading a message body from the the underlying NetworkStream", e);
                }

                MemoryStream ms = new MemoryStream(msgBuffer);
                return ms;
            }
        }

        /// <summary>
        /// Obtains a message header from the network stream that specifies attributes about
        /// the upcoming message to be transmitted.
        /// </summary>
        /// <returns>Expected size of message body in bytes to be received.</returns>
        /// <exception cref="IOException">When an error occurs with accessing the NetworkStream or the incoming message header contains invalid values.</exception>
        private int readMsgHeader()
        {
            byte[] headerDeclaration = new byte[validMsgHeaderBlockLength];
            try
            {
                readNBytesFromStream(ref headerDeclaration, validMsgHeaderBlockLength);
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while reading a message header the underlying NetworkStream", e);
            }

            if (!compareByteBuffers(validMsgHeader, headerDeclaration)) throw new IOException("Malformed MSG received through TCP channel.");

            string msgLengthAttribute = "";
            try
            {
                msgLengthAttribute = readUntilTerminationCharacter('\n');
            }
            catch (IOException e)
            {
                throw new IOException("An error occurred while processing a message header.", e);
            }

            int msgLength = 0;
            try
            {
                msgLength = Convert.ToInt32(msgLengthAttribute);
            }
            catch (FormatException e)
            {
                throw new IOException("Malformed MSG received through TCP channel.", e);
            }
            catch (OverflowException e)
            {
                throw new IOException("Network message size is too big. Break the message into smaller chunks and try again.", e);
            }

            return msgLength;
        }

        /// <summary>
        /// Reads from the network stream one character at a time until the specified termination character is found.
        /// Only used for parsing header information.
        /// </summary>
        /// <param name="terminationCharacter">Character to stop at once found in the underlying NetworkStream.</param>
        /// <returns>All characters from beginning of stream until the termination character (exclusive, but still removed from stream).</returns>
        /// <exception cref="IOException">When an error occurs in reading the underlying NetworkStream or when parsing the input.</exception>
        private string readUntilTerminationCharacter(char terminationCharacter)
        {
            string msgLengthAttribute = "";
            byte[] lengthBuffer = new byte[1];
            while (true)
            {
                try
                {
                    readNBytesFromStream(ref lengthBuffer, 1);
                }
                catch (Exception e)
                {
                    throw new IOException("An error occurred while reading the underlying NetworkStream", e);
                }

                string s = Encoding.ASCII.GetString(lengthBuffer);
                if (s == terminationCharacter.ToString()) break;
                else msgLengthAttribute += s;
            }
            return msgLengthAttribute;
        }

        /// <summary>
        /// Reads a guaranteed number of bytes from a stream. Method is blocking until
        /// all expectedSize bytes are received.
        /// </summary>
        /// <param name="buffer">Buffer to write to.</param>
        /// <param name="expectedSize">Expected size of message to receive</param>
        /// <exception cref="ArgumentException">When supplied buffer or expected message size arguments are invalid.</exception>
        /// <exception cref="IOException">When a problem occurs with the underlying NetworkStream.</exception>
        private void readNBytesFromStream(ref byte[] buffer, int expectedSize)
        {
            // TODO: Add timeout detection with custom exception thrown.
            int bytesRead = 0;
            while (bytesRead != expectedSize)
            {
                Thread.Sleep(100);
                try
                {
                    bytesRead = stream.Read(buffer, 0, expectedSize);
                }
                catch (ArgumentNullException e)
                {
                    throw new ArgumentException("A null buffer or expected message size argument was passed to a NetworkStream read function.", e);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    throw new ArgumentException("An invalid expected message size argument was passed to a NetworkStream read function.", e);
                }
                catch (ObjectDisposedException e)
                {
                    throw new IOException("The underlying NetworkStream was unexpectedly closed.", e);
                }
                catch (InvalidOperationException e)
                {
                    throw new IOException("An error occurred while accessing the data in the NetowrkStream.", e);
                }
            }
        }

        /// <summary>
        /// Helper function that compares the values and length of two byte[] arrays.
        /// </summary>
        /// <param name="a">Base array to compare.</param>
        /// <param name="b">Second array to compare.</param>
        /// <returns>If the arrays are exactly value-equal.</returns>
        /// <exception cref="ArgumentNullException">When the supplied arguments are NULL in value.</exception>
        private static bool compareByteBuffers(byte[] a, byte[] b)
        {
            if (a == null) throw new ArgumentNullException("a", "Supplied parameter was NULL.");
            if (b == null) throw new ArgumentNullException("b", "Supplied parameter was NULL.");
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
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
                if(waitTime > dataWaitTimeout)
                {
                    throw new TimeoutException($"No data recieved for over {dataWaitTimeout / 100} seconds.");
                }
            }
        }
    }
}
