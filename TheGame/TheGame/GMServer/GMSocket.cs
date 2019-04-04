using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TheGame.GMServer
{
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public Action<string> cb = null;
    }

    class GMSocket
    {

        private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static String response = String.Empty;

        public static Socket server;

        public static void StartClient()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Loopback;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                server = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));

                server.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), server);
                connectDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Action<string> cb = null)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = server;
                if (cb != null)
                    state.cb = cb;

                server.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                if (bytesRead > 0)
                {
                    var content = state.sb.ToString();
                    if (content.IndexOf((char)23) > -1)
                    {
                        content = content.Remove(content.IndexOf((char)23));
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
                     //   RequestHandler.handleRequest(content, client);
                        state.sb.Clear();
                        receiveDone.Set();

                        if (state.cb != null)
                        {
                            state.cb(content);
                            state.cb = null;
                        }
                        Receive();
                    }
                    else
                    {
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket handler, String data, Action<string> cb = null)
        {
            //cb = _cb;
            byte[] byteData = Encoding.ASCII.GetBytes(data + (char)23);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

            Receive(cb);
            receiveDone.WaitOne();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
