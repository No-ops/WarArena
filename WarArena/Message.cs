using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarArena
{
    class Message
    {
        public enum RecipientType
        {
            All, One, AllExcept
        }
        public string Content { get; set; }
        public RecipientType Recipient { get; set; }
        public int Id { get; set; }

        public Message(string content, RecipientType recipient, int id)
        {
            Id = id;
            Recipient = recipient;
            Content = content;
        }
    }
}
