using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class Response
    {
        public enum MessageType
        {
            NEWPLAYER, REMOVEPLAYER, UPDATEPLAYER, UPDATETILE, YOURTURN, DENIED, INFO, MESSAGE
        }
        public string StringParam { get; set; }
        public MessageType ResponseType { get; set; }
        public int IdParam { get; set; }
        public Socket Socket { get; set; }

        private static void SendString(string message, Socket socket)
        {
            var buffer = WWaServer.encoding.GetBytes(message);
            //Console.WriteLine($"Send: {message}\nTo: {socket.RemoteEndPoint}");
            socket.Send(buffer);
        }

        public static void SendNewPlayerResponses(List<Client> clients, WorldMap world, int id)
        {
            var newPlayer = clients.Single(c => c.Player.PlayerId == id);
            foreach (var client in clients)
            {
                if (client.Player.PlayerId == id)
                {
                    var worldBuilder = new StringBuilder("WAP/1.0 SENDSTATE");
                    AddWorld(clients, world, worldBuilder);
                    worldBuilder.Append(';');
                    SendString(worldBuilder.ToString(), client.Socket);
                }
                else
                {
                    SendString($"WAP/1.0 NEWPLAYER {newPlayer.Player.Name},{newPlayer.Player};", client.Socket);
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
                if(tile.HasGold)
                    stringBuilder.Append($" G,{tile.X},{tile.Y},{tile.Gold}");

                if(tile.HasHealth)
                    stringBuilder.Append($" P,{tile.X},{tile.Y},{tile.Health}");
            }
        }

        public static void SendDeniedResponse(List<Client> clients, Socket socket, string stringParam)
        {
            SendString($"WAP/1.0 DENIED {stringParam};", socket);
        }

        public static void SendRemovePlayerResponse(List<Client> clients, int id)
        {
            foreach (var client in clients)
            {
                SendString($"WAP/1.0 REMOVEPLAYER {id};", client.Socket);
            }
        }

        public static void SendUpdateTile(List<Client> clients, string stringParam)
        {
            foreach (var client in clients)
            {
                SendString($"WAP/1.0 UPDATETILE {stringParam};", client.Socket);
            }
        }

        public static void SendYourTurn(List<Client> clients, int id)
        {
            var client = clients.Single(c => c.Player.PlayerId == id);
            SendString($"WAP/1.0 YOURTURN {id};", client.Socket);

        }

        public static void SendUpdatePlayer(List<Client> clients, int id)
        {
            var updatedPlayer = clients.Single(c => c.Player.PlayerId == id);
            foreach (var client in clients)
            {
                SendString($"WAP/1.0 UPDATEPLAYER {updatedPlayer.Player};", client.Socket);
            }
        }

        public static void SendMessage(List<Client> clients, int id, string message)
        {
            foreach (var client in clients)
            {
                SendString($"WAP/1.0 MESSAGE {id} {message};", client.Socket);
            }
        }
    }
}
