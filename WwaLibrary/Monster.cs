using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    public class Monster
    {
        public int Health { get; set; }
        public int Damage { get; set; }
        public int Gold { get; set; }
        public Coords Coordinates { get; set; }

        public Monster(int health, int damage, int gold, Coords startCoordinates)
        {
            Health = health;
            Damage = damage;
            Gold = gold;
            Coordinates = startCoordinates;
        }

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


    }
}
