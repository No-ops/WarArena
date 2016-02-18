using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena.Models
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Attack { get; set; }
        public int Gold { get; set; }
        public int Health { get; set; }
        public bool IsDead { get; set; }
    }
}
