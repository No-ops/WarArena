using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarArena.World
{
    [Serializable]
    class Tile
    { 
        public bool IsCaveWall { get; private set; }
        public char ImageCharacter
        {
            get
            {
                if (IsCaveWall)
                    return '#';
                
                return '.';

            }
        }

        public string Color
        {
            get
            {
                if (IsCaveWall)
                    return "DarkCyan";
                
                return "White";
            }
        }
        public int X { get; set; }
        public int Y { get; set; }

        public Tile(int x, int y, bool isCaveWall = false)
            : base()
        {            
            X = x;
            Y = y;
            IsCaveWall = isCaveWall;
        }

        public int Gold { get; set; }
        public bool HasGold => Gold > 0;
    }
}
