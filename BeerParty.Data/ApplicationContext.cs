using BeerParty.Data.Entities;
using BeerParty.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingReview> MeetingReviews { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInterest>()
                .HasKey(ui => new { ui.UserId, ui.InterestId });

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserInterests)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.Interest)
                .WithMany(i => i.UserInterests)
                .HasForeignKey(ui => ui.InterestId);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()// Настройка уникального индекса для Email
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasConversion(
                    e => e!.ToLower(),
                    e => e);

            modelBuilder.Entity<MeetingReview>()
          .HasOne(r => r.Meeting)
          .WithMany(m => m.Reviews)
          .HasForeignKey(r => r.MeetingId)
          .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingReview>()
                .HasOne(r => r.User)
                .WithMany(u => u.MeetingReviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Profile>()
       .HasOne(p => p.User)
       .WithOne(u => u.Profile) // Указываем навигационное свойство User
       .HasForeignKey<Profile>(p => p.UserId)
       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
        .Ignore(p => p.Photo);

            modelBuilder.Entity<MessageEntity>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageEntity>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация Meeting
            modelBuilder.Entity<Meeting>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Creator)
                .WithMany()
                .HasForeignKey(m => m.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meeting>()
      .HasMany(m => m.Participants) // Встреча может иметь много участников
      .WithOne(mp => mp.Meeting) // Участник ссылается на одну встречу
      .HasForeignKey(mp => mp.MeetingId) // Указываем внешний ключ
      .OnDelete(DeleteBehavior.Cascade); // При удалении встречи, удаляются и участники

            // Конфигурация MeetingParticipant
            modelBuilder.Entity<MeetingParticipant>()
         .HasKey(mp => new { mp.MeetingId, mp.UserId }); // Композитный ключ

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.Meeting) // Указываем связь с встречей
                .WithMany(m => m.Participants) // У встречи может быть много участников
                .HasForeignKey(mp => mp.MeetingId); // Указываем внешний ключ

            modelBuilder.Entity<MeetingParticipant>()
                .HasOne(mp => mp.User) // Указываем связь с пользователем
                .WithMany() // У пользователя может быть много участников
                .HasForeignKey(mp => mp.UserId); // Указываем внешний ключ

            modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Опционально


            modelBuilder.Entity<Like>()
                .HasOne(l => l.MeetingReview)
                .WithMany(m => m.Likes)
                .HasForeignKey(l => l.MeetingReviewId)
                .OnDelete(DeleteBehavior.Cascade); // Опционально
        }
    }
}
