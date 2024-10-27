using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record MeetingUpdateDto(
      string? Title,
      DateTime? MeetingTime,
      string? Location,
      string? Description,
      bool? IsPublic,
      long? CoAuthorId,
      short? ParticipantLimit,
      IFormFile? Photo,
      double? Latitude,   // Новое поле для широты
      double? Longitude,  // Новое поле для долготы
      double? Radius      // Новое поле для радиуса
  );

}
