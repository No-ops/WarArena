using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class Player
    {
        public static readonly string[] PlayerColors = { "Red", "Blue", "Green" };

        static int _playersCreated = 0;
        public static int PlayersCreated => _playersCreated;

        public Player(Coords startCoordinates)
        {
            PlayerId = _playersCreated++;
            Coordinates = startCoordinates;
        }
        public Player(string name, int health, int attack, int gold, Coords startCoordinates)
        {
            Name = name;
            PlayerId = _playersCreated++;
            Health = health;
            Attack = attack;
            Coordinates = startCoordinates;
            IsDead = false;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Attack { get; set; }
        public int Gold { get; set; }
        public int PlayerId { get; set; }
        public int Health { get; set; }
        public bool IsDead { get; set; }
        public Coords Coordinates { get; set; }
        public AttackType AttackType { get; set; }
        public string PlayerColor => PlayerColors[PlayerId];
        public void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    Coordinates.Y--;
                    break;
                case Direction.South:
                    Coordinates.Y++;
                    break;
                case Direction.East:
                    Coordinates.X++;
                    break;
                case Direction.West:
                    Coordinates.X--;
                    break;
            }
        }

        public bool IsPlayerAdjacentInGivenDirection(Player other, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Coordinates.X == other.Coordinates.X && Coordinates.Y - 1 == other.Coordinates.Y;
                case Direction.South:
                    return Coordinates.X == other.Coordinates.X && Coordinates.Y + 1 == other.Coordinates.Y;
                case Direction.East:
                    return Coordinates.X + 1 == other.Coordinates.X && Coordinates.Y == other.Coordinates.Y;
                case Direction.West:
                    return Coordinates.X - 1 == other.Coordinates.X && Coordinates.Y == other.Coordinates.Y;
                default:
                    return false;
            }
        }
    }
}
