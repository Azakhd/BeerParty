using BeerParty.Data.Enums;
using System;
using System.Text.Json.Serialization;
namespace BeerParty.Data.Entities
{
    public class Meeting : BaseEntity
    {
        public string? Title { get; set; }
        public long CreatorId { get; set; } // Идентификатор создателя встречи
        public User? Creator { get; set; } // Связь с пользователем

        public DateTime MeetingTime { get; set; } // Дата и время встречи
        public string? Location { get; set; } // Место встречи
        public string? Description { get; set; } // Описание встречи
        public MeetingCategory Category { get; set; }
        public bool IsPublic { get; set; } // Новое свойство
        [JsonIgnore]
        public List<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>(); // Участники встречи
    }
}