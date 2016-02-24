using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArenaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer server = new HttpServer();
            server.Start();
            Console.ReadKey();
            //SimpleHttpListener.SimpleListenerExample(new[] { "http://localhost:8001/" });
        }
    }
}
