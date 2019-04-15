using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThePlayers
{

    class PlayerSocket
    {
        private const int port = 11000;
        private const char ETB = (char)23;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static String response = String.Empty;

        public static Player Player;
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
              
                PlayerRequestHandler.sendJoinGame(server);
                Receive();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Receive(Action<string> cb = null)
        {
            try
            {
                StateObject state = new StateObject();
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

                int bytesRead = server.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb = state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    if (state.sb.ToString().IndexOf(ETB) < 0)
                    {
                        server.BeginReceive(state.buffer, /* bytesRead ? */0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                        return;
                    }
                }
                var content = state.sb.ToString();
                content = content.Remove(content.IndexOf(ETB), 1);
                Console.WriteLine("Read {0} bytes from socket. \nData : {1}",
                    content.Length, content);

                if (state.cb != null)
                {
                    state.cb(content);
                    state.cb = null;
                    receiveDone.Set();
                }
                else
                {
                    Console.WriteLine("Dint get shit");
//                    RequestHandler.handleRequest(content, server);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket handler, String data, Action<string> cb = null)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data + ETB);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

            Receive(cb);
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
