using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.Communication
{
    public class TCPServer
    {
        private Socket listener;
        private Logger.Logger SharedLogger;

        public TCPServer(IPAddress bindAddress, int bindPort)
        {
            IPEndPoint endpoint = new IPEndPoint(bindAddress, bindPort);
            listener = new Socket(bindAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endpoint);
            listener.Listen(100);
        }

        public Socket WaitForNextConnection()
        {
            return listener.Accept();
        }

        public string GetNextMessage(Socket clientConnection)
        {
            byte[] byteStream = new Byte[1024];
            string data = null;
            while (true)
            {
                int bytesRec = clientConnection.Receive(byteStream);
                data += Encoding.ASCII.GetString(byteStream, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1) break;
            }
            data = data.Remove(data.Length - 5);
            return data;
        }

        public void SendMesage(Socket clientConnection, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message + "<EOF>");
            clientConnection.Send(msg);
        }

        public void CloseClientConnection(Socket clientConnection)
        {
            clientConnection.Shutdown(SocketShutdown.Both);
            clientConnection.Close();
        }
    }
}