using BeerParty.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace BeerParty.Data.Entities
{
    public class Profile:BaseEntity
    {

        public long UserId { get; set; }
        public User User { get; set; } // Связь с пользователем
        public string? PhotoUrl { get; set; }
        public ICollection<Friend>? Friends { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public ICollection<Interest>? Interests { get; set; }
        public double? Height { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public int? Age => CalculateAge();
        public Gender Gender { get; set; }
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

