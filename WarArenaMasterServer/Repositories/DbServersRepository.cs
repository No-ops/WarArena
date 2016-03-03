using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArenaMasterServer.Models;
using WarArenaMasterServer.Repositories;

namespace WarArena.Repositories
{
    class DbServersRepository : IServersRepository
    {
        WarArenaMasterServerContext context = new WarArenaMasterServerContext();

        public ServerModel GetByIPAndPort(string ip, string port)
        {
            return context.Servers.SingleOrDefault(s =>
            s.Ip == ip && s.Port == port);
        }
        public void Add(ServerModel server)
        {
            context.Servers.Add(server);
            context.SaveChanges();
        }
        public void Remove(ServerModel server)
        {
            context.Servers.Remove(server);
            context.SaveChanges();
        }
        public List<ServerModel> GetActiveServers()
        {
            var servers = new List<ServerModel>();
            foreach (var server in context.Servers)
            {
                servers.Add(server);
            }
            return servers;
        }

    }
}
