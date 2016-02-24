using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArenaServer.Models
{
    class WarArenaContext : DbContext
    {
        public DbSet<PlayerModel> Players { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerModel>()
                .ToTable("Players")
                .HasKey(p => p.Name);
        }
    }
}
