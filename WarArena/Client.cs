using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class Client
    {
        public Player Player { get; set; }
        public Socket Socket { get; set; }
    }
}
