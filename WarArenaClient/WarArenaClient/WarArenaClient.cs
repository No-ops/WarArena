using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WarArena.World;
using WarArena;
using WwaLibrary.World;

namespace WarArenaClient
{
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
        static string _playerName;
        static int _playerId;
        static string _opponentName;
        static int _opponentId;

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
            Console.WriteLine(response);
            Console.ReadKey();
            if (response == "Fel lösenord")
            {
                return false;
            }
            _playerName = name;
            return true;
        }

        public static LoginData Login()
        {
            LoginData data = new LoginData();

            string name = "";
            do
            {
                Handler.Clear();
                Handler.WriteLine($"Player {Player.PlayersCreated + 1}");
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

                RecieveResponse();
                _playerId = _players.Single(p => p.Name == _playerName).PlayerId;
                _opponentId = _playerId == 0 ? 1 : 0;
                _opponentName = _players[_opponentId].Name;
                if (_playerId == 0) //Jag har första draget
                {

                }
                else //Väntar på att motståndaren gör sitt drag
                {

                }
                while (true)
                {
                    RecieveResponse();
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

            string response;
            do
            {
                byte[] bytes = new byte[BUFFERLENGTH];
                socket.Receive(bytes);
                response = Encoding.UTF8.GetString(bytes);
            } while (response == "BAD REQUEST");
            string[] responseParts = response.Split(' ');
            if (responseParts[0] == "WAP/1.0" && responseParts[1] == "SENDSTATE")
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
                                player = new Player(name, health, 10, gold, coordinates);
                                player.PlayerId = playerId;
                                _players[playerId] = player;
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
                            break;
                        case "G":
                            coordinates = new Coords(int.Parse(itemInfos[1]), int.Parse(itemInfos[2]));
                            gold = int.Parse(itemInfos[3]);
                            Tile tile = gameBoard[coordinates.X, coordinates.Y];
                            tile.Gold = gold;
                            break;
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

        public void Display(bool isInTurn)
        {
            Handler.Clear();
            PrintBoard();
            foreach (var currentPlayer in _players)
            {
                PrintHealthBar(currentPlayer.PlayerId, currentPlayer.Health);
            }
            PrintPlayerStats();
            Handler.ChangeTextColor("Red");
           
        }

        private void PrintPlayerStats()
        {
            Handler.ChangeTextColor("White");
            Handler.SetCursorPosition(0, gameBoard.GetLength(1) + 1);
            Handler.Write($"{_playerName}'s turn. Gold: {_players[_playerId].Gold}.");
            Handler.SetCursorPosition(0, gameBoard.GetLength(1) + 2);
            Handler.Write($"{_opponentName}'s turn. Gold: {_players[_opponentId].Gold}.");
        }

        void PrintHealthBar(int playerId, int health)
        {
            Handler.ChangeTextColor("Black");
            Handler.SetCursorPosition(gameBoard.GetLength(0), playerId);

            for (int i = 0; i < 10; i++)
            {
                Handler.Write("");
            }

            if (playerId == 1)
            {
                Handler.ChangeTextColor("Red");
            }
            else if (playerId == 2)
            {
                Handler.ChangeTextColor("Blue");
            }

            Handler.SetCursorPosition(gameBoard.GetLength(0), playerId);

            for (int i = 0; i < health/10; i++)
            {
                Handler.WriteBlock(Player.PlayerColors[playerId]);
            }
        }

        public void PrintBoard()
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

        void PrintTile(Tile tile)
        {
            Handler.SetCursorPosition(tile.X, tile.Y);
            if (tile.HasGold)
            {
                Handler.ChangeTextColor("Yellow");
                Handler.Write('*');
            }
            else
            {
                Handler.ChangeTextColor(tile.Color);
                Handler.Write(tile.ImageCharacter);
            }
        }

        void PrintPlayer(Player player)
        {
            Handler.SetCursorPosition(player.Coordinates);
            Handler.ChangeTextColor(player.PlayerColor);
            Handler.Write("@");
        }
    }

}

