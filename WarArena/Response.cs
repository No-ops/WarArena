using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WwaLibrary;
using WwaLibrary.Utilities;
using WwaLibrary.World;

namespace WarArena
{
    class Response
    {
        public enum MessageType
        {
            NEWPLAYER, REMOVEPLAYER, UPDATEPLAYER, UPDATETILE, YOURTURN, DENIED, INFO, MESSAGE, WELCOME
        }
        public string StringParam { get; set; }
        public MessageType ResponseType { get; set; }
        public int IdParam { get; set; }
        public Socket Socket { get; set; }

        public static void SendNewPlayerResponses(List<Client> clients, WorldMap world, int id, bool json)
        {
            var newPlayer = clients.Single(c => c.Player.PlayerId == id);
            foreach (var client in clients)
            {
                if (client.Player.PlayerId == id)
                {
                    var data = string.Empty;
                    if (json)
                    {
                        var tiles = new List<Tile>();
                        foreach (var tile in world.GameMap)
                        {
                            if (tile.HasGold || tile.HasHealth)
                                tiles.Add(tile);
                        }
                        var dataObject = new SendState(clients.Select(c => c.Player).ToList(), tiles);
                        data = $"WAP/1.0 SENDSTATE {SerializationFunctions.SerializeObject(dataObject)}";
                    }
                    else
                    {
                        var worldBuilder = new StringBuilder("WAP/1.0 SENDSTATE");
                        AddWorld(clients, world, worldBuilder);
                        worldBuilder.Append(';');
                        data = worldBuilder.ToString();
                    }
                    NetHelpers.SendString(data, client.Socket);
                }
                else
                {
                    if (json)
                    {
                        NetHelpers.SendString($"WAP/1.0 NEWPLAYER {SerializationFunctions.SerializeObject(newPlayer.Player)};", client.Socket);
                    }
                    else
                    {
                        NetHelpers.SendString($"WAP/1.0 NEWPLAYER {newPlayer.Player.Name},{newPlayer.Player};", client.Socket);
                    }
                }
            }
        }

        private static void AddWorld(List<Client> clients, WorldMap world, StringBuilder stringBuilder)
        {
            foreach (var client in clients)
            {
                var player = client.Player;
                stringBuilder.Append(
                    $" Pl,{player.Name},{player.PlayerId},{player.Coordinates.X},{player.Coordinates.Y},{player.Health},{player.Gold}");
            }

            foreach (var tile in world.GameMap)
            {
                if (tile.HasGold)
                    stringBuilder.Append($" G,{tile.X},{tile.Y},{tile.Gold}");

                if (tile.HasHealth)
                    stringBuilder.Append($" P,{tile.X},{tile.Y},{tile.Health}");
            }
        }

        public static void SendDeniedResponse(List<Client> clients, Socket socket, string stringParam)
        {
            NetHelpers.SendString($"WAP/1.0 DENIED {stringParam};", socket);
        }

        public static void SendRemovePlayerResponse(List<Client> clients, int id)
        {
            foreach (var client in clients)
            {
                NetHelpers.SendString($"WAP/1.0 REMOVEPLAYER {id};", client.Socket);
            }
        }

        public static void SendUpdateTile(List<Client> clients, string stringParam)
        {
            foreach (var client in clients)
            {
                NetHelpers.SendString($"WAP/1.0 UPDATETILE {stringParam};", client.Socket);
            }
        }

        public static void SendYourTurn(List<Client> clients, int id)
        {
            var client = clients.Single(c => c.Player.PlayerId == id);
            NetHelpers.SendString($"WAP/1.0 YOURTURN {id};", client.Socket);

        }

        public static void SendUpdatePlayer(List<Client> clients, int id, bool json)
        {
            var updatedPlayer = clients.Single(c => c.Player.PlayerId == id);
            foreach (var client in clients)
            {
                if (json)
                    NetHelpers.SendString($"WAP/1.0 UPDATEPLAYER {SerializationFunctions.SerializeObject(updatedPlayer.Player)};", client.Socket);
                else
                    NetHelpers.SendString($"WAP/1.0 UPDATEPLAYER {updatedPlayer.Player};", client.Socket);
            }
        }

        public static void SendMessage(List<Client> clients, int id, string message)
        {
            foreach (var client in clients)
            {
                NetHelpers.SendString($"WAP/1.0 MESSAGE {id} {message};", client.Socket);
            }
        }

        public static void SendWelcome(Socket socket, bool json)
        {
            var encoding = json ? "JSON" : "TEXT";
            NetHelpers.SendString($"WAP/1.0 WELCOME {encoding}", socket);
        }


    }
}
