using System.Linq;
using WarArenaDbLibrary.Models;

namespace WarArenaDbLibrary.Repositories
{
    public class DbPlayersRepository : IPlayersRepository
    {
        WarArenaContext context = new WarArenaContext();

        public PlayerModel GetByName(string name)
        {
            return context.Players.SingleOrDefault(p => p.Name == name);
        }

        public PlayerModel GetById(int id)
        {
            return context.Players.Find(id);
        }

        public void Add(PlayerModel player)
        {
            context.Players.Add(player);
            context.SaveChanges();
        }

        public void Update(PlayerModel player)
        {
            context.Players.Attach(player);
            var entry = context.Entry(player);
            entry.Property(p => p.Gold).IsModified = true;
            entry.Property(p => p.Health).IsModified = true;
            context.SaveChanges();
        }

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

        public PlayerModel GetLastLoggedInPlayer()
        {
            return context.Players.OrderByDescending(p => p.LoggedInDate).First();
        }
    }
}
