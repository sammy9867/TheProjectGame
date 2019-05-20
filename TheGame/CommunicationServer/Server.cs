using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationServer
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 2048;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class Server
    {
        // Thread signal.
        public static ManualResetEvent allDone
            = new ManualResetEvent(false);

        private const char ETB = (char)23;
        private const int PORT = 11000;

        private static Socket GMSocket;
        private static Dictionary<String, Socket> Clients;

        public static void StartListening()
        {
            // Establish the local endpoint for the socket
            IPAddress ipAddress = IPAddress.Loopback;

            //Console.WriteLine("Communication Server IP: " + ipAddress.ToString());
            //Console.WriteLine("Communication Server PORT: " + PORT);
            //            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Console.WriteLine("Communication Server IP: " + aIP_ADDRESS);
            Console.WriteLine("Communication Server PORT: " + aPORT);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(aIP_ADDRESS), Int32.Parse(aPORT));

            // Create TCP/IP socket.
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Setting socket options (WHY??)
            listener.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.SendTimeout, 1000);
            listener.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.Linger, new LingerOption(true, 10));

            // Bind the socket to the local endpoint and listen for incoming
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100); // Maximum length of pending connections qeueu

                while (true)
                {
                    // Set event to nonsignaled state 
                    allDone.Reset();

                    // Start an asynch socket to listen for connections
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue. 
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            //First Start CS => start GM 
            //After Accepting Connection from GM, 
            //RequestHandler.sendConfirmGame(handler);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrive the state object and the handler socket
                // from the asynchronous state object
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                int bytesRead = handler.EndReceive(ar);


                if (bytesRead > 0)
                {
                    // There  might be more data, 
                    // so store the data received so far. 
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for End-Transmission-Block
                    // If it is not there, read more data
                    content = state.sb.ToString();
                    if (content.IndexOf(ETB) > -1)
                    {
                        /* Actual Work on Received message */
                        content = content.Remove(content.IndexOf(ETB));
                        //Console.WriteLine("Read {0} bytes from socket. \nData:\n{1}",
                        //    content.Length, content);

                        AnalizeTheMessage(content, state);


                        // Clear the state object and receive a new message   
                        state.sb.Clear();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data + (char)23);
//            Console.WriteLine("Send data : " + data);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
//                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static string aIP_ADDRESS = null;
        public static string aPORT = null;
        public static int Main(string[] args)
        {
            Console.WriteLine("Communication Server has started");

            Console.WriteLine("0 " + args[0]);
            Console.WriteLine("1 " + args[1]);
            aIP_ADDRESS = args[0];
            aPORT = args[1];
            Console.WriteLine("Start Listening...");
            Clients = new Dictionary<string, Socket>();
            StartListening();

            Console.WriteLine("Communication Server has done its job.");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            return 0;
        }


        private static void AnalizeTheMessage(string json, StateObject state)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string action = magic.action;
            string result = magic.result;
            string userGuid = magic.userGuid;
            List<String> listOfGuids = new List<string>(); 

            string direction = magic.direction;

            switch (action)
            {
                case "start":
                    {
                        if(userGuid == null || userGuid == "")
                        {
                            GMSocket = state.workSocket;
                            CSRequestHandler.SendConfirmGame(state.workSocket);
                            break;
                        }
                        else /* case "begin": */
                        {
                            Socket destPlayer = null;
                            if (Clients.TryGetValue(userGuid, out destPlayer))
                            {
                                CSRequestHandler.BeginPlayer(json, destPlayer);
                                //                            Clients.Remove(userGuid);
                            }
                            break;
                        }
                    }
                case "connect":
                    {
                        if (result == null)
                        {
                            // Player to GM
                            Clients.Add(userGuid, state.workSocket); // add player's socket
                            listOfGuids.Add(userGuid);
                            CSRequestHandler.ConnectPlayer(state.sb.ToString(), GMSocket);
                        }
                        else
                        {
                            // GM to Player
                            Socket destPlayer = null;
                            if (Clients.TryGetValue(userGuid, out destPlayer))
                            {
                                CSRequestHandler.ConnectPlayerConfirmation(state.sb.ToString(), destPlayer);
                            }
                        }
                        break;
                    }
               

                case "move":
                case "state":
                case "pickup":
                case "test":
                case "destroy":
                case "place":
                    {
                        if (GMSocket == state.workSocket)
                        {
                            // Forward Message from GM to player
                            Console.WriteLine("Forward "+action+" Message GM -> Player");
                            Socket destPlayer = null;
                            if (Clients.TryGetValue(userGuid, out destPlayer))
                            {
                                Send(destPlayer, state.sb.ToString());
                                Console.WriteLine(" "+action + "  "+ userGuid);
                            }
                            else
                                Console.WriteLine("404 Player not found\n" + userGuid);
                            Console.WriteLine();
                        }
                        else
                        {
                            // Forward Message from player to GM
                            Console.WriteLine("Forward " + action + " Message Player -> GM");
                            Console.WriteLine(" " + action + "  " + userGuid+"\n");
                            Send(GMSocket, state.sb.ToString());
                        }
                        break;
                    }
                case "send":
                case "exchange":
                    {
                        if (GMSocket == state.workSocket)
                        {
                            // Forward Message from GM to player
                            Console.WriteLine("Forward " + action + " Message GM -> Receiver-Player");
                            Socket destPlayer = null;
                            string receiverGuid = magic.receiverGuid;
                            if (Clients.TryGetValue(receiverGuid, out destPlayer))
                            {

                                Send(destPlayer, state.sb.ToString());
                                Console.WriteLine(" " + action + "  " + receiverGuid);
                            }
                            else
                                Console.WriteLine("404 Player not found\n" + receiverGuid);
                            Console.WriteLine();
                        }
                        else
                        {
                            // Forward Message from player to GM
                            Console.WriteLine("Forward " + action + " Message Player -> GM");
                            Console.WriteLine(" " + action + "  " + userGuid + "\n");
                            Send(GMSocket, state.sb.ToString());
                        }
                        break;
                    }

                case "end":
                    {
                        if (GMSocket == state.workSocket)
                        {
                            // Forward Message from GM to player
                            Console.WriteLine("Forward " + action + " Message GM -> Players");
                           
                            foreach (Socket destPlayer in Clients.Values)
                            {
                                Send(destPlayer, state.sb.ToString());
                                Console.WriteLine("the end ");
                            }
                        }
                        break;
                    }

                default:
                    Console.WriteLine("Error");
                    Console.WriteLine("Unexpected action in the message");
                    break;
            }
        }

    }
}
