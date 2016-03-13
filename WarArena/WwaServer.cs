using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WarArena.Utilities;
using WarArenaDbLibrary.Models;
using WarArenaDbLibrary.Repositories;
using WwaLibrary.Utilities;

namespace WarArena
{

    // War Arena Protocol (WAP/1.0):

    // Data format: Text.
    // Character encoding: UTF8.
    // Maximal message size: 500 bytes.

    // Request WAP/1.0 LOGIN <username> <password>

    // Respons WAP/1.0 WELCOME <protocol> där protocol är TEXT eller JSON

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

    // Respons WAP/1.0 MESSAGE <id> <message>

    static class WWaServer
    {
        static IPlayersRepository _repository = new DbPlayersRepository();
        public const Int32 LISTENERBACKLOG = 100;
        public const Int32 BUFFERLENGTH = 500;
        const Int32 PORT = 8001;
        const Int32 MASTERPORT = 8002;
        static IPAddress ipAddress = IPAddress.Any;
        static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
        public static UTF8Encoding encoding = new UTF8Encoding();

        static void Main()
        {
            Console.Write("Enter server name: ");
            var serverName = Console.ReadLine();
            if (serverName.Contains(' '))
                throw new Exception("Server name cannot contain spaces.");
            Console.Write("Use (T)ext or (J)son? ");
            ConsoleKey keyResponse;
            do
            {
                keyResponse = Console.ReadKey().Key;
            } while (keyResponse != ConsoleKey.T && keyResponse != ConsoleKey.J);
            var json = keyResponse == ConsoleKey.J;
            Console.WriteLine();
            Console.WriteLine("Initiating...");
            Initiator.AutoMapperConfig();
            Console.WriteLine("Creating new game...");
            var world = new WorldMap();
            Console.WriteLine("Starting server...");
            Socket listeningSocket = null;
            var clients = new List<Client>();
            var responseQueue = new Queue<Response>();
            var unconfirmedConnections = new List<Socket>();
            var currentPlayerIndex = 0;
            //try
            //{
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(localEndPoint);
            listeningSocket.Listen(LISTENERBACKLOG);
            Console.WriteLine("Informing master server...");
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(IPAddress.Loopback, MASTERPORT);
                NetHelpers.SendString($"WAMP/1.0 ADD {serverName} {PORT}", socket);
                socket.Shutdown(SocketShutdown.Both);
            }

