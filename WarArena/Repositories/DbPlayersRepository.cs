﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArena.Models;

namespace WarArena.Repositories
{
    class DbPlayersRepository : IPlayersRepository
    {
        WarArenaContext context = new WarArenaContext();

        public PlayerModel GetByName(string name)
        {
            return context.Players.SingleOrDefault(p => p.Name == name);
        }

        public void Add(PlayerModel player)
        {
            context.Players.Add(player);
            context.SaveChanges();
        }

        public void Update(PlayerModel player)
        {
            context.Entry(player).State = EntityState.Modified;
            context.SaveChanges();
        }
    }
}
