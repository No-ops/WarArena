using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using WarArenaDbLibrary.Models;

namespace WarArena
{
    static class Initiator
    {
        public static MapperConfiguration configuration;
        public static IMapper Mapper { get; set; }

        public static void AutoMapperConfig()
        {
            configuration = new MapperConfiguration(config =>
            {
                config.CreateMap<Player, PlayerModel>();
                config.CreateMap<PlayerModel, Player>();
            });
            Mapper = configuration.CreateMapper();
        }
    }
}
