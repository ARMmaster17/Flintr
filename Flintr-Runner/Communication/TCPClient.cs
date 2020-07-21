using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Flintr_Runner.Communication
{
    public class TCPClient
    {
        private Socket sender;

        public TCPClient(IPAddress address, int port)
        {
            IPEndPoint hostEndpoint = new IPEndPoint(address, port);
            sender = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(hostEndpoint);
        }

        public void Send(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message + "<EOF>");
            sender.Send(msg);
        }

        public bool MessageIsAvailable()
        {
            return sender.Available > 0;
        }

        public string Recieve()
        {
            if (!MessageIsAvailable()) return null;
            string data = null;
            byte[] byteStream = new Byte[1024];
            while (true)
            {
                int bytesRec = sender.Receive(byteStream);
                data += Encoding.ASCII.GetString(byteStream, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1) break;
            }
            data = data.Remove(data.Length - 5);
            return data;
        }
    }
}