using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Flintr_lib.Communication
{
    /// <summary>
    /// Wrapper around TcpListener class. Provides TCP network support
    /// and generates a TCPClient whenever a new connection is requested.
    /// </summary>
    public class TCPServer
    {
        private TcpListener listener;

        /// <summary>
        /// Initializer. Sets up a TCP network listener and starts
        /// listening for new connections.
        /// </summary>
        /// <param name="bindAddress">IP address to bind to.</param>
        /// <param name="bindPort">Port number to listen on for new connections.</param>
        /// <exception cref="ArgumentException">When invalid connection details are specified.</exception>
        /// <exception cref="IOException">When a socket error occurs while setting up listener.</exception>
        public TCPServer(IPAddress bindAddress, int bindPort)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(bindAddress, bindPort);
                listener = new TcpListener(endpoint);
                listener.Start();
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentException("Binding IP address and port cannot be NULL.", e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentException("Invalid IP address or bind port specified. Check values and try again.", e);
            }
            catch (SocketException e)
            {
                throw new IOException("An error occurred while initializing a TCP listener.", e);
            }
        }

        /// <summary>
        /// De-initializer. Cleans up connections before handoff to garbage collector.
        /// </summary>
        ~TCPServer()
        {
            listener.Stop();
        }

        /// <summary>
        /// Listens for new connection requests. Blocks thread until new connection is established.
        /// </summary>
        /// <returns>TCPClient instance that manages the connection.</returns>
        /// <exception cref="IOException">When a listening or connection creation network operation fails.</exception>
        public TCPClient WaitForNextConnection()
        {
            TcpClient tc;
            try
            {
                tc = listener.AcceptTcpClient();
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while listening for new connection requests.", e);
            }

            TCPClient client;

            try
            {
                client = new TCPClient(tc);
            }
            catch (IOException e)
            {
                throw new IOException("A network error occured while establishing a new connection with a client.", e);
            }

            return client;
        }

        /// <summary>
        /// Closes a given TcpClient connection.
        /// </summary>
        /// <param name="clientConnection">Connection to close.</param>
        public static void CloseClientConnection(TcpClient clientConnection)
        {
            clientConnection.Close();
        }
    }
}