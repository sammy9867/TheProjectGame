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
        private int PORT;
        private IPAddress ipAddress;

        public Socket socket;

        PlayerSocket(int port, IPAddress ip)
        {
            this.PORT = port;
            this.ipAddress = ip;

            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));

            socket.BeginConnect(new IPEndPoint(ipAddress, PORT),
                new AsyncCallback(ConnectCallback), socket);
            
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Console.WriteLine("Socket connected");
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
