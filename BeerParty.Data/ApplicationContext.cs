using BeerParty.Data.Entities;
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
        public DbSet<UserInterest> Interests { get; set; }
        public DbSet<Profile> Profiles { get; set; }

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

            modelBuilder.Entity<User>()// Настройка уникального индекса для Email
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasConversion(
                         e => e.ToLower(), // Сохранение в нижнем регистре
                        e => e);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserInterests)
                .WithOne(ui => ui.User)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
            .HasOne(p => p.User) // Каждый профиль связан с одним пользователем
            .WithOne() // Один пользователь может иметь только один профиль
            .HasForeignKey<Profile>(p => p.UserId) // Указываем внешний ключ
            .OnDelete(DeleteBehavior.Cascade);

     
        }
    }
}
