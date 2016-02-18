using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena.Repositories
{
    interface IPlayersRepository
    {
        Player GetByName(string name);
    }
}
