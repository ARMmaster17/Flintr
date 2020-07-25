using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_lib.Communication
{
    public class TCPServer
    {
        private TcpListener listener;

        public TCPServer(IPAddress bindAddress, int bindPort)
        {
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