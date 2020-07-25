using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Flintr_lib.Communication
{
    public class TCPClient
    {
        private TcpClient sender;
        private NetworkStream stream;

        //private StreamReader streamReader;
        private StreamWriter streamWriter;

        private int streamLocked;

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
            //streamReader = new StreamReader(stream);
            streamLocked = 0;
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
            //streamReader = new StreamReader(stream);
            streamLocked = 0;
        }

        public TCPClient(TcpClient newClientConnection)
        {
            sender = newClientConnection;
            stream = sender.GetStream();
            streamWriter = new StreamWriter(stream);
            //streamReader = new StreamReader(stream);
            streamLocked = 0;
        }

        ~TCPClient()
        {
            stream.Close();
            sender.Close();
        }

        public void Send(string message)
        {
            //streamWriter.WriteLine(message);
            //streamWriter.Flush();
            byte[] encodedMessage = Encoding.ASCII.GetBytes(message);
            writeBytesToStream(encodedMessage);
        }

        public void SendObject<T>(T obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            xmlSerializer.Serialize(ms, obj);
            writeBytesToStream(ms.ToArray());
        }

        public bool MessageIsAvailable()
        {
            return stream.DataAvailable;
        }

        public string Receive()
        {
            string result = Encoding.ASCII.GetString(readBytesFromStream().ToArray());
            return result;
        }

        public T ReceiveObject<T>()
        {
            // We have to use a MemoryStream because the NetworkStream can't tell us that
            // the object buffer is complete, so the XmlSerializer will hang if we just pass
            // the NetworkStream.
            MemoryStream ms = readBytesFromStream();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(ms);
        }

        private void writeBytesToStream(byte[] buffer)
        {
            lockStream();
            byte[] header = Encoding.ASCII.GetBytes($"MSG|{buffer.Length}\n");
            stream.Write(header, 0, header.Length);
            stream.Write(buffer, 0, buffer.Length);
            unlockStream();
        }

        private MemoryStream readBytesFromStream()
        {
            lockStream();
            while (!stream.DataAvailable)
            {
                Thread.Sleep(500);
            }
            Thread.Sleep(500);
            byte[] declTest = Encoding.ASCII.GetBytes("MSG|");
            byte[] headerDeclaration = new byte[declTest.Length];
            stream.Read(headerDeclaration, 0, declTest.Length);
            if (!compareByteBuffers(declTest, headerDeclaration)) throw new Exception("Malformed MSG received through TCP channel.");
            string msgLengthAttribute = "";
            byte[] lengthBuffer = new byte[1];
            while (true)
            {
                stream.Read(lengthBuffer, 0, 1);
                string s = Encoding.ASCII.GetString(lengthBuffer);
                if (s == "\n") break;
                else msgLengthAttribute += s;
            }
            int msgLength = Convert.ToInt32(msgLengthAttribute);
            byte[] msgBuffer = new byte[msgLength];
            int bytesRead = 0;
            while (bytesRead != msgLength)
            {
                bytesRead = stream.Read(msgBuffer, 0, msgLength);
            }
            unlockStream();
            MemoryStream ms = new MemoryStream(msgBuffer);
            return ms;
        }

        private void lockStream()
        {
            while (true)
            {
                if (0 == Interlocked.Exchange(ref streamLocked, 1))
                {
                    return;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void unlockStream()
        {
            Interlocked.Exchange(ref streamLocked, 0);
        }

        private bool compareByteBuffers(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}