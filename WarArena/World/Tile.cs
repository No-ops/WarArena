using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.World
{
    [Serializable]
    class Tile
    { 
        internal bool IsCaveWall { get; private set; }


        internal char ImageCharacter
        {
            get
            {
                if (IsCaveWall)
                    return '#';
                
                return '.';

            }
        }

        internal string Color
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

        internal Tile(int x, int y, bool isCaveWall = false)
            : base()
        {            
            X = x;
            Y = y;
            IsCaveWall = isCaveWall;
        }
    }
}
