using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Flintr_lib
{
    public class TCPClient
    {
        private TcpClient sender;
        private NetworkStream stream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        public TCPClient(IPAddress address, int port)
        {
            IPEndPoint hostEndpoint = new IPEndPoint(address, port);
            //try
            //{
            sender = new TcpClient();
            sender.Connect(hostEndpoint);
            //}
            //catch (SocketException e)
            //{
            //}
            while (!sender.Connected)
            {
                Thread.Sleep(500);
            }
            stream = sender.GetStream();
            streamWriter = new StreamWriter(stream);
            streamReader = new StreamReader(stream);
        }

        public TCPClient(IPEndPoint hostEndpoint)
        {
            //try
            //{
            sender = new TcpClient();
            sender.Connect(hostEndpoint);
            //}
            //catch (SocketException e)
            //{
            //}
            while (!sender.Connected)
            {
                Thread.Sleep(500);
            }
            stream = sender.GetStream();
            streamWriter = new StreamWriter(stream);
            streamReader = new StreamReader(stream);
        }

        public TCPClient(TcpClient newClientConnection)
        {
            sender = newClientConnection;
            stream = sender.GetStream();
            streamWriter = new StreamWriter(stream);
            streamReader = new StreamReader(stream);
        }

        ~TCPClient()
        {
            stream.Close();
            sender.Close();
        }

        public void Send(string message)
        {
            streamWriter.WriteLine(message);
            streamWriter.Flush();
        }

        public bool MessageIsAvailable()
        {
            return stream.DataAvailable;
        }

        public string Receive()
        {
            string data = streamReader.ReadLine();
            return data;
        }

        public StreamWriter GetNetworkStreamWriter()
        {
            return streamWriter;
        }

        public StreamReader GetNetworkStreamReader()
        {
            return streamReader;
        }
    }
}