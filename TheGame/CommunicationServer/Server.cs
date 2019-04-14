﻿using System;
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
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class Server
    {
        // Thread signal.
        public static ManualResetEvent allDone 
            = new ManualResetEvent(false);

        private const char ETB = (char)23;
        public const int PORT = 11000;


        public static void StartListening()
        {
            // Establish the local endpoint for the socket
            IPAddress ipAddress = IPAddress.Loopback;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            Console.WriteLine("> IP: " + ipAddress.ToString());

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

        /***
         * AccepCallback function accepts a client and notifies the main thread 
         */
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
                        Console.WriteLine("Read {0} bytes from socket. \nData : {1}",
                            content.Length, content);

                    //      Console.WriteLine("Player has connected");
                      
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
            Console.WriteLine("Send data : " + data);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("Communication Server has started");
            Console.WriteLine("Start Listening...");

            StartListening();

            Console.WriteLine("Communication Server has done its job.");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            return 0;
        }

    }
}
