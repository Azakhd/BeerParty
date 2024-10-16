using BeerParty.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeerParty.Data.Entities.Meet;

namespace BeerParty.Data
{
    public class ApplicationContext : DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<MeetEntity> MeetEntities { get; set; }

        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<Meet> Meets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInterest>()
                .HasKey(ui => new { ui.UserId, ui.InterestId }); // Композитный ключ

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User) // Указание на пользователя
                .WithMany(u => u.UserInterests) // У пользователя может быть много интересов
                .HasForeignKey(ui => ui.UserId); // Указываем внешний ключ

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.Interest) // Указание на интерес
                .WithMany(i => i.UserInterests) // У интереса может быть много пользователей
                .HasForeignKey(ui => ui.InterestId); // Указываем внешний ключ
            modelBuilder.Entity<Friend>()
              .HasOne(f => f.User) // Пользователь, который добавил друга
              .WithMany() // У пользователя может быть много друзей
              .HasForeignKey(f => f.UserId)
              .OnDelete(DeleteBehavior.Cascade); // Если пользователь удален, то и все его связи

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser) // Указываем на другого пользователя
                .WithMany() // У друга также может быть много друзей
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Cascade);

                        modelBuilder.Entity<MeetEntity>()
                 .HasOne(m => m.Organizer)
                 .WithMany() // Определите, если организатор может иметь много встреч
                 .HasForeignKey(m => m.OrganizerId)
                 .OnDelete(DeleteBehavior.Cascade); // Укажите поведение при удалении организатора


            modelBuilder.Entity<User>()// Настройка уникального индекса для Email
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasConversion(
                         e => e!.ToLower(), // Сохранение в нижнем регистре
                        e => e);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserInterests)
                .WithOne(ui => ui.User)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
         .HasOne(p => p.User) // Связь с пользователем
         .WithOne() // Один пользователь может иметь только один профиль
         .HasForeignKey<Profile>(p => p.UserId) // Указываем внешний ключ
         .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageEntity>()
    .HasOne(m => m.Sender)
    .WithMany()
    .HasForeignKey(m => m.SenderId)
    .OnDelete(DeleteBehavior.Restrict); // Для предотвращения каскадного удаления

            modelBuilder.Entity<MessageEntity>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
