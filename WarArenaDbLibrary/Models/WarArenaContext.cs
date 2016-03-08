using System.Data.Entity;

namespace WarArenaDbLibrary.Models
{
    public class WarArenaContext : DbContext
    {
        public DbSet<PlayerModel> Players { get; set; }
    }
}
