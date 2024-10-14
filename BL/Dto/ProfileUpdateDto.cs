using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record ProfileUpdateDto(string FirstName, string LastName,
        List<int> InterestIds, double Height, string PhotoUrl, string Bio, string Location);


}
