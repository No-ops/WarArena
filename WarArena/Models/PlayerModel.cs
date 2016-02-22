using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena.Models
{
    [Table("Players")]
    public class PlayerModel
    {
        [Key]
        public string Name { get; set; }
        public string Password { get; set; }
        public int Attack { get; set; }
        public int Gold { get; set; }
        public int Health { get; set; }
    }
}