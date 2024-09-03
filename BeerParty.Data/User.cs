using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data
{
    public class User
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string FirstName { get; set; }   

        public string Biography { get; set; }
        public string LastName { get; set; }
         
        public string Gender { get; set; }
        public string Location { get; set; }

        public DateTime Created { get; set; }





    }
}
