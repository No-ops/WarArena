using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExcersiseSerialization
{
    [DataContract]
    class Coords
    {
        public Coords(int x, int y)
        {
            X = x;
            Y = y;
        }
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
    }
}
