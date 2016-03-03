using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace WarArenaMasterServer.Models
{
    class WarArenaMasterServerContext : DbContext
    {
        public DbSet<ServerModel> Servers { get; set; }
    }
}
