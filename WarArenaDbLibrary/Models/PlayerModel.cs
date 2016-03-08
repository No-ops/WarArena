using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarArenaDbLibrary.Models
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
        public DateTime Created { get; set; }
    }
}