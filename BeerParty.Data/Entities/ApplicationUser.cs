using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public decimal? MoneyAmount { get; set; }
    }
}
