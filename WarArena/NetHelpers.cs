using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    static class NetHelpers
    {
        public static void SendString(string message, Socket socket)
        {
            var buffer = WWaServer.encoding.GetBytes(message);
            //Console.WriteLine($"Send: {message}\nTo: {socket.RemoteEndPoint}");
            socket.Send(buffer);
        }
    }
}