            Console.WriteLine($"WWAServer is listening on {listeningSocket.LocalEndPoint}. Press Q to quit.");
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q)
                    break;
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
                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.REMOVEPLAYER, IdParam = clients[i].Player.PlayerId });
                        clients[i].Socket.Close();
                        clients.RemoveAt(i--);
                        if (clients.Count <= currentPlayerIndex)
                        {
                            currentPlayerIndex = 0;
                            UpdateMap(world, clients, responseQueue, json);
                        }
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
                            command = command.TrimEnd(';');
                            string[] parts;
                            // Ignore any command that is not LOGIN
                            if (command.StartsWith("WAP/1.0 LOGIN") && (parts = command.Split(' ')).Length == 4)
                            {
                                var name = parts[2];
                                var password = parts[3];
                                var model = _repository.GetByName(parts[2]);
                                Player player = null;
                                if (model == null) //Finns inte i databasen
                                {
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.WELCOME, Socket = connection });
                                    player = new Player(GetFirstFreeId(clients), name, 100, 10, 0, world.GetRandomFreeCoords(clients));
                                    model = Initiator.Mapper.Map<PlayerModel>(player);
                                    model.Password = password;
                                    model.LastLogin = DateTime.Now;
                                    _repository.Add(model);
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.NEWPLAYER, IdParam = player.PlayerId });
                                    clients.Add(new Client { Player = player, Socket = connection });
                                    unconfirmedConnections.RemoveAt(i--);
                                    if (clients.Count == 2)
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.YOURTURN, IdParam = clients[currentPlayerIndex].Player.PlayerId });
                                    Console.WriteLine($"{connection.RemoteEndPoint} created player {player.Name}");
                                }
                                else //Finns i databasen
                                {
                                    if (clients.Select(c => c.Player.Name).Contains(model.Name))
                                    {
                                        Console.WriteLine($"{connection.RemoteEndPoint} tried to log in with {model.Name} already in game");
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = connection, StringParam = "LOGIN Player already in game" });
                                    }
                                    else if (model.Password == password)
                                    {
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.WELCOME, Socket = connection });
                                        model.LastLogin = DateTime.Now;
                                        _repository.Update(model);
                                        player = Initiator.Mapper.Map<Player>(model);
                                        player.Coordinates = world.GetRandomFreeCoords(clients);
                                        player.PlayerId = GetFirstFreeId(clients);
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.NEWPLAYER, IdParam = player.PlayerId });
                                        clients.Add(new Client { Player = player, Socket = connection });
                                        unconfirmedConnections.RemoveAt(i--);
                                        if (clients.Count == 2)
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.YOURTURN, IdParam = clients[currentPlayerIndex].Player.PlayerId });
                                        Console.WriteLine($"{connection.RemoteEndPoint} logged in as {player.Name}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{connection.RemoteEndPoint} failed password for {model.Name}");
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = connection, StringParam = "LOGIN Wrong password" });
                                    }
                                }
                            }
                        }
                    }
                }

                // Accept new connections
                if (unconfirmedConnections.Count < 20 && listeningSocket.Poll(1000, SelectMode.SelectRead))
                {
                    var newConnection = listeningSocket.Accept();
                    Console.WriteLine($"New connection accepted from {newConnection.RemoteEndPoint}");
                    unconfirmedConnections.Add(newConnection);
                }

                // Get player commands
                foreach (var client in clients)
                {
                    if (client.Socket.Available > 0)
                    {
                        var buffer = new byte[BUFFERLENGTH];
                        var bytesReceived = client.Socket.Receive(buffer);
                        var command = encoding.GetString(buffer, 0, bytesReceived);
                        //Console.WriteLine($"{command} <- {client.Socket.RemoteEndPoint}");
                        command = command.TrimEnd(';');
                        string[] parts = command.Split(' ');
                        if (parts.Length < 2 || parts[0] != "WAP/1.0")
                            continue;
                        switch (parts[1])
                        {
                            case "MOVE":
                                if (clients.IndexOf(client) != currentPlayerIndex || clients.Count < 2)
                                {
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = client.Socket, StringParam = "MOVE Not your turn" });
                                }
                                else if (parts.Length < 3)
                                {
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = client.Socket, StringParam = "MOVE invalid direction" });
                                }
                                else
                                {
                                    var result = world.MovePlayer(client.Player, parts[2], clients);
                                    switch (result.MoveResult)
                                    {
                                        case MoveResult.Fail:
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = client.Socket, StringParam = "MOVE You cannot move that way" });
                                            break;
                                        case MoveResult.Success:
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATEPLAYER, IdParam = client.Player.PlayerId });
                                            break;
                                        case MoveResult.Gold:
                                        case MoveResult.Potion:
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATETILE, StringParam = EncodeTile(world, client.Player.Coordinates, json) });
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATEPLAYER, IdParam = client.Player.PlayerId });
                                            UpdateDbStats(_repository, client.Player);
                                            break;
                                        case MoveResult.Player:
                                            if (result.Player.Health <= 0)
                                            {
                                                world.GameMap[result.Player.Coordinates.X, result.Player.Coordinates.Y].Gold = result.Player.Gold;
                                                result.Player.Gold = 0;
                                                result.Player.Coordinates = world.GetRandomFreeCoords(clients);
                                                result.Player.Health = 100;
                                                result.Player.IsDead = false;
                                            }
                                            responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATEPLAYER, IdParam = result.Player.PlayerId });
                                            UpdateDbStats(_repository, result.Player);
                                            break;
                                    }
                                    if (result.MoveResult != MoveResult.Fail)
                                    {
                                        if (++currentPlayerIndex >= clients.Count)
                                        {
                                            currentPlayerIndex = 0;
                                            UpdateMap(world, clients, responseQueue, json);
                                        }
                                        responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.YOURTURN, IdParam = clients[currentPlayerIndex].Player.PlayerId });
                                    }
                                }
                                break;
                            case "MESSAGE":
                                if (parts.Length < 3)
                                {
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.DENIED, Socket = client.Socket, StringParam = "MESSAGE Malformed message" });
                                }
                                else
                                {
                                    responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.MESSAGE, IdParam = client.Player.PlayerId, StringParam = command.Substring("WAP/1.0 MESSAGE ".Length) });
                                }
                                break;
                        }
                    }
                }

                // Fire off responses
                while (responseQueue.Count > 0)
                {
                    var response = responseQueue.Dequeue();
                    switch (response.ResponseType)
                    {
                        case Response.MessageType.NEWPLAYER:
                            Response.SendNewPlayerResponses(clients, world, response.IdParam, json);
                            break;

                        case Response.MessageType.DENIED:
                            Response.SendDeniedResponse(clients, response.Socket, response.StringParam);
                            break;

                        case Response.MessageType.REMOVEPLAYER:
                            Response.SendRemovePlayerResponse(clients, response.IdParam);
                            break;

                        case Response.MessageType.UPDATETILE:
                            Response.SendUpdateTile(clients, response.StringParam);
                            break;

                        case Response.MessageType.YOURTURN:
                            Response.SendYourTurn(clients, response.IdParam);
                            break;

                        case Response.MessageType.UPDATEPLAYER:
                            Response.SendUpdatePlayer(clients, response.IdParam, json);
                            break;

                        case Response.MessageType.MESSAGE:
                            Response.SendMessage(clients, response.IdParam, response.StringParam);
                            break;

                        case Response.MessageType.WELCOME:
                            Response.SendWelcome(response.Socket, json);
                            break;
                    }
                }
            }
            // End while

            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine(exception.Message);
            //    Console.ReadLine();
            //}
            //finally
            //{
            //    listeningSocket?.Close();
            //}

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(IPAddress.Loopback, MASTERPORT);
                NetHelpers.SendString($"WAMP/1.0 REMOVE {PORT}", socket);
                socket.Shutdown(SocketShutdown.Both);
            }
        }

        private static void UpdateMap(WorldMap world, List<Client> clients, Queue<Response> responseQueue, bool json)
        {
            if (RandomizationFunctions.Chance(15))
            {
                var coords = world.PlaceGold(clients);

                responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATETILE, StringParam = EncodeTile(world, coords, json) });

            }

            if (RandomizationFunctions.Chance(95))
            {
                var coords = world.CreatePotion(clients);
                responseQueue.Enqueue(new Response { ResponseType = Response.MessageType.UPDATETILE, StringParam = EncodeTile(world, coords, json) });
            }
        }

        private static string EncodeTile(WorldMap world, Coords coords, bool json)
        {
            return json ? SerializationFunctions.SerializeObject(world.GameMap[coords.X, coords.Y]) : world.GameMap[coords.X, coords.Y].ToString();
        }

        private static void UpdateDbStats(IPlayersRepository repository, Player player)
        {
            var model = repository.GetByName(player.Name);
            Initiator.Mapper.Map(player, model);
            repository.Update(model);
        }

        private static int GetFirstFreeId(List<Client> clients)
        {
            int counter = -1;
            while (++counter < int.MaxValue)
            {
                if (!clients.Any(c => c.Player.PlayerId == counter))
                    return counter;
            }

            // If we're here something has gone very wrong
            throw new IndexOutOfRangeException("More clients than ints");
        }

        private static bool IsDisconnected(Socket socket)
        {
            return socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0;
        }
    }
}
