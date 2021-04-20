using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DBConnect connection = new DBConnect();
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server
            Console.WriteLine("Server Started: " + server.LocalEndpoint);
            Console.WriteLine("Waiting for clients to connect.");


            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                while (client.Connected)  //while the client is connected, we look for incoming messages
                {
                    Byte[] bytes = new Byte[256];
                    int i;

                    while ((i = ns.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        string hex = BitConverter.ToString(bytes);
                        string data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine(data);
                        IDictionary<string, object> p = new Dictionary<string, object>();
                        //p.Add("@link", link);

                        connection.runQueryAsync("INSERT INTO dbo.Message(userID, username, messageType, messageTime) VALUES (@userID, @username, @messageType, @messageTime)", p);
                    }
                }
            }
        }
    }
}
