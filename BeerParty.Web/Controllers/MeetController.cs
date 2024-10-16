using BeerParty.Data;
using BeerParty.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BeerParty.Data.Entities.Meet;

namespace BeerParty.Web.Controllers
{
    public class MeetController : Controller
    {
        private readonly ApplicationContext _context;

        public MeetController(ApplicationContext context)
        {
            _context = context;
        }

        // Метод для отображения формы создания встречи
        public IActionResult Create()
        {
            // Загружаем доступных пользователей, чтобы отобразить их в форме
            var users = _context.Users.ToList();
            ViewBag.AvailableUsers = users;

            return View(new MeetEntity());
        }

        // Метод для обработки данных формы создания встречи
        [HttpPost]
        public IActionResult Create(MeetEntity meet, List<long> invitedUserIds)
        {
            if (ModelState.IsValid)
            {
                // Найти приглашенных пользователей по ID
                var invitedUsers = _context.Users.Where(u => invitedUserIds.Contains(u.Id)).ToList();

                // Присваиваем данные встрече
                meet.InvitedUsers = invitedUsers;
                meet.OrganizerId = GetCurrentUserId(); // метод для получения текущего пользователя

                // Сохраняем встречу
                _context.MeetEntities.Add(meet);
                _context.SaveChanges();

                return RedirectToAction("Index"); // перенаправление на список встреч или другую страницу
            }

            // Если модель недействительна, загрузим доступных пользователей снова
            ViewBag.AvailableUsers = _context.Users.ToList();
            return View(meet);
        }

        private long GetCurrentUserId()
        {
            // Реализуйте логику получения текущего пользователя
            return 1; // Пример с возвращением ID пользователя
        }
    }
}
