using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.Communication
{
    public class TCPServer
    {
        private TcpListener listener;
        private Logger.Logger sharedLogger;

        public TCPServer(IPAddress bindAddress, int bindPort, RuntimeConfiguration runtimeConfiguration)
        {
            sharedLogger = runtimeConfiguration.GetLoggerInstance();
            IPEndPoint endpoint = new IPEndPoint(bindAddress, bindPort);
            listener = new TcpListener(endpoint);
            listener.Start();
        }

        ~TCPServer()
        {
            listener.Stop();
        }

        public TCPClient WaitForNextConnection()
        {
            return new TCPClient(listener.AcceptTcpClient());
        }

        public static void CloseClientConnection(TcpClient clientConnection)
        {
            clientConnection.Close();
        }
    }
}