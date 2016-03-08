using WarArenaDbLibrary.Models;

namespace WarArenaDbLibrary.Repositories
{
    public interface IPlayersRepository
    {
        PlayerModel GetByName(string name);
        void Add(PlayerModel player);
        void Update(PlayerModel player);
        PlayerModel GetPlayerWithMostGold();
        PlayerModel GetPlayerWithMostHealth();
        int GetTotalNumberOfPlayers();
        PlayerModel GetLastCreatedPlayer();
    }
}
