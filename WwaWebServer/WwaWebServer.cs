using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WarArenaDbLibrary.Models;
using WarArenaDbLibrary.Repositories;

namespace WwaWebServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class WwaWebServer : IWwaWebServer
    {
        public Statistics GetStats()
        {
            //hämta statistik från reposiotryn.
            IPlayersRepository repository = new DbPlayersRepository();
            PlayerModel goldPlayer = repository.GetPlayerWithMostGold();
            PlayerModel healthPlayer = repository.GetPlayerWithMostHealth();
            int numberOfPlayers = repository.GetTotalNumberOfPlayers();
            PlayerModel lastPlayer = repository.GetLastLoggedInPlayer();
            return new Statistics
            {
                LastPlayerLoggedIn = lastPlayer.Name,
                PlayerWithMostGold = goldPlayer.Name,
                PlayerWithMostHealth = healthPlayer.Name,
                TotalNumberOfPlayers = numberOfPlayers
            };
        }
    }
}
