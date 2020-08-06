using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Flintr_lib.Communication
{
    public static class ApplicationLayer
    {
        public static readonly int DataWaitTimeout = 10000;
        private static readonly byte[] validMsgHeader = Encoding.ASCII.GetBytes("MSG|");
        private static readonly int validMsgHeaderBlockLength = validMsgHeader.Length;

        public static byte[] GenerateMessageHeader(int messageLength)
        {
            return Encoding.ASCII.GetBytes($"MSG|{messageLength}\n");
        }
        
        /// <summary>
        /// Retrieves the body of a message from a stream using the content length data provided in to pre-processed header.
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        /// <param name="contentLength">Length of message body as reported by the message header.</param>
        /// <returns>Byte array of complete message.</returns>
        public static byte[] GetMessageBody(NetworkStream stream, int contentLength)
        {
            byte[] msgBuffer = new byte[contentLength];

            try
            {
                readNBytesFromStream(stream, ref msgBuffer);
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while reading a message body from the the underlying NetworkStream", e);
            }

            return msgBuffer;
        }
        
        /// <summary>
        /// Parses a header message from a data stream and retrieves the reported message body length.
        /// </summary>
        /// <param name="stream">Stream to read header information from.</param>
        /// <returns>Reported size of message body from header data.</returns>
        public static int GetMessageLengthFromHeader(NetworkStream stream)
        {
            byte[] headerDeclaration = new byte[validMsgHeaderBlockLength];
            try
            {
                readNBytesFromStream(stream, ref headerDeclaration);
            }
            catch (Exception e)
            {
                throw new IOException("An error occurred while reading a message header in the underlying network stream.", e);
            }

            if (!compareByteBuffers(validMsgHeader, headerDeclaration)) throw new IOException("Malformed MSG received through TCP channel.");

            string msgLengthAttribute = "";
            try
            {
                msgLengthAttribute = readUntilTerminationCharacter(stream, '\n');
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
        private static string readUntilTerminationCharacter(NetworkStream stream, char terminationCharacter)
        {
            string msgLengthAttribute = "";
            byte[] lengthBuffer = new byte[1];
            while (true)
            {
                try
                {
                    readNBytesFromStream(stream, ref lengthBuffer);
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
        private static void readNBytesFromStream(NetworkStream stream, ref byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) throw new ArgumentException("Cannot read to null buffer or buffer of size 0.");

            int bytesRead = 0;
            int waitTimeInMilliseconds = 0;
            while (bytesRead != buffer.Length)
            {
                Thread.Sleep(100);
                waitTimeInMilliseconds += 100;
                if (waitTimeInMilliseconds > DataWaitTimeout) throw new TimeoutException($"No data recieved for over {waitTimeInMilliseconds / 100} seconds.");

                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                catch (ObjectDisposedException e)
                {
                    throw new IOException("The underlying NetworkStream was unexpectedly closed.", e);
                    // TODO: Attempt to perform a re-connect and try again.
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
            if (a == null || b == null) throw new ArgumentNullException("Cannot compare empty buffers.");

            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
