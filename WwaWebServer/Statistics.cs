using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WwaWebServer
{
    [DataContract]
    public class Statistics
    {
        [DataMember]
        public string PlayerWithMostGold { get; set; }
        [DataMember]
        public string PlayerWithMostHealth { get; set; }
        [DataMember]
        public int TotalNumberOfPlayers { get; set; }
        [DataMember]
        public string LastPlayerLoggedIn { get; set; }
    }
}
