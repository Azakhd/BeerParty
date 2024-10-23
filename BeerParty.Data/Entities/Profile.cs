using BeerParty.Data.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeerParty.Data.Entities
{
    public class Profile:BaseEntity
    {

        public long UserId { get; set; }
        public User ?User { get; set; } // Связь с пользователем
        public string? PhotoUrl { get; set; }
        public ICollection<Friend>? Friends { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }

        public virtual ICollection<Interest> Interests { get; set; } = new List<Interest>(); 
        public double? Height { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public int? Age => CalculateAge();
        public Gender Gender { get; set; }
        public PreferenceType? LookingFor { get; set; }
        private int? CalculateAge()
        {
            if (DateOfBirth == default) return null; // Если дата рождения не задана, возвращаем null
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--; // Корректируем, если день рождения еще не наступил в текущем году
            return age;
        }
    }
}

