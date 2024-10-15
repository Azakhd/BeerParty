using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class ChatMessage
    {

        public string? Sender { get; set; }
        public string? Receiver { get; set; } // Получатель сообщения
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
