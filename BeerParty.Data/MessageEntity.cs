using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data
{
    internal class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public User Sender { get; set; }
        public User Recipient { get; set; }

    }
}
