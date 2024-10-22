using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class MeetingParticipant:BaseEntity
    {
        public long MeetingId { get; set; }
        [JsonIgnore]
        public Meeting? Meeting { get; set; } // Связь с встречей
        public long UserId { get; set; } // Идентификатор пользователя
        public User? User { get; set; } // Связь с пользователем
        public bool IsConfirmed { get; set; } // Статус подтверждения
    }
}
