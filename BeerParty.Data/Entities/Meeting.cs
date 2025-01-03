﻿using BeerParty.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace BeerParty.Data.Entities
{
    public class Meeting : BaseEntity
    {
        public string? Title { get; set; }
        public long CreatorId { get; set; } // Идентификатор создателя встречи
        public User? Creator { get; set; } // Связь с пользователем
        public long? CoAuthorId { get; set; } // Идентификатор соавтора (может быть null)
        public User? CoAuthor { get; set; } // Связь с соавтором
        public DateTime MeetingTime { get; set; } // Дата и время встречи
        public string? Location { get; set; } // Место встречи
        public string? Description { get; set; } // Описание встречи
        public MeetingCategory Category { get; set; }
        public short? ParticipantLimit {get; set;}
        public string PhotoUrl { get; set; }
        public bool IsPublic { get; set; } // Новое свойство
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
        public double? Radius { get; set; }
        [JsonIgnore]
        public List<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>(); // Участники встречи
        public virtual ICollection<MeetingReview> Reviews { get; set; }
        public ICollection<MeetingLike> MeetingLikes { get; set; } = new List<MeetingLike>();

        // Связь с отзывами
        public ICollection<MeetingReview> MeetingReviews { get; set; } = new List<MeetingReview>();
    }
}