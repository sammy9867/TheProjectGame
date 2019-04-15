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

    public class GMSocket
    {

        private const int port = 11000;
        private const char ETB = (char)23;

        // ManualResetEvent instances signal completion.
        public ManualResetEvent connectDone =
            new ManualResetEvent(false);
        public ManualResetEvent sendDone =
            new ManualResetEvent(false);
        public ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        public string Response = "";
        public Socket socket;

        public void StartClient()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Loopback;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));

                socket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}\n",
                    client.RemoteEndPoint.ToString());

                connectDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Receive(Action<string> cb = null)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = socket;
                if (cb != null)
                    state.cb = cb;

                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                receiveDone.WaitOne();
                receiveDone.Reset();
                var content = state.sb.ToString();
                content = content.Remove(content.IndexOf(ETB));
                state.sb.Clear();
                if(content != null)
                    Response =  content;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Response = null;
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
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
                    if (content.IndexOf(ETB) > -1)
                    {
                        content = content.Remove(content.IndexOf(ETB));
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}\n",
                            content.Length, content);
                     //   RequestHandler.handleRequest(content, client);
                     //   state.sb.Clear();
                        receiveDone.Set();


                        //if (state.cb != null)
                        //{
                        //    state.cb(content);
                        //    state.cb = null;
                        //}
                        //Receive();
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

        public void Send(Socket handler, String data, Action<string> cb = null)
        {
            //cb = _cb;
            byte[] byteData = Encoding.ASCII.GetBytes(data + ETB);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

//            Receive(cb);
//            receiveDone.WaitOne();
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.\n", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
