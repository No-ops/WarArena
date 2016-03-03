using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarArena.Repositories;
using WarArenaMasterServer.Models;
using WarArenaMasterServer.Repositories;

namespace WarArenaMasterServer
{
    /*

    War Arena Master Protocol (WAMP/1.0):

    Data format: Text.
    Character encoding: UTF8.
    Maximal message size: 200 bytes.


    ---------- SERVER -------------- 

    Request WAMP/1.0 ADD <servername> <port> <format>
    --- Request from server to master to add server to list of available servers
    --- <format> = t for text, o for object
    --- Servername MUST NOT contain blank spaces!
    --- Example: WAMP/1.0 ADD EriksCoolServer 8602 o

    Request WAMP/1.0 REMOVE <port>
    --- Request from server to master to remove server from list of available servers


    Response WAMP/1.0 ERROR_MESSAGE _____
    Response WAMP/1.0 MESSAGE _____
    --- '_____' = text message


    ---------- CLIENT --------------

    Request WAMP/1.0 AVAILABLE_SERVERS
    --- Ask master for list of available servers


    Response WAMP/1.0 SERVERLIST <name>,<ip>:<port>,<format>, <name>,<ip>:<port>,<format>, ...
    --- Return list of available servers to client,
    --- servers separated by ', '

    */

    class WarArenaMasterServer
    {
        static IServersRepository repository = new DbServersRepository();
        const int LISTENERBACKLOG = 100;
        const int BUFFERLENGTH = 500;
        const int PORT = 8002;
        static IPAddress ipAddress = IPAddress.Any;
        static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
        static UTF8Encoding encoding = new UTF8Encoding();
        static Socket listeningSocket = null;

        static string message = $"WAMP/1.0 MESSAGE ";
        static string errorMessage = $"WAMP/1.0 ERROR_MESSAGE ";
        static string serverlist = $"WAMP/1.0 SERVERLIST ";

        static void Main()
        {
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(localEndPoint);
                listeningSocket.Listen(LISTENERBACKLOG);
                Console.WriteLine("War Arena Master Server started.");
                while (true)
                {
                    Console.WriteLine($"Listening on {listeningSocket.LocalEndPoint}");
                    var newConnection = listeningSocket.Accept();
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(ConnectionHandler);
                    Thread thread = new Thread(pts);
                    thread.Start(newConnection);  
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.ReadLine();
            }
            finally
            {
                listeningSocket?.Close();
            }
        }

        public static void ConnectionHandler(object newConnection)
        {
            Socket connection = (Socket)newConnection;
            var buffer = new byte[BUFFERLENGTH];
            var bufferOut = new byte[BUFFERLENGTH];
            var bytesReceived = connection.Receive(buffer);
            var command = encoding.GetString(buffer, 0, bytesReceived);
            string[] parts = command.Split(' ');
            string response = null;
            var remoteIPEndPoint = connection.RemoteEndPoint as IPEndPoint;
            var ip = remoteIPEndPoint.Address.ToString();

            if (parts[0] != "WAMP/1.0" || parts.Length < 2)
                response = errorMessage + "Incorrect syntax";

            command.Substring(9);

            switch (parts[1])
            {
                case "ADD" :
                    {
                        var name = parts[2];
                        var port = parts[3];
                        var format = parts[4];
                        var model = repository.GetByIPAndPort(ip, port);

                        if (model != null)
                            response = message + $"Server {name} ({ip}:{port}) already exists in list of active servers";
                        else
                        {
                            model = new ServerModel
                            {
                                Name = name,
                                Ip = ip,
                                Port = port,
                                Format = format
                            };
                            repository.Add(model);
                            response = message + $"Server {name} ({ip}:{port}) added to list of active servers";
                        }
                    }
                    break;
                case "REMOVE" :
                    { 
                        var port = parts[2];
                        var model = repository.GetByIPAndPort(ip, port);
                        if (model == null)
                            response = message + $"No server with address {ip}:{port} listed as active server";
                        else
                        {
                            repository.Remove(model);
                            response = message + $"Server {model.Name} ({ip}:{port}) removed from list of active servers";
                        }
                    }
                    break;
                case "AVAILABLE_SERVERS":
                    {
                        response = serverlist;
                        var servers = repository.GetActiveServers();
                        foreach (var server in servers)
                        {
                            response += $"{server.Name},{server.Ip}:{server.Port},{server.Format}, ";
                        }
                        response.Remove(response.Length - 2);
                    }
                    break;
                default:
                    response = errorMessage + $"Unknown command {parts[1]}";
                    break; 
            }
            bufferOut = encoding.GetBytes(response);
            connection.Send(bufferOut);
            connection.Shutdown(SocketShutdown.Both);
        }
    }
}
 