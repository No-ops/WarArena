using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities;
using Core.World;
using WarArena.Models;
using WarArena.Repositories;

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

    enum MoveResult
    {
        None,
        Success,
        Fail,
        Gold,
        Potion,
        Player
    }

    class Game
    {
        public Game()
        {
            Handler = new IOHandler();
            Validator = new Validator();
            GameMap = MapCreator.CreateEmptyMap();
            Players = new Player[2];
        }
        public IOHandler Handler { get; set; }
        public Validator Validator { get; set; }

        public Player[] Players { get; set; }

        public List<HealthPotion> Potions { get; set; }

        public Tile[,] GameMap { get; set; }

        public Player CreatePlayer()
        {
            Player player = null;
            while (player == null)
            {

                string name = "";
                do
                {
                    Handler.Clear();
                    Handler.WriteLine($"Player {Player.PlayersCreated + 1}");
                    Handler.Write($"Please enter your name: ");
                    name = Handler.ReadString();
                } while (!Validator.HasMinLength(name, 3));

                IPlayersRepository repository = new DbPlayersRepository();
                var existingPlayer = repository.GetByName(name);
                if (existingPlayer == null)
                {
                    player = new Player(name, 100, 10, 0, GetRandomFreeCoords());
                    Handler.Write("Enter a password for your character:");
                    var password = Handler.ReadString();
                    var model = Initiator.Mapper.Map<PlayerModel>(player);
                    model.Password = password;
                    model.Created = DateTime.Now;
                    repository.Add(model);
                }
                else
                {
                    string keepTrying = "y";
                    bool correctPassword = false;
                    while (keepTrying.ToLower() == "y" && !correctPassword)
                    {

                        Handler.WriteLine("This character already exist.");
                        Handler.Write("Enter your password:");
                        var password = Handler.ReadString();
                        if (existingPlayer.Password == password)
                        {
                            correctPassword = true;
                            player = new Player(GetRandomFreeCoords());
                            Initiator.Mapper.Map(existingPlayer, player);
                        }
                        else
                        {
                            Handler.WriteLine("Wrong password.");
                            Handler.Write("Try again? (y/n):");
                            keepTrying = Handler.ReadString();
                        }
                    }
                }
            }
            return player;
        }

        public void SetUpGame()
        {
            Players = new Player[2];
            Players[0] = CreatePlayer();
            Players[1] = CreatePlayer();
        }

        public void Display(Player player)
        {
            Handler.Clear();
            PrintBoard();
            foreach (var currentPlayer in Players)
            {
                PrintHealthBar(currentPlayer.PlayerId, currentPlayer.Health);
            }
            PrintPlayerStats(player);
            Handler.ChangeTextColor("Red");
            if (Potions != null)
            {
                foreach (HealthPotion potion in Potions)
                {
                    Handler.SetCursorPosition(potion.Coordinates);
                    Handler.Write("P");
                }
            }
        }

        private void PrintPlayerStats(Player player)
        {
            Handler.ChangeTextColor("White");
            Handler.SetCursorPosition(0, GameMap.GetLength(1) + 1);
            Handler.Write($"{player.Name}'s turn. Gold: {player.Gold}.");
        }

        public void PrintBoard()
        {
            for (int y = 0; y < GameMap.GetLength(1); y++)
            {
                for (int x = 0; x < GameMap.GetLength(0); x++)
                {
                    PrintTile(GameMap[x, y]);
                }
            }

            foreach (var player in Players)
            {
                PrintPlayer(player);
            }
        }


        public Coords GetRandomFreeCoords()
        {
            Coords coords = null;
            var tileIsFree = false;
            do
            {
                coords = new Coords(
                    RandomizationFunctions.GetRandomNumber(0, GameMap.GetLength(0) - 1),
                    RandomizationFunctions.GetRandomNumber(0, GameMap.GetLength(1) - 1)
                    );

                if (GameMap[coords.X, coords.Y].HasGold || GameMap[coords.X, coords.Y].IsCaveWall)
                    continue;

                tileIsFree = true;
                foreach (var player in Players)
                {
                    if (player == null)
                        continue;

                    if (player.Coordinates.X == coords.X && player.Coordinates.Y == coords.Y)
                    {
                        tileIsFree = false;
                        break;
                    }
                }
                if (Potions != null)
                {
                    foreach (var potion in Potions)
                    {
                        if (potion.Coordinates.X == coords.X && potion.Coordinates.Y == coords.Y)
                        {
                            tileIsFree = false;
                            break;
                        }
                    }
                }
            } while (!tileIsFree);
            return coords;
        }

        void PrintHealthBar(int playerId, int health)
        {
            Handler.ChangeTextColor("Black");
            Handler.SetCursorPosition(GameMap.GetLength(0), playerId);

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

            Handler.SetCursorPosition(GameMap.GetLength(0), playerId);

            for (int i = 0; i < health / 10; i++)
            {
                Handler.WriteBlock(Player.PlayerColors[playerId]);
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

        void PrintHealthPotion(HealthPotion potion)
        {
            Handler.SetCursorPosition(potion.Coordinates);
            Handler.ChangeTextColor("Red");
            Handler.Write("P");
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

        public void TakeAction(Player player, ConsoleKey key)
        {
            MoveResult moveResult = MoveResult.None;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    moveResult = MovePlayer(player, Direction.North);
                    break;
                case ConsoleKey.DownArrow:
                    moveResult = MovePlayer(player, Direction.South);
                    break;
                case ConsoleKey.LeftArrow:
                    moveResult = MovePlayer(player, Direction.West);
                    break;
                case ConsoleKey.RightArrow:
                    moveResult = MovePlayer(player, Direction.East);
                    break;
            }

            switch (moveResult)
            {
                case MoveResult.Gold:
                    player.Gold += GameMap[player.Coordinates.X, player.Coordinates.Y].Gold;
                    GameMap[player.Coordinates.X, player.Coordinates.Y].Gold = 0;
                    break;
                case MoveResult.Potion:
                    HealthPotion potion = Potions
                        .Single(p => p.Coordinates.X == player.Coordinates.X &&
                                     p.Coordinates.Y == player.Coordinates.Y);
                    player.Health += potion.Health;
                    Potions.Remove(potion);
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

        MoveResult MovePlayer(Player player, Direction direction)
        {
            var newCoords = new Coords(player.Coordinates.X, player.Coordinates.Y);
            switch (direction)
            {
                case Direction.North:
                    newCoords.Y--;
                    break;

                case Direction.South:
                    newCoords.Y++;
                    break;
                case Direction.East:
                    newCoords.X++;
                    break;
                case Direction.West:
                    newCoords.X--;
                    break;
            }

            if (GameMap[newCoords.X, newCoords.Y].IsCaveWall)
                return MoveResult.Fail;

            foreach (var otherPlayer in Players)
            {
                if (otherPlayer.Coordinates.X == newCoords.X && otherPlayer.Coordinates.Y == newCoords.Y)
                {
                    SimpleCombat(player, otherPlayer);
                    return MoveResult.Player;
                }
            }

            //PrintTile(GameMap[player.Coordinates.X, player.Coordinates.Y]);
            player.Coordinates = newCoords;
            //PrintPlayer(player);
            if (GameMap[newCoords.X, newCoords.Y].HasGold)
                return MoveResult.Gold;
            if (Potions != null)
            {
                foreach (var potion in Potions)
                {
                    if (potion.Coordinates.X == newCoords.X && potion.Coordinates.Y == newCoords.Y)
                    {
                        return MoveResult.Potion;
                    }
                }

            }
            return MoveResult.Success;
        }

        private void SimpleCombat(Player attacker, Player defender)
        {
            defender.Health -= attacker.Attack;
            if (defender.Health <= 0)
                defender.IsDead = true;
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

        public void GameLoop()
        {
            do
            {
                foreach (Player player in Players)
                {
                    if (player.IsDead)
                        RespawnPlayer(player);
                    PlaceGold();
                    CreatePotion();
                    Display(player);
                    var input = Handler.ReadKey();
                    MoveResult moveResult = MoveResult.None;
                    switch (input.Key)
                    {
                        case ConsoleKey.UpArrow:
                            moveResult = MovePlayer(player, Direction.North);
                            break;
                        case ConsoleKey.DownArrow:
                            moveResult = MovePlayer(player, Direction.South);
                            break;
                        case ConsoleKey.LeftArrow:
                            moveResult = MovePlayer(player, Direction.West);
                            break;
                        case ConsoleKey.RightArrow:
                            moveResult = MovePlayer(player, Direction.East);
                            break;
                    }

                    switch (moveResult)
                    {
                        case MoveResult.Gold:
                            player.Gold += GameMap[player.Coordinates.X, player.Coordinates.Y].Gold;
                            GameMap[player.Coordinates.X, player.Coordinates.Y].Gold = 0;
                            break;
                        case MoveResult.Potion:
                            HealthPotion potion = Potions
                                .Single(p => p.Coordinates.X == player.Coordinates.X &&
                                             p.Coordinates.Y == player.Coordinates.Y);
                            player.Health += potion.Health;
                            Potions.Remove(potion);
                            break;
                    }
                }

                IPlayersRepository repository = new DbPlayersRepository();
                foreach (var player in Players)
                {
                    var model = Initiator.Mapper.Map<PlayerModel>(player);
                    repository.Update(model);
                }

            } while (true);
        }

        public void PlaceGold()
        {
            if (RandomizationFunctions.Chance(30))
            {
                var coords = GetRandomFreeCoords();
                GameMap[coords.X, coords.Y].Gold = RandomizationFunctions.GetRandomNumber(10, 50);
            }
        }

        public void CreatePotion()
        {
            if (RandomizationFunctions.Chance(30))
            {
                var coords = GetRandomFreeCoords();
                var health = RandomizationFunctions.GetRandomNumber(1, 100);
                HealthPotion potion = new HealthPotion(health, coords);
                if (Potions == null)
                {
                    Potions = new List<HealthPotion>();
                }
                Potions.Add(potion);
            }
        }

        public void RespawnPlayer(Player player)
        {
            GameMap[player.Coordinates.X, player.Coordinates.Y].Gold = player.Gold;
            player.Gold = 0;
            player.Coordinates = GetRandomFreeCoords();
            player.Health = 100;
            player.IsDead = false;
        }
    }
}
