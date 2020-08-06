using Flintr_lib.Factory;
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
        private StreamWrapper streamWrapper;

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
                streamWrapper = new StreamWrapper(sender.GetStream());
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
                streamWrapper = new StreamWrapper(sender.GetStream());
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
                streamWrapper = new StreamWrapper(sender.GetStream());
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
            sender.Close();
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
            streamWrapper.SendObject<T>(obj);
        }

        /// <summary>
        /// Checks if data is available to be read from the incoming stream.
        /// </summary>
        /// <returns>If at least one byte of data is ready to be read on the underlying NetworkStream.</returns>
        /// <exception cref="IOException">When an error occurs with a corrupted, missing, or closed NetworkStream.</exception>
        public bool MessageIsAvailable()
        {
            return streamWrapper.MessageIsAvailable();
        }

        // TODO: read methods should flush the stream on a read/write error.

        /// <summary>
        /// Receive a deserialized object from the network. This method is blocking until a complete object is received.
        /// </summary>
        /// <typeparam name="T">Base type of object to deserialize.</typeparam>
        /// <returns>Deserialized object transmitted through network.</returns>
        /// <exception cref="ArgumentException">When an invalid type is passed as an argument.</exception>
        /// <exception cref="IOException">When an error occurs with an underlying NetworkStream read or deserialization operation.</exception>
        public T ReceiveObject<T>()
        {
            return streamWrapper.ReceiveObject<T>();
        }        
    }
}