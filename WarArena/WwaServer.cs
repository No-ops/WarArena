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

    namespace Natverk_Uppg15a
    {
        // War Arena Protocol (WAP/1.0):

        // Data format: Text.
        // Character encoding: UTF8.
        // Maximal message size: 200 bytes.

        // Request WAP/1.0 LOGIN <username> <password>)

        // Respons WAP/1.0 SENDSTATE Pl,Name,Id,X,Y,h,g P,X,Y,h G,X,Y,g
        //där Pl är spelare, Name är namn, Id är PlayerId, h är health, G är guld och g är mängden guld,
        //och P är potion.

        //Request WAP/1.0 MOVE DIR där DIR kan vara UP, DOWN, LEFT or Right.

            //Request WAP/1.0 MESSAGE ____ där ____ är meddelandet.

        static class WWaServer
        {
            const Int32 LISTENERBACKLOG = 100;
            const Int32 BUFFERLENGTH = 200;
            const String IPADDRESS = "127.0.0.1";
            //const String IPADDRESS = "10.56.5.232";
            const Int32 PORT = 8001;
            static Game _game = new Game();
            static IPAddress ipAddress = IPAddress.Parse(IPADDRESS);
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
                PlayerModel model = repository.GetByName("name");
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
                Player player1 = null;
                Player player2 = null;
                string message;
                Tile[,] board;

                try
                {
                    bool startOk1 = false;
                    bool startOk2 = false;
                    while (!startOk1)
                    {
                        startOk1 =
                        ReceiveStartRequest(sockets[0], out player1, out message);
                        SendStartResponse(sockets[0], message);
                    }
                                           
                    while (!startOk2)
                    {
                        startOk2 = ReceiveStartRequest(sockets[1], out player2, out message);
                        SendStartResponse(sockets[1], message);
                    }
                    _game.Players[0] = player1;
                    _game.Players[1] = player2;
                    _game.PlaceGold();
                    _game.CreatePotion();
                    foreach (Player player in _game.Players)
                    {
                        if (player.IsDead)
                            _game.RespawnPlayer(player);
                    }
                    foreach (Socket socket in sockets)
                    {
                        SendResponse(socket, true);
                    }

                    while (true)
                    {
                        _game.PlaceGold();
                        _game.CreatePotion();

                        // Player 1.
                        Boolean ok = false;
                        while (!ok)
                        {
                            ok = ReceiveRequest(sockets[0]);
                            SendResponse(sockets[0], ok);
                        }

                        // Player 2.
                        ok = false;
                        while (!ok)
                        {
                            ok = ReceiveRequest(sockets[1]);
                            SendResponse(sockets[1], ok);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception: " + exception.Message);
                    Console.WriteLine("Stack trace: " + exception.StackTrace);
                    Console.ReadLine();
                }
                finally
                {
                    sockets[0]?.Close();
                    sockets[1]?.Close();
                }
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
                Socket listeningSocket = null;
                Socket[] sockets = new Socket[2];
                try
                {
                    Int32 threadId = Thread.CurrentThread.ManagedThreadId;
                    listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listeningSocket.Bind(localEndPoint);
                    while (true)
                    {
                        // Player 1.
                        listeningSocket.Listen(LISTENERBACKLOG);
                        Console.WriteLine(threadId + ": BattlefieldServer is listening.");
                        Console.WriteLine(threadId + ": Local end point: " + listeningSocket.LocalEndPoint);
                        sockets[0] = listeningSocket.Accept();
                        Console.WriteLine(threadId + ": Connection accepted (player 1).");
                        Console.WriteLine(threadId + ": Local end point: " + sockets[0].LocalEndPoint);
                        Console.WriteLine(threadId + ": Remote end point: " + sockets[0].RemoteEndPoint);

                        // Player 2
                        listeningSocket.Listen(LISTENERBACKLOG);
                        Console.WriteLine(threadId + ": BattlefieldServer is listening.");
                        Console.WriteLine(threadId + ": Local end point: " + listeningSocket.LocalEndPoint);
                        sockets[1] = listeningSocket.Accept();
                        Console.WriteLine(threadId + ": Connection accepted (player 2).");
                        Console.WriteLine(threadId + ": Local end point: " + sockets[1].LocalEndPoint);
                        Console.WriteLine(threadId + ": Remote end point: " + sockets[1].RemoteEndPoint);

                        ParameterizedThreadStart threadStart = PlayWarArena;
                        Thread thread = new Thread(threadStart);
                        thread.Start(sockets);
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
        }
    }
}
