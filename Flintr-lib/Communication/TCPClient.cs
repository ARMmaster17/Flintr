using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Flintr_lib.Communication
{
    /// <summary>
    /// Wrapper around Net.TcpClient. Holds connection state information and
    /// buffer services to send messages and objects over the network using TCP.
    /// </summary>
    public class TCPClient
    {
        private TcpClient sender;
        private NetworkStream stream;
        private static readonly byte[] validMsgHeader = Encoding.ASCII.GetBytes("MSG|");
        private static readonly int validMsgHeaderBlockLength = validMsgHeader.Length;

        /// <summary>
        /// Creates an instance of TCPClient and attempts to connect to
        /// the specified IP address and port number over TCP.
        /// </summary>
        /// <param name="address">IP address of server to connect to.</param>
        /// <param name="port">Port number to connect to. (Source port is random.)</param>
        /// <exception cref="ArgumentException">When supplied connection information is not valid.</exception>
        /// <exception cref="IOException">When an error occurs in initializing the connection.</exception>
        public TCPClient(IPAddress address, int port)
        {
            // Generate endpoint object to feed to underlying TcpClient.
            IPEndPoint hostEndpoint = null;
            try
            {
                hostEndpoint = new IPEndPoint(address, port);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException("Supplied IP address or port is NULL. Check values and try again.", e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentException("Supplied IP address or port number is not valid. Check values and try again.", e);
            }

            sender = new TcpClient();
            // Attempt to connect to endpoint, and catch any error that occurs.
            try
            {
                sender.Connect(hostEndpoint);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException($"Invalid connection information '{address.ToString()}:{port}', Connection could not be established.", e);
            }
            catch (SocketException e)
            {
                throw new IOException($"Could not connect to endpoint '{address.ToString()}:{port}'. Check that the host is up and accepting connections. Ensure your firewall is not blocking Flintr.", e);
            }

            // Get the underlying stream for the socketed connection.
            try
            {
                stream = sender.GetStream();
            }
            catch (ObjectDisposedException e)
            {
                throw new IOException("Underlying network stream was unexpectedly closed or not properly initialized.", e);
            }
        }

        /// <summary>
        /// Creates an instance of TCPClient and attempts to connect
        /// to the specified IPEndPoint over TCP.
        /// </summary>
        /// <param name="hostEndpoint">Endpoint of server that is listening for TCP connections to connect to.</param>
        /// <exception cref="ArgumentException">When provided hostEndpoint is not a valid network destination.</exception>
        /// <exception cref="IOException">When an error occurs while establishing a connection with the specified endpoint.</exception>
        public TCPClient(IPEndPoint hostEndpoint)
        {
            sender = new TcpClient();

            try
            {
                sender.Connect(hostEndpoint);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException("Invalid IPEndPoint supplied. Connection could not be established.", e);
            }
            catch (SocketException e)
            {
                throw new IOException("Could not connect to endpoint. Check that the host is up and accepting connections. Ensure your firewall is not blocking Flintr.", e);
            }

            // Get the underlying stream for the socketed connection.
            try
            {
                stream = sender.GetStream();
            }
            catch (ObjectDisposedException e)
            {
                throw new IOException("Underlying network stream was unexpectedly closed or was not properly initialized.", e);
            }
        }

        /// <summary>
        /// Creates an instance of TCPClient from an existing connection state (usually a pre-established connection
        /// from TCPServer).
        /// </summary>
        /// <param name="newClientConnection">Pre-established client connection object.</param>
        /// <exception cref="IOException">When the provided existing connection does not have a valid network stream handle.</exception>
        public TCPClient(TcpClient newClientConnection)
        {
            sender = newClientConnection;
            // Get the underlying stream for the socketed connection.
            try
            {
                stream = sender.GetStream();
            }
            catch (ObjectDisposedException e)
            {
                throw new IOException("Underlying network stream was unexpectedly closed or was not properly initialized.", e);
            }
        }

        /// <summary>
        /// Deconstructor for TCPClient class. Handles the closing of the streams before garbage collection.
        /// </summary>
        ~TCPClient()
        {
            stream.Close();
            sender.Close();
        }

        /// <summary>
        /// Send an ASCII-encoded string message through the network using TCP.
        /// </summary>
        /// <param name="message">ASCII-encoding safe message to transmit.</param>
        /// <exception cref="ArgumentException">When provided message is invalid.</exception>
        /// <exception cref="IOException">When a write operation to the network stream fails.</exception>
        public void Send(string message)
        {
            if (message == null) throw new ArgumentException("Parameter message cannot be NULL.");
            // Encode the message to an ASCII byte[] array.
            byte[] encodedMessage = Encoding.ASCII.GetBytes(message);
            // Write the bytes to the stream with automated header construction.
            try
            {
                writeBytesToStream(encodedMessage);
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while writing a message to the network stream.", e);
            }
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
            // Create the serializer. Using XMLSerializer because BinarySerializer
            // has security issues according to MSDN.
            // TODO: Create a cache of serializers based on type.
            XmlSerializer xmlSerializer;
            try
            {
                xmlSerializer = new XmlSerializer(typeof(T));
            }
            catch (Exception e)
            {
                throw new ArgumentException($"An error occurred initializing a serializer for objects of type {typeof(T).Name}.", e);
            }
            // Create a MemoryStream so we can get the message length for the header information.
            MemoryStream ms = new MemoryStream();
            // Serialize the object to XML and write it to the stream.
            try
            {
                xmlSerializer.Serialize(ms, obj);
            }
            catch (Exception e)
            {
                throw new IOException($"Object of type {typeof(T).Name} could not be serialized", e);
            }
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
            catch (IOException e)
            {
                throw new IOException("An exception occurred while checking for pending data in the NetworkStream.", e);
            }
            catch (SocketException e)
            {
                throw new IOException("An error occurred while probing the network socket for pending data.", e);
            }

            return result;
        }

        // TODO: read methods should flush the stream on a read/write error.

        /// <summary>
        /// Receive an ASCII-encoded string message from the network. This method is blocking until a complete MSG transmission is read.
        /// </summary>
        /// <returns>Complete decoded message received from the network through TCP.</returns>
        /// <exception cref="IOException">When an internal network error occurs while reading the network stream.</exception>
        public string Receive()
        {
            MemoryStream ms;
            try
            {
                ms = readBytesFromStream();
            }
            catch (IOException e)
            {
                throw new IOException("An error occurred while reading a string message from the network.", e);
            }

            string message = "";

            try
            {
                message = Encoding.ASCII.GetString(ms.ToArray());
            }
            catch (Exception e)
            {
                throw new IOException("An error occured while decoding a message from the network.", e);
            }

            return message;
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
            // We have to use a MemoryStream because the NetworkStream can't tell us that
            // the object buffer is complete, so the XmlSerializer will hang if we just pass
            // the NetworkStream.
            MemoryStream ms;
            try
            {
                ms = readBytesFromStream();
            }
            catch (IOException e)
            {
                throw new IOException("An error occurred while receiving an object from the NetworkStream.", e);
            }
            XmlSerializer xmlSerializer;
            try
            {
                xmlSerializer = new XmlSerializer(typeof(T));
            }
            catch (Exception e)
            {
                throw new ArgumentException($"An error occurred initializing a serializer for objects of type {typeof(T).Name}.", e);
            }

            object o;
            try
            {
                o = xmlSerializer.Deserialize(ms);
            }
            catch (Exception e)
            {
                throw new IOException($"Incoming network object could not be deserialized to a valid object.", e);
            }

            try
            {
                return (T)o;
            }
            catch (Exception e)
            {
                throw new IOException($"Incoming deserialized network object could not be converted to type {typeof(T).Name}.", e);
            }
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
                // Wait for at least some data to become available so we can read the header.
                while (!stream.DataAvailable)
                {
                    Thread.Sleep(100);
                }

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
    }
}