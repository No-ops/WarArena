using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WarArena;
using WwaLibrary.World;

namespace WwaLibrary
{
    [DataContract]
    public class SendState
    {
        public SendState(IList<Player> players, IList<Tile> tiles)
        {
            Players = new List<Player>(players);
            Tiles = new List<Tile>(tiles);
        }
        [DataMember]
        public List<Player> Players { get; set; }
        [DataMember]
        public List<Tile> Tiles { get; set; }
    }
}
