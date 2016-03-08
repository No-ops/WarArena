using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WwaWebServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class WwaWebServer : IWwaWebServer
    {
        public Statistics GetStats()
        {
            //hämta statistik från reposiotryn.
            IPlayerRepository repository = new PlayerRepository();
            PlayerModel goldPlayer = repository.GetPlayerWithMostGold();
            PlayerModel healthPlayer = repository.GetPlayerWithMostHealth();
            int numberOfPlayers = repository.GetTotalNumberOfPlayers();
            PlayerModel lastPlayer = repository.GetLastCreatedPlayer();
            return new Statistics
            {
                
            };
        }
    }
}
