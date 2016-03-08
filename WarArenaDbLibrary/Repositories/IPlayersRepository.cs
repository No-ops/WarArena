using WarArenaDbLibrary.Models;

namespace WarArenaDbLibrary.Repositories
{
    public interface IPlayersRepository
    {
        PlayerModel GetByName(string name);
        PlayerModel GetById(int id);
        void Add(PlayerModel player);
        void Update(PlayerModel player);
    }
}
