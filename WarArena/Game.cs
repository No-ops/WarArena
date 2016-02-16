using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.World;

namespace WarArena
{
    enum Direction
    {
        North,
        South,
        East,
        West
    }

    enum AttackType
    {
        OverheadSwing,
        Slash,
        Thrust
    }

    class Game
    {
        public Game()
        {
            Handler = new IOHandler();
            Validator = new Validator();
            GameMap = MapCreator.CreateMap();
        }
        public IOHandler Handler { get; set; }
        public Validator Validator { get; set; }

        public Player[] Players { get; set; }

        public Tile[,] GameMap { get; set; }

        public Player CreatePlayer()
        {
            string name = "";
            do
            {
                Handler.Clear();
                Handler.Write($"Please enter your name: ");
                name = Handler.ReadString();
            } while (!Validator.HasMinLength(name, 3));
            return new Player(name, 100, 10, 10, new Coords(0, 0));
        }

        void SetUpBoard()
        {
            PrintBoard();
            PrintHealthBar(1, Players[0].Health);
            PrintHealthBar(2, Players[1].Health);
        }

        public void PrintBoard()
        {
            for (int i = 0; i < GameMap.GetLength(1); i++)
            {
                for (int j = 0; j < GameMap.GetLength(0); i++)
                {
                    PrintTile(GameMap[j, i]);
                }
            }
        }

        void PrintHealthBar(int playerId, int health)
        {
            Handler.ChangeTextColor("Black");
            Handler.SetCursorPosition(GameMap.GetLength(0), playerId - 1);

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

            Handler.SetCursorPosition(GameMap.GetLength(0), playerId - 1);

            for (int i = 0; i < health / 10; i++)
            {
                Handler.Write("");
            }
        }

        void PrintTile(Tile tile)
        {
            Handler.SetCursorPosition(tile.X, tile.Y);
            Handler.ChangeTextColor(tile.Color);
            Handler.Write(tile.ImageCharacter);
        }

        void PrintInstructions()
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            Handler.Write("Use the arrow keys to move or attack if adjacent to your opponent");
        }

        void PrintCombatInstructions(Player player)
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            Handler.WriteLine($"Choose type of attack, player {player.PlayerId}: ");
            Handler.WriteLine("(O)verhead swing");
            Handler.WriteLine("(S)lash");
            Handler.WriteLine("(T)hrust");
        }

        void ClearInstructions()
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            for (int i = 0; i < 65; i++)
            {
                Handler.Write("");
            }
        }

        void ClearCombatInstructions()
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 31; j++)
                {
                    Handler.Write("");
                }
            }
        }

        void PrintCombatResult(Player winner, Player loser)
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            Handler.Write($"Player {winner.PlayerId} hits {loser.PlayerId} for 10 hit points");
        }

        void ClearCombatResults()
        {
            Handler.SetCursorPosition(0, GameMap.GetLength(1));
            for (int i = 0; i < 33; i++)
            {
                Handler.Write("");
            }
        }

        void PrintPlayer(Player player)
        {
            Handler.SetCursorPosition(player.Coordinates);
            Handler.Write(player.PlayerId.ToString());
        }

        void TakeAction(Player player, ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    break;

                case ConsoleKey.DownArrow:
                    break;

                case ConsoleKey.RightArrow:
                    break;

                case ConsoleKey.LeftArrow:
                    break;
            }
        }

        public Player FindAdjacentPlayerInGivenDirection(Player player, Direction direction)
        {
            Player result = null;
            foreach (Player otherPlayer in Players)
            {
                if (player.IsPlayerAdjacentInGivenDirection(otherPlayer, direction))
                {
                    result = otherPlayer;
                }
            }
            return result;
        }

        void MovePlayer(Player player, Direction direction)
        {
            PrintTile(GameMap[player.Coordinates.X, player.Coordinates.Y]);
            switch (direction)
            {
                case Direction.North:
                    if (!GameMap[player.Coordinates.X, player.Coordinates.Y - 1].IsCaveWall)
                    {
                        player.Coordinates.Y--;
                    }
                    break;

                case Direction.South:
                    if (!GameMap[player.Coordinates.X, player.Coordinates.Y + 1].IsCaveWall)
                    {
                        player.Coordinates.Y++;
                    }
                    break;
                case Direction.East:
                    if (!GameMap[player.Coordinates.X + 1, player.Coordinates.Y].IsCaveWall)
                    {
                        player.Coordinates.X++;
                    }
                    break;
                case Direction.West:
                    if (!GameMap[player.Coordinates.X - 1, player.Coordinates.Y].IsCaveWall)
                    {
                        player.Coordinates.X--;
                    }
                    break;
            }
            PrintPlayer(player);
        }

        public void Attack(Player attacker, Player defender)
        {
            SetAttackType(attacker);
            SetAttackType(defender);
            switch (attacker.AttackType)
            {
                case AttackType.OverheadSwing:
                    if (defender.AttackType == AttackType.Slash)
                    {
                        defender.Health -= 10;
                        PrintCombatResult(attacker, defender);
                    }
                    else if (defender.AttackType == AttackType.Thrust)
                    {
                        attacker.Health -= 10;
                        PrintCombatResult(defender, attacker);
                    }
                    break;
                case AttackType.Slash:
                    if (defender.AttackType == AttackType.Thrust)
                    {
                        defender.Health -= 10;
                        PrintCombatResult(attacker, defender);
                    }
                    else if (defender.AttackType == AttackType.OverheadSwing)
                    {
                        attacker.Health -= 10;
                        PrintCombatResult(defender, attacker);
                    }
                    break;
                case AttackType.Thrust:
                    if (defender.AttackType == AttackType.OverheadSwing)
                    {
                        defender.Health -= 10;
                        PrintCombatResult(attacker, defender);
                    }
                    else if (defender.AttackType == AttackType.Slash)
                    {
                        attacker.Health -= 10;
                        PrintCombatResult(defender, attacker);
                    }
                    break;
            }
        }

        void SetAttackType(Player player)
        {
            ConsoleKey key;
            PrintCombatInstructions(player);
            do
            {
                key = Handler.ReadKey().Key;
            } while (Validator.IsKeyValid(key, ConsoleKey.O, ConsoleKey.S, ConsoleKey.T));
            switch (key)
            {
                case ConsoleKey.O:
                    player.AttackType = AttackType.OverheadSwing;
                    break;
                case ConsoleKey.S:
                    player.AttackType = AttackType.Slash;
                    break;
                case ConsoleKey.T:
                    player.AttackType = AttackType.Thrust;
                    break;
            }
        }

        //public Player GameLoop()
        //{
        //    do
        //    {
        //        foreach (Player player in Players)
        //        {

        //        }
        //    } while (!Players.Any(p => p.IsDead));
        //}
    }
}
