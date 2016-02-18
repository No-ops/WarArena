using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarArena.Models;

namespace WarArena.Repositories
{
    interface IPlayersRepository
    {
        PlayerModel GetByName(string name);
        void Add(PlayerModel player);
    }
}
