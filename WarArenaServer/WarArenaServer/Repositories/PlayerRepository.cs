using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArenaServer.Models;

namespace WarArenaServer.Repositories
{
    class PlayerRepository : IPlayerRepository
    {
        WarArenaContext context = new WarArenaContext();

        public PlayerModel GetPlayerWithMostGold()
        {
            return context.Players.OrderByDescending(p => p.Gold).First();
        }

        public PlayerModel GetPlayerWithMostHealth()
        {
            return context.Players.OrderByDescending(p => p.Health).First();
        }

        public int GetTotalNumberOfPlayers()
        {
            return context.Players.Count();
        }

        public PlayerModel GetLastCreatedPlayer()
        {
            return context.Players.OrderByDescending(p => p.Created).First();
        }
    }
}
