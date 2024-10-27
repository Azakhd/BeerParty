using BeerParty.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record UserSearchDto(
    string? UserName,
    string? ProfileName,
    int? MinAge,
    int? MaxAge,
    Gender? Gender,
    List<long>? Interests,
    double? MinHeight,
    double? MaxHeight,
    PreferenceType? Preference
);


}
