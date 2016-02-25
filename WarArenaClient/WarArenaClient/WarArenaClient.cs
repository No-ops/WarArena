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
using WwaLibrary.World;

namespace WarArenaClient
{
    [Flags]
    enum ServerResponse
    {
        YourTurn,
        Sendstate,
        NewPlayer,
        UpdatePlayer,
        RemovePlayer,
        UpdateTile,
        Message,
        Denied
    }
    class WarArenaClient
    {
        const Int32 BUFFERLENGTH = 200;
        const String IPADDRESS = "127.0.0.1";
        //const String IPADDRESS = "10.56.5.232";
        const Int32 PORT = 8001;
        static IPAddress ipAddress = IPAddress.Parse(IPADDRESS);
        static IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, PORT);
        static UTF8Encoding encoding = new UTF8Encoding();
        static Socket socket = null;

        static Tile[,] gameBoard = MapCreator.CreateEmptyMap();
        static List<Player> _players;
        static readonly IOHandler Handler = new IOHandler();
        static int? _playerId;
        static List<string> _chattMessages = new List<string>();
        static Queue<ServerResponse> _responseQueue = new Queue<ServerResponse>();

        static void ConnectToServer()
        {
            Console.WriteLine("Welcome to BattlefieldClient!");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEndPoint);
            Console.WriteLine("Connected to server.");
            Console.WriteLine("Local end point: " + socket.LocalEndPoint);
            Console.WriteLine("Remote end point: " + socket.RemoteEndPoint);
        }

        static bool SendLoginRequest(string name, string password)
        {
            string request = $"WAP/1.0 LOGIN {name} {password}";
            byte[] bytes = Encoding.UTF8.GetBytes(request);
            socket.Send(bytes);
            bytes = new byte[BUFFERLENGTH];
            socket.Receive(bytes);
            string response = Encoding.UTF8.GetString(bytes);
            string[] responseParts = response.Split(' ');
            if (responseParts[0] == "WAP/1.0" && responseParts[1] == "DENIED")
            {
                Console.WriteLine("Wrong password");
                Console.ReadKey();
                return false;
            }
            return true;
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
                bool ok = false;

                while (!ok)
                {
                    ok = SendLoginRequest(data.Name, data.Password);
                }

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
                                Handler.Write("Your turn", 0, gameBoard.GetLength(1) + _players.Count);
                                ok = false;
                                while (!ok)
                                {
                                    ok = SendMoveRequest();
                                }
                                Handler.ClearLine(0, gameBoard.GetLength(1) + _players.Count);
                                Handler.Write("Waiting for other players to move", 0, gameBoard.GetLength(1) + _players.Count);
                                break;
                            case ServerResponse.Sendstate | ServerResponse.UpdatePlayer | ServerResponse.RemovePlayer:
                                Display();
                                break;
                            case ServerResponse.Message:
                                ClearChattMessages();
                                PrintChattMessages();
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
                socket?.Close();
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
            socket.Receive(bytes);
            string fromServer = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            string[] responses = fromServer.Split(';');
            foreach (string response in responses)
            {
                string[] responseParts = response.Split(' ');
                if (responseParts[0] == "WAP/1.0")
                {
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

                        _responseQueue.Enqueue(ServerResponse.Sendstate);
                    }
                    if (responseParts[1] == "NEWPLAYER")
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
                        _responseQueue.Enqueue(ServerResponse.NewPlayer);
                    }
                    if (responseParts[1] == "UPDATEPLAYER")
                    {
                        string[] playerInfos = responseParts[2].Split(',');
                        int id = int.Parse(playerInfos[0]);
                        Coords coordinates = new Coords(int.Parse(playerInfos[1]), int.Parse(playerInfos[2]));
                        int health = int.Parse(playerInfos[3]);
                        int gold = int.Parse(playerInfos[4]);
                        Player playerToUpdate = _players.Single(p => p.PlayerId == id);
                        playerToUpdate.Coordinates = coordinates;
                        playerToUpdate.Health = health;
                        playerToUpdate.Gold = gold;
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
                        string[] tileInfos = responseParts[2].Split(',');
                        Coords coordinates = new Coords(int.Parse(tileInfos[0]), int.Parse(tileInfos[1]));
                        int gold = int.Parse(tileInfos[2]);
                        int health = int.Parse(tileInfos[3]);
                        gameBoard[coordinates.X, coordinates.Y].Gold = gold;
                        gameBoard[coordinates.X, coordinates.Y].Health = health;
                        _responseQueue.Enqueue(ServerResponse.UpdateTile);
                    }
                    if (responseParts[1] == "MESSAGE")
                    {
                        int id = int.Parse(responseParts[2]);
                        string name = _players.SingleOrDefault(p => p.PlayerId == id)?.Name;
                        if (_chattMessages.Count == 5)
                        {
                            _chattMessages.RemoveAt(0);
                        }
                        _chattMessages.Add($"{name}: {responseParts[3]}");
                    }
                    if (responseParts[1] == "DENIED")
                    {
                        _responseQueue.Enqueue(ServerResponse.Denied);
                    }
                }
            }
        }

        static bool SendMoveRequest()
        {
            ConsoleKeyInfo info = Handler.ReadKey();
            var request = new StringBuilder();
            request.Append("WAP/1.0 MOVE ");

            switch (info.Key)
            {
                case ConsoleKey.UpArrow:
                    request.Append("UP");
                    break;
                case ConsoleKey.DownArrow:
                    request.Append("DOWN");
                    break;
                case ConsoleKey.LeftArrow:
                    request.Append("LEFT");
                    break;
                case ConsoleKey.RightArrow:
                    request.Append("RIGHT");
                    break;
            }

            if (request.Length >= 14)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(request.ToString());
                socket.Send(bytes);
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
                Handler.Write($"{_players[i].Name}. Gold: {_players[i].Gold}.", 0, gameBoard.GetLength(1) + i + 1);
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
                Handler.Write($"{_players[i].Name}. Gold: {_players[i].Gold}.", 0, gameBoard.GetLength(1) + i + 1);
            }
        }

        static void PrintChattMessages()
        {
            for (int i = 0; i < _chattMessages.Count; i++)
            {
                Handler.Write(_chattMessages[i], gameBoard.GetLength(0), _players.Count + i + 1);
            }
        }

        static void ClearChattMessages()
        {
            for (int i = 0; i < _chattMessages.Count; i++)
            {
                Handler.ClearLine(gameBoard.GetLength(0), _players.Count + i + 1);
            }
        }
    }
}

