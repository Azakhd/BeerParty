using System;
using BeerParty.Data.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BeerParty.Data.Entities
{
    public class ProfileInteraction
    {
        public int Id { get; set; }
        public int FromProfileId { get; set; } // ID профиля, который ставит лайк
        public int ToProfileId { get; set; }   // ID профиля, который получает лайк
        public InteractionType Type { get; set; } // Лайк или дизлайк
        public DateTime Timestamp { get; set; }

        public Profile? FromProfile { get; set; } // Навигационное свойство
        public Profile? ToProfile { get; set; }   // Навигационное свойство
    }
}
