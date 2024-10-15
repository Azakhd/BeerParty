using BeerParty.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record ProfileUpdateDto(string FirstName, string LastName,
        List<long> InterestIds, double Height, string PhotoUrl, string Bio,
        string Location, DateTime DateOfBirth, Gender Gender);


}
