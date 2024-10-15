using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class MessageEntity : BaseEntity
    {
        public long SenderId { get; set; }
        public long RecipientId { get; set; }
        [Required]
        [MaxLength(500)] // Ограничение на максимальную длину сообщения
        public string ?Content { get; set; }
        public DateTime SentAt { get; set; }
        public User ?Sender { get; set; }
        public User ?Recipient { get; set; }

    }
}
