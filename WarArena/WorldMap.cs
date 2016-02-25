using System.Collections.Generic;
using WarArena.Utilities;
using WwaLibrary.World;

namespace WarArena
{
    enum MoveResult
    {
        None,
        Success,
        Fail,
        Gold,
        Potion,
        Player
    }

    class WorldMap
    {
        public Tile[,] GameMap { get; set; }

        public void PlaceGold(IEnumerable<Client> clients)
        {
            if (RandomizationFunctions.Chance(30))
            {
                var coords = GetRandomFreeCoords(clients);
                GameMap[coords.X, coords.Y].Gold = RandomizationFunctions.GetRandomNumber(10, 50);
            }
        }

        public void CreatePotion(IEnumerable<Client> clients)
        {
            if (RandomizationFunctions.Chance(30))
            {
                var coords = GetRandomFreeCoords(clients);
                var health = RandomizationFunctions.GetRandomNumber(1, 100);
                GameMap[coords.X, coords.Y].Health = health;
            }
        }

        public Coords GetRandomFreeCoords(IEnumerable<Client> clients)
        {
            Coords coords = null;
            var tileIsFree = false;
            do
            {
                coords = new Coords(
                    RandomizationFunctions.GetRandomNumber(0, GameMap.GetLength(0) - 1),
                    RandomizationFunctions.GetRandomNumber(0, GameMap.GetLength(1) - 1)
                    );

                if (GameMap[coords.X, coords.Y].HasGold
                    || GameMap[coords.X, coords.Y].IsCaveWall
                    || GameMap[coords.X, coords.Y].HasHealth)
                    continue;

                tileIsFree = true;
                foreach (var client in clients)
                {
                    var player = client.Player;
                    if (player == null)
                        continue;

                    if (player.Coordinates.X == coords.X && player.Coordinates.Y == coords.Y)
                    {
                        tileIsFree = false;
                        break;
                    }
                }

            } while (!tileIsFree);
            return coords;
        }

        public MoveResultWrapper MovePlayer(Player player, string direction, IEnumerable<Client> clients)
        {
            var newX = player.Coordinates.X;
            var newY = player.Coordinates.Y;
            switch (direction)
            {
                case "UP":
                    newY--;
                    break;
                case "DOWN":
                    newY++;
                    break;
                case "LEFT":
                    newX--;
                    break;
                case "RIGHT":
                    newX++;
                    break;
                default:
                    return new MoveResultWrapper { MoveResult = MoveResult.Fail };
            }
            Player enemy;
            var result = GetTileContent(newX, newY, clients, out enemy);
            switch (result)
            {
                case MoveResult.Fail:
                    return new MoveResultWrapper { MoveResult = MoveResult.Fail };
                case MoveResult.Gold:
                    player.Coordinates.X = newX;
                    player.Coordinates.Y = newY;
                    player.Gold += GameMap[newX, newY].Gold;
                    GameMap[newX, newY].Gold = 0;
                    return new MoveResultWrapper { MoveResult = MoveResult.Gold };
                case MoveResult.Potion:
                    player.Coordinates.X = newX;
                    player.Coordinates.Y = newY;
                    player.Health += GameMap[newX, newY].Health;
                    GameMap[newX, newY].Health = 0;
                    return new MoveResultWrapper { MoveResult = MoveResult.Potion };
                case MoveResult.Success:
                    player.Coordinates.X = newX;
                    player.Coordinates.Y = newY;
                    return new MoveResultWrapper { MoveResult = MoveResult.Success };
                case MoveResult.Player:
                    player.Health += GameMap[newX, newY].Health;
                    enemy.Health -= player.Attack;
                    return new MoveResultWrapper { MoveResult = MoveResult.Player, Player = enemy };
                default:
                    return new MoveResultWrapper { MoveResult = MoveResult.Fail };

            }
        }

        private MoveResult GetTileContent(int x, int y, IEnumerable<Client> clients, out Player enemyPlayer)
        {
            enemyPlayer = null;
            if (x >= GameMap.GetLength(0) || y >= GameMap.GetLength(1) || GameMap[x,y].IsCaveWall)
                return MoveResult.Fail;
            if (GameMap[x, y].HasGold)
                return MoveResult.Gold;
            if (GameMap[x, y].HasHealth)
                return MoveResult.Potion;
            foreach (var client in clients)
            {
                var player = client.Player;
                if (player.Coordinates.X == x && player.Coordinates.Y == y)
                {
                    enemyPlayer = player;
                    return MoveResult.Player;
                }
            }

            return MoveResult.Success;
        }
    }
}