using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WarArena;
using WwaLibrary;
using WwaLibrary.Utilities;
using WwaLibrary.World;

namespace WarArenaClient
{
    enum ServerResponse
    {
        YourTurn,
        Sendstate,
        NewPlayer,
        UpdatePlayer,
        RemovePlayer,
        UpdateTile,
        Message,
        Welcome,
        LoginDenied,
        MoveDenied
    }
    class WarArenaClient
    {
        const Int32 BUFFERLENGTH = 500;
        const String IPADDRESS = "127.0.0.1";
        //const String IPADDRESS = "10.56.5.232";
        const Int32 MASTERPORT = 8002;
        const Int32 PORT = 8001;
        static readonly IPAddress IpAddress = IPAddress.Parse(IPADDRESS);
        static readonly IPEndPoint MasterEndPoint = new IPEndPoint(IpAddress, MASTERPORT);
        static readonly IPEndPoint RemoteEndPoint = new IPEndPoint(IpAddress, PORT);
        static UTF8Encoding _encoding = new UTF8Encoding();
        private static Socket _masterSocket = null;
        static Socket _socket = null;

        static Tile[,] gameBoard = MapCreator.CreateEmptyMap();
        static List<Player> _players;
        static readonly IOHandler Handler = new IOHandler();
        static int? _playerId;
        static List<string> _chatMessages = new List<string>();
        static Queue<ServerResponse> _responseQueue = new Queue<ServerResponse>();
        static List<IPEndPoint> _serverList = new List<IPEndPoint>();
        static bool _receiveJson = false;

        static void ConnectToServer()
        {
            Handler.WriteLine("Welcome to WarArenaClient!");
            _masterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _masterSocket.Connect(MasterEndPoint);
            Handler.WriteLine("fetching info...");
            string messageToMaster = "WAMP/1.0 AVAILABLE_SERVERS";
            byte[] bytes = Encoding.UTF8.GetBytes(messageToMaster);
            _masterSocket.Send(bytes);
            bytes = new byte[BUFFERLENGTH];
            _masterSocket.Receive(bytes);
            string masterMessage = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            string[] masterMessageParts = masterMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (masterMessageParts[1] == "SERVERLIST")
            {
                for (int i = 2; i < masterMessageParts.Length; i++)
                {
                    string[] serverInfo = masterMessageParts[i].Split(new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries);
                    string[] ipAndPort = serverInfo[1].Split(new char[] { ':' },
                        StringSplitOptions.RemoveEmptyEntries);
                    _serverList.Add(new IPEndPoint(IPAddress.Parse(ipAndPort[0]), int.Parse(ipAndPort[1])));
                    Handler.WriteLine($"({i - 1}) {serverInfo[0]} {ipAndPort[0]}:{ipAndPort[1]}");
                }
                Handler.WriteLine($"Please press the corresponing number key to connect to a server: ");
                ConsoleKeyInfo info;
                int serverNumber;
                bool isNumber;
                bool isValid;
                do
                {
                    info = Handler.ReadKey();
                    isNumber = int.TryParse(info.KeyChar.ToString(), out serverNumber);
                    isValid = serverNumber != 0 && serverNumber <= _serverList.Count;
                } while (!isNumber || !isValid);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_serverList[serverNumber - 1]);
                Handler.WriteLine("Connected to server.");
                Handler.WriteLine("Local end point: " + _socket.LocalEndPoint);
                Handler.WriteLine("Remote end point: " + _socket.RemoteEndPoint);
                Handler.WriteLine("Press any key to login");
                Handler.ReadKey();
            }
            else
            {
                Console.WriteLine("No servers found...");
                Console.ReadKey();
            }


        }

