﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArena.Models;

namespace WarArena.Repositories
{
    class DbPlayersRepository : IPlayersRepository
    {
        WarArenaContext context = new WarArenaContext();

        public Player GetByName(string name)
        {
            return context.Players.SingleOrDefault(p => p.Name == name);
        }
    }
}
