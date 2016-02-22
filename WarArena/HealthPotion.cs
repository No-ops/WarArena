using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class HealthPotion
    {
        public HealthPotion(int health, Coords coordinates)
        {
            Health = health;
            Coordinates = coordinates;
        }

        public int Health { get; set; }
        public Coords Coordinates { get; set; }
    }
}
