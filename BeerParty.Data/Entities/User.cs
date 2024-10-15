

using BeerParty.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System.ComponentModel.DataAnnotations;

namespace BeerParty.Data.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string ?Name { get; set; }

        [EmailAddress]
        [Required]
        public string ?Email { get; set; }
        public int? Age => CalculateAge();

        [Required]
        public string ?PasswordHash { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public Profile? Profile { get; set; }
        public ICollection<UserInterest> ?UserInterests { get; set; }

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

