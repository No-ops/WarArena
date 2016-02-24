using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.World;
using WarArena.Models;
using WarArena.Repositories;

namespace WarArena
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    // War Arena Protocol (WAP/1.0):

    // Data format: Text.
    // Character encoding: UTF8.
    // Maximal message size: 500 bytes.

    // Request WAP/1.0 LOGIN <username> <password>

    // Respons WAP/1.0 SENDSTATE Pl,Name,Id,X,Y,h,g P,X,Y,h G,X,Y,g
    // där Pl är spelare, Name är namn, Id är PlayerId, h är health, G är guld och g är mängden guld,
    // och P är potion.

    // Respons WAP/1.0 NEWPLAYER Name,Id,X,Y,h,g

    // Respons WAP/1.0 REMOVEPLAYER Id

    // Respons WAP/1.0 UPDATEPLAYER Id,X,Y,h,g

    // Respons WAP/1.0 UPDATEGOLD X,Y,g

    // Respons WAP/1.0 UPDATEPOTION X,Y,h

    // Respons WAP/1.0 YOURTURN

    // Respons WAP/1.0 DENIED <COMMAND> (<MESSAGE>)

    //Request WAP/1.0 MOVE DIR där DIR kan vara UP, DOWN, LEFT or Right.

    //Request WAP/1.0 MESSAGE ____ där ____ är meddelandet.

    static class WWaServer
    {
        const Int32 LISTENERBACKLOG = 100;
        const Int32 BUFFERLENGTH = 500;
        const Int32 PORT = 8001;
        static Game _game = new Game();
        static IPAddress ipAddress = IPAddress.Any;
        static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
        static UTF8Encoding encoding = new UTF8Encoding();

        static Boolean ReceiveStartRequest(Socket socket, out Player player,
            out string message)
        {
            _game.GameMap = MapCreator.CreateEmptyMap();

            Byte[] bufferIn = new Byte[BUFFERLENGTH];
            socket.Receive(bufferIn);
            String request = encoding.GetString(bufferIn).TrimEnd('\0').Trim();

            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(threadId + ": Received request from " + socket.RemoteEndPoint + ": " + request);
            var tokens = request.Split(' ');
            bool ok = tokens[0] == "WAP/1.0" &&
                      tokens[1] == "LOGIN";
            string name = tokens[2];
            string password = tokens[3];
            IPlayersRepository repository = new DbPlayersRepository();
            PlayerModel model = repository.GetByName(name);
            var game = new Game();
            if (model == null) //Finns inte i databasen
            {
                player = new Player(name, 100, 10, 0, game.GetRandomFreeCoords());
                model = Initiator.Mapper.Map<PlayerModel>(player);
                model.Password = password;
                repository.Add(model);
                message = "En ny spelare har skapats";

            }
            else //Finns i databasen
            {
                if (model.Password != password)
                {
                    message = "Fel lösenord";
                    player = null;
                    return false;
                }
                player = Initiator.Mapper.Map<Player>(model);
                player.Coordinates = game.GetRandomFreeCoords();
                message = "En befintlig spelare har hämtats.";
            }
            return ok;
        }

        static bool ReceiveRequest(Socket socket)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var bufferIn = new byte[BUFFERLENGTH];
            socket.Receive(bufferIn);
            string request = encoding.GetString(bufferIn).TrimEnd('\0').Trim();
            Console.WriteLine(threadId + ": Received request from " + socket.RemoteEndPoint + ": " + request);
            string[] requestParts = request.Split(' ');
            if (requestParts[0] == "WAP/1.0" && requestParts[1] == "MOVE")
            {
                Player player = _game.Players.SingleOrDefault(p => p.Name == requestParts[2]);
                if (player == null)
                {
                    return false;
                }
                switch (requestParts[3])
                {
                    case "UP":
                        _game.TakeAction(player, ConsoleKey.UpArrow);
                        break;
                    case "DOWN":
                        _game.TakeAction(player, ConsoleKey.DownArrow);
                        break;
                    case "LEFT":
                        _game.TakeAction(player, ConsoleKey.LeftArrow);
                        break;
                    case "RIGHT":
                        _game.TakeAction(player, ConsoleKey.RightArrow);
                        break;
                }
                return true;
            }
            return false;
        }

        static void SendResponse(Socket socket, bool ok)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var responseBuilder = new StringBuilder();
            if (ok)
            {
                responseBuilder.Append("WAP/1.0 SENDSTATE ");
                foreach (Player player in _game.Players)
                {
                    responseBuilder.Append(
                        $"Pl,{player.Name},{player.PlayerId},{player.Coordinates.X},{player.Coordinates.Y},{player.Health},{player.Gold} ");
                }
                foreach (HealthPotion potion in _game.Potions)
                {
                    responseBuilder.Append(
                        $"P,{potion.Coordinates.X},{potion.Coordinates.Y},{potion.Health} ");
                }
                foreach (Tile tile in _game.GameMap)
                {
                    if (tile.HasGold)
                    {
                        responseBuilder.Append($"G,{tile.X},{tile.Y},{tile.Gold}");
                    }
                }
                string response = responseBuilder.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(response);
                socket.Send(bytes);
                Console.WriteLine(threadId + ": Sent response to " + socket.RemoteEndPoint + ": " + response);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes("BAD REQUEST");
                socket.Send(bytes);
            }
        }

        static void PlayWarArena(Object parameter)
        {
            Socket[] sockets = (Socket[])parameter;
            var players = new List<Player>();
            IPlayersRepository repository = new DbPlayersRepository();
        }

        static void SendStartResponse(Socket socket, string message)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string response = $"WAP/1.0 MESSAGE {message}";
            byte[] bytes = Encoding.UTF8.GetBytes($"WAP/1.0 MESSAGE {message}");
            socket.Send(bytes);
            Console.WriteLine(threadId + ": Sent response to " + socket.RemoteEndPoint + ": " + response);
        }

        static void Main()
        {
            Console.WriteLine("Initiating...");
            Initiator.AutoMapperConfig();
            Console.WriteLine("Creating new game...");
            var game = new Game();
            Console.WriteLine("Starting server...");
            Socket listeningSocket = null;
            var clients = new List<Client>();
            var messageQueue = new Queue<Message>();
            var unconfirmedConnections = new List<Socket>();
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(localEndPoint);
                listeningSocket.Listen(LISTENERBACKLOG);
                Console.WriteLine(": WWAServer is listening.");
                Console.WriteLine(": Local end point: " + listeningSocket.LocalEndPoint);
                while (true)
                {
                    // Check for dead connections
                    for (int i = 0; i < unconfirmedConnections.Count; i++)
                    {
                        if (IsDisconnected(unconfirmedConnections[i]))
                        {
                            Console.WriteLine($"{unconfirmedConnections[i].RemoteEndPoint} disconnected.");
                            unconfirmedConnections[i].Close();
                            unconfirmedConnections.RemoveAt(i--);
                        }
                    }

                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (IsDisconnected(clients[i].Socket))
                        {
                            Console.WriteLine($"{clients[i].Socket.RemoteEndPoint} ({clients[i].Player.Name}) disconnected.");
                            messageQueue.Enqueue(new Message($"REMOVEPLAYER {clients[i].Player.PlayerId}", Message.RecipientType.All, 0));
                            clients[i].Socket.Close();
                            clients.RemoveAt(i--);
                        }
                    }

                    // Check for new logins
                    if (clients.Count < 5)
                    {
                        for (int i = 0; i < unconfirmedConnections.Count; i++)
                        {
                            var connection = unconfirmedConnections[i];
                            if (connection.Available > 0)
                            {
                                var buffer = new byte[BUFFERLENGTH];
                                var bytesReceived = connection.Receive(buffer);
                                var command = encoding.GetString(buffer, 0, bytesReceived);
                                string[] parts;
                                // Ignore any command that is not LOGIN
                                if (command.StartsWith("WAP/1.0 LOGIN") && (parts = command.Split(' ')).Length == 4)
                                {
                                    
                                }
                            }
                        }
                    }

                    if (clients.Count < 5 && listeningSocket.Available != 0)
                    {
                        var newConnection = listeningSocket.Accept();
                        Console.WriteLine(": New connection accepted.");
                        Console.WriteLine(": Local end point: " + newConnection.LocalEndPoint);
                        Console.WriteLine(": Remote end point: " + newConnection.RemoteEndPoint);
                        unconfirmedConnections.Add(newConnection);
                    }
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

        private static bool IsDisconnected(Socket socket)
        {
            return socket.Available == 0 && socket.Poll(1000, SelectMode.SelectRead);
        }
    }
}
