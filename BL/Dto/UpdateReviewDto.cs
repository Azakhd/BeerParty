using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.BL.Dto
{
    public record UpdateReviewDto
 (
     long MeetingId,         // Идентификатор встречи
     long reviewId,
     int? Rating = null,     // Рейтинг (необязательный)
     string Comment = null   // Комментарий (необязательный)
 );
}