        static void SendLoginRequest(string name, string password)
        {
            string request = $"WAP/1.0 LOGIN {name} {password}";
            byte[] bytes = Encoding.UTF8.GetBytes(request);
            _socket.Send(bytes);
            //bytes = new byte[BUFFERLENGTH];
            //socket.Receive(bytes);
            //string response = Encoding.UTF8.GetString(bytes);
            //string[] responseParts = response.Split(' ');
            //if (responseParts[0] == "WAP/1.0" && responseParts[1] == "DENIED")
            //{
            //    Console.WriteLine("Wrong password");
            //    Console.ReadKey();
            //    return false;
            //}
            //return true;
        }

        public static LoginData Login()
        {
            LoginData data = new LoginData();

            string name = "";
            do
            {
                Handler.Clear();
                Handler.WriteLine("Login: ");
                Handler.Write($"Please enter your name: ");
                name = Handler.ReadString();
            } while (!Validator.HasMinLength(name, 3));

            data.Name = name;
            Handler.Write("Enter a password for your character:");
            data.Password = Handler.ReadString();
            return data;
        }

        static void Main(string[] args)
        {
            try
            {
                ConnectToServer();
                LoginData data = Login();
                SendLoginRequest(data.Name, data.Password);
                Handler.Clear();
                while (true)
                {
                    RecieveResponse();
                    while (_responseQueue.Count != 0)
                    {
                        ServerResponse response = _responseQueue.Dequeue();
                        switch (response)
                        {
                            case ServerResponse.YourTurn:
                                Handler.ClearLine(0, gameBoard.GetLength(1) + _players.Count);
                                Handler.Write("Your turn. Press Arrow keys to move or press (c) to chat", 0, gameBoard.GetLength(1) + _players.Count);
                                bool ok = false;
                                while (!ok)
                                {
                                    ok = SendMoveRequest();
                                }
                                Handler.ClearLine(0, gameBoard.GetLength(1) + _players.Count);
                                Handler.Write("Waiting for other players to move", 0, gameBoard.GetLength(1) + _players.Count);
                                break;
                            case ServerResponse.Sendstate:
                                Display();
                                break;
                            case ServerResponse.NewPlayer:
                                Display();
                                break;
                            case ServerResponse.UpdatePlayer:
                                Display();
                                break;
                            case ServerResponse.RemovePlayer:
                                Display();
                                break;
                            case ServerResponse.Message:
                                ClearChattMessages();
                                PrintChattMessages();
                                break;
                            case ServerResponse.LoginDenied:
                                Handler.Clear();
                                Handler.WriteLine("Wrong password!");
                                Handler.ReadKey();
                                data = Login();
                                SendLoginRequest(data.Name, data.Password);
                                break;
                            case ServerResponse.MoveDenied:
                                Handler.ClearLine(0, gameBoard.GetLength(1) + _players.Count);
                                Handler.Write("Your turn", 0, gameBoard.GetLength(1) + _players.Count);
                                ok = false;
                                while (!ok)
                                {
                                    ok = SendMoveRequest();
                                }
                                break;
                        }
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
                _socket?.Close();
            }
            Console.WriteLine("Client shut down.");
            Console.ReadLine();
        }

        static void RecieveResponse()
        {
            if (_players == null)
            {
                _players = new List<Player>();
            }
            byte[] bytes = new byte[BUFFERLENGTH];
            _socket.Receive(bytes);
            string fromServer = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            string[] responses = fromServer.Split(';');
            foreach (string response in responses)
            {
                string[] responseParts = response.Split(' ');
                if (responseParts[0] == "WAP/1.0")
                {
                    if (responseParts[1] == "WELCOME")
                    {
                        _receiveJson = responseParts[2] == "JSON";
                    }

                    if (responseParts[1] == "YOURTURN")
                    {
                        if (!_playerId.HasValue)
                        {
                            _playerId = int.Parse(responseParts[2]);
                        }
                        _responseQueue.Enqueue(ServerResponse.YourTurn);
                    }

                    if (responseParts[1] == "SENDSTATE")
                    {
                        if (_receiveJson)
                        {
                            var sendState = SerializationFunctions.DeserializeObject<SendState>(responseParts[2]);
                            foreach (Player player in sendState.Players)
                            {
                                Player existingPlayer = _players.SingleOrDefault(p => p.PlayerId == player.PlayerId);
                                if (existingPlayer == null)
                                {
                                    _players.Add(new Player(player.PlayerId, player.Name, player.Health, 10, player.Gold, player.Coordinates));
                                }
                                else
                                {
                                    existingPlayer.Gold = player.Gold;
                                    existingPlayer.Health = player.Health;
                                    existingPlayer.Coordinates = player.Coordinates;
                                }
                            }

                            foreach (Tile tile in sendState.Tiles)
                            {
                                gameBoard[tile.X, tile.Y].Gold = tile.Gold;
                                gameBoard[tile.X, tile.Y].Health = tile.Health;
                            }
                        }
                        else
                        {
                            for (int i = 2; i < responseParts.Length; i++)
                            {
                                string[] itemInfos = responseParts[i].Split(',');
                                switch (itemInfos[0])
                                {
                                    case "Pl":
                                        string name = itemInfos[1];
                                        int playerId = int.Parse(itemInfos[2]);
                                        Coords coordinates = new Coords(int.Parse(itemInfos[3]), int.Parse(itemInfos[4]));
                                        int health = int.Parse(itemInfos[5]);
                                        int gold = int.Parse(itemInfos[6]);
                                        Player player = _players.SingleOrDefault(p => p.PlayerId == playerId);
                                        if (player == null)
                                        {
                                            player = new Player(playerId, name, health, 10, gold, coordinates);
                                            _players.Add(player);
                                        }
                                        else
                                        {
                                            player.Health = health;
                                            player.Gold = gold;
                                            player.Coordinates = coordinates;
                                        }
                                        break;
                                    case "P":
                                        coordinates = new Coords(int.Parse(itemInfos[1]), int.Parse(itemInfos[2]));
                                        health = int.Parse(itemInfos[3]);
                                        gameBoard[coordinates.X, coordinates.Y].Health = health;
                                        break;
                                    case "G":
                                        coordinates = new Coords(int.Parse(itemInfos[1]), int.Parse(itemInfos[2]));
                                        gold = int.Parse(itemInfos[3]);
                                        gameBoard[coordinates.X, coordinates.Y].Gold = gold;
                                        break;
                                }
                            }

                        }
                        _responseQueue.Enqueue(ServerResponse.Sendstate);
                    }

                    if (responseParts[1] == "NEWPLAYER")
                    {
                        if (_receiveJson)
                        {
                            var player = SerializationFunctions.DeserializeObject<Player>(responseParts[2]);
                            _players.Add(new Player(player.PlayerId, player.Name, player.Health, 10, player.Gold, player.Coordinates));
                        }
                        else
                        {
                            string[] playerInfos = responseParts[2].Split(',');
                            string name = playerInfos[0];
                            int id = int.Parse(playerInfos[1]);
                            Coords coordinates = new Coords(int.Parse(playerInfos[2]), int.Parse(playerInfos[3]));
                            int health = int.Parse(playerInfos[4]);
                            int gold = int.Parse(playerInfos[5]);
                            var player = new Player(id, name, health, 10, gold, coordinates);
                            player.PlayerId = id;
                            _players.Add(player);
                        }

                        _responseQueue.Enqueue(ServerResponse.NewPlayer);
                    }

                    if (responseParts[1] == "UPDATEPLAYER")
                    {
                        Player playerToUpdate;
                        if (_receiveJson)
                        {
                            var player = SerializationFunctions.DeserializeObject<Player>(responseParts[2]);
                            playerToUpdate = _players.Single(p => p.PlayerId == player.PlayerId);
                            playerToUpdate.Coordinates = player.Coordinates;
                            playerToUpdate.Gold = player.Gold;
                            playerToUpdate.Health = player.Health;
                        }
                        else
                        {
                            string[] playerInfos = responseParts[2].Split(',');
                            int id = int.Parse(playerInfos[0]);
                            Coords coordinates = new Coords(int.Parse(playerInfos[1]), int.Parse(playerInfos[2]));
                            int health = int.Parse(playerInfos[3]);
                            int gold = int.Parse(playerInfos[4]);
                            playerToUpdate = _players.Single(p => p.PlayerId == id);
                            playerToUpdate.Coordinates = coordinates;
                            playerToUpdate.Health = health;
                            playerToUpdate.Gold = gold;
                        }
                        
                        _responseQueue.Enqueue(ServerResponse.UpdatePlayer);
                    }

                    if (responseParts[1] == "REMOVEPLAYER")
                    {
                        int id = int.Parse(responseParts[2]);
                        Player playerToRemove = _players.SingleOrDefault(p => p.PlayerId == id);
                        if (playerToRemove != null)
                        {
                            _players.Remove(playerToRemove);
                        }
                        _responseQueue.Enqueue(ServerResponse.RemovePlayer);
                    }

                    if (responseParts[1] == "UPDATETILE")
                    {
                        if (_receiveJson)
                        {
                            var tile = SerializationFunctions.DeserializeObject<Tile>(responseParts[2]);
                            Tile tileToUpdate = gameBoard[tile.X, tile.Y];
                            tileToUpdate.Gold = tile.Gold;
                            tileToUpdate.Health = tile.Health;
                        }
                        else
                        {
                            string[] tileInfos = responseParts[2].Split(',');
                            Coords coordinates = new Coords(int.Parse(tileInfos[0]), int.Parse(tileInfos[1]));
                            int gold = int.Parse(tileInfos[2]);
                            int health = int.Parse(tileInfos[3]);
                            gameBoard[coordinates.X, coordinates.Y].Gold = gold;
                            gameBoard[coordinates.X, coordinates.Y].Health = health;
                        }                      
                        _responseQueue.Enqueue(ServerResponse.UpdateTile);
                    }

                    if (responseParts[1] == "MESSAGE")
                    {
                        int id = int.Parse(responseParts[2]);
                        string name = _players.SingleOrDefault(p => p.PlayerId == id)?.Name;
                        if (_chatMessages.Count == 5)
                        {
                            _chatMessages.RemoveAt(0);
                        }
                        var messageBuilder = new StringBuilder();
                        messageBuilder.Append($"{name}: ");
                        for (int i = 3; i < responseParts.Count(); i++)
                        {
                            messageBuilder.Append($"{responseParts[i]} ");
                        }
                        _chatMessages.Add(messageBuilder.ToString());
                        _responseQueue.Enqueue(ServerResponse.Message);
                    }

                    if (responseParts[1] == "DENIED")
                    {
                        if (responseParts[2] == "LOGIN")
                        {
                            _responseQueue.Enqueue(ServerResponse.LoginDenied);
                        }
                        else if (responseParts[2] == "MOVE")
                        {
                            _responseQueue.Enqueue(ServerResponse.MoveDenied);
                        }
                    }
                }
            }
        }

        static bool SendMoveRequest()
        {
            ConsoleKeyInfo info;
            StringBuilder request;
            do
            {
                request = new StringBuilder();
                request.Append("WAP/1.0 ");
                info = Handler.ReadKey();
                switch (info.Key)
                {
                    case ConsoleKey.UpArrow:
                        request.Append("MOVE UP");
                        break;
                    case ConsoleKey.DownArrow:
                        request.Append("MOVE DOWN");
                        break;
                    case ConsoleKey.LeftArrow:
                        request.Append("MOVE LEFT");
                        break;
                    case ConsoleKey.RightArrow:
                        request.Append("MOVE RIGHT");
                        break;
                    case ConsoleKey.C:
                        Handler.Write("message: ", gameBoard.GetLength(0), _players.Count + _chatMessages.Count);
                        string message = Handler.ReadString();
                        request.Append($"MESSAGE {message}");
                        Handler.ClearLine(gameBoard.GetLength(0), _players.Count + _chatMessages.Count);
                        byte[] buffer = Encoding.UTF8.GetBytes(request.ToString());
                        _socket.Send(buffer);
                        break;
                }
            } while (info.Key == ConsoleKey.C);
            if (request.Length >= 14)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(request.ToString());
                _socket.Send(bytes);
                return true;
            }

            return false;
        }

        public static void Display()
        {
            ClearBoard();
            PrintBoard();
            ClearHealthBars();
            foreach (var currentPlayer in _players)
            {
                PrintHealthBar(currentPlayer.PlayerId, currentPlayer.Health);
            }
            ClearPlayerStats();
            PrintPlayerStats();
        }

        static void PrintPlayerStats()
        {
            Handler.ChangeTextColor("White");
            for (int i = 0; i < _players.Count; i++)
            {
                Handler.Write($"{_players[i].Name} Gold: {_players[i].Gold}", 0, gameBoard.GetLength(1) + i);
            }
        }

        static void PrintHealthBar(int playerId, int health)
        {
            //Handler.ChangeTextColor("Black");
            //Handler.SetCursorPosition(gameBoard.GetLength(0), playerId);

            //for (int i = 0; i < 10; i++)
            //{
            //    Handler.Write("");
            //}

            if (playerId == 1)
            {
                Handler.ChangeTextColor("Red");
            }
            else if (playerId == 2)
            {
                Handler.ChangeTextColor("Blue");
            }
            else if (playerId == 3)
            {
                Handler.ChangeTextColor("Green");
            }
            else if (playerId == 4)
            {
                Handler.ChangeTextColor("Yellow");
            }
            else if (playerId == 5)
            {
                Handler.ChangeTextColor("Orange");
            }

            Handler.SetCursorPosition(gameBoard.GetLength(0), playerId);

            for (int i = 0; i < health / 10; i++)
            {
                Handler.WriteBlock(Player.PlayerColors[playerId]);
            }
        }

        public static void PrintBoard()
        {
            for (int y = 0; y < gameBoard.GetLength(1); y++)
            {
                for (int x = 0; x < gameBoard.GetLength(0); x++)
                {
                    PrintTile(gameBoard[x, y]);
                }
            }

            foreach (var player in _players)
            {
                PrintPlayer(player);
            }
        }

        static void PrintTile(Tile tile)
        {
            Handler.SetCursorPosition(tile.X, tile.Y);
            if (tile.HasGold)
            {
                Handler.ChangeTextColor("Yellow");
                Handler.Write('*');
            }
            else if (tile.HasHealth)
            {
                Handler.ChangeTextColor("Red");
                Handler.Write("P");
            }
            else
            {
                Handler.ChangeTextColor(tile.Color);
                Handler.Write(tile.ImageCharacter);
            }
        }

        static void PrintPlayer(Player player)
        {
            Handler.SetCursorPosition(player.Coordinates);
            Handler.ChangeTextColor(player.PlayerColor);
            Handler.Write("@");
        }

        static void ClearBoard()
        {
            Handler.ClearArea(0, 0, gameBoard.GetLength(0) - 1, gameBoard.GetLength(1) - 1);
        }

        static void ClearHealthBars()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                Handler.ClearLine(gameBoard.GetLength(0), i);
            }
        }

        static void ClearPlayerStats()
        {
            Handler.ChangeTextColor("Black");
            for (int i = 0; i < _players.Count; i++)
            {
                Handler.Write($"{_players[i].Name} Gold: {_players[i].Gold}", 0, gameBoard.GetLength(1) + i);
            }
        }

        static void PrintChattMessages()
        {
            for (int i = 0; i < _chatMessages.Count; i++)
            {
                Handler.Write(_chatMessages[i], gameBoard.GetLength(0), _players.Count + i);
            }
        }

        static void ClearChattMessages()
        {
            for (int i = 0; i < _chatMessages.Count; i++)
            {
                Handler.ClearLine(gameBoard.GetLength(0), _players.Count + i);
            }
        }
    }
}

