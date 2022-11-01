using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chat
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Runs the program as the server or client.
            if (args.Contains("-server"))
            {
                ServerStart();
            }
            else
            {
                Console.WriteLine("Client");
                //var client = new Client();
                ClientConnect("127.0.0.1");
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        static void ServerStart()
        {
            Console.WriteLine("Server");
            //var server = new Server();

            TcpListener server = null;
            try
            {
                var quit = false;
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] data = new Byte[256];
                TcpClient client = null;

                // Enter the listening loop.
                while (!quit)
                {
                    if (client == null)
                    {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also use server.AcceptSocket() here.
                        client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                    }

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        var message = "";

                        if (key.Key == ConsoleKey.I)
                        {
                            Console.Write("Insertion Mode>>");
                            message = Console.ReadLine();

                            // Translate the passed message into ASCII and store it as a Byte array.
                            byte[] msg = Encoding.ASCII.GetBytes(message);

                            // Send the message to the connected TcpServer.
                            stream.Write(msg, 0, msg.Length);
                        }
                    }
                    if (stream.DataAvailable)
                    {
                        // String to store the response ASCII representation.
                        string responseData = string.Empty;

                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(data, 0, data.Length);
                        responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        Console.WriteLine(responseData);
                    }

                    // Detect if client disconnected. Not very graceful but it is functional.
                    try
                    {
                        if (client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                quit = true;
                            }
                        }
                    } catch
                    {
                        Console.WriteLine("Application has been disconnected");
                        quit = true;
                    }

                    if (quit)
                    {
                        // Shutdown and end the connection
                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }
        }

        static void ClientConnect(string server)
        {
            try
            {
                var quit = false;
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;

                // Prefer using declaration to ensure the instance is Disposed later.
                TcpClient client = new TcpClient(server, port);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Client is connected to Server!");

                while (!quit)
                {
                    Byte[] data;
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        var message = "";
                        
                        if (key.Key == ConsoleKey.Escape)
                        {
                            quit = true;
                            Console.WriteLine("You hit Escape key to exit.");
                        }
                        else if (key.Key == ConsoleKey.I)
                        {
                            Console.Write("Insertion Mode>>");
                            message = Console.ReadLine();
                            if (message == "quit")
                            {
                                quit = true;
                                Console.WriteLine("You typed \"quit\" to exit.");
                            }
                            else
                            {

                                // Translate the passed message into ASCII and store it as a Byte array.
                                data = Encoding.ASCII.GetBytes(message);

                                // Send the message to the connected TcpServer.
                                stream.Write(data, 0, data.Length);
                            }
                        }
                    }
                    // Buffer to store the response bytes.
                    data = new Byte[256];

                    if (stream.DataAvailable)
                    {

                        // String to store the response ASCII representation.
                        string responseData = string.Empty;

                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(data, 0, data.Length);
                        responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        Console.WriteLine(responseData);
                    }

                    // Detect if client disconnected. Not very graceful but it is functional.
                    try
                    {
                        if (client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                quit = true;
                            }
                        }
                    }
                    catch
                    {
                        quit = true;
                    }
                }

                Console.WriteLine("Disconnected\nGood Bye!");

                // Explicit close is not necessary since TcpClient.Dispose() will be
                // called automatically.
                // stream.Close();
                // client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
