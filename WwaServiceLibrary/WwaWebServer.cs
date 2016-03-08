using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WarArenaDbLibrary.Models;
using WarArenaDbLibrary.Repositories;
using WwaWebServer;

namespace WwaServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class WwaWebServer : IWwaWebServer
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
