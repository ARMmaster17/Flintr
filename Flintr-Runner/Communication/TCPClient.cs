using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Flintr_Runner.Communication
{
    public class TCPClient
    {
        private TcpClient sender;
        private NetworkStream stream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private BinaryFormatter binaryFormatter;

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
            binaryFormatter = new BinaryFormatter();
        }

        public TCPClient(TcpClient newClientConnection)
        {
            sender = newClientConnection;
            stream = sender.GetStream();
            streamWriter = new StreamWriter(stream);
            streamReader = new StreamReader(stream);
            binaryFormatter = new BinaryFormatter();
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

        public void SendObject(object obj)
        {
            binaryFormatter.Serialize(streamWriter.BaseStream, obj);
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

        public object RecieveObject()
        {
            return binaryFormatter.Deserialize(streamReader.BaseStream);
        }

        public T RecieveObject<T>()
        {
            return (T)RecieveObject();
        }
    }
}