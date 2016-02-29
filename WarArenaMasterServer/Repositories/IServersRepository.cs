using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArenaMasterServer.Models;

namespace WarArenaMasterServer.Repositories
{
    interface IServersRepository
    {
        void Add(ServerModel server);
        void Remove(ServerModel server);
        ServerModel GetByIPAndPort(string ip, string port);
        List<ServerModel> GetActiveServers();
    }
}
