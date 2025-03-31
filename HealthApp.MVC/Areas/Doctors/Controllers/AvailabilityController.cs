using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using System.Security.Claims;
using HealthApp.MVC.Areas.Doctors.ViewModels;

namespace HealthApp.MVC.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class AvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == userId);
            if (doctor == null)
            {
                return NotFound("No doctor found");
            }

            var model = new WeeklyAvailabilityViewModel();

            int startHour = model.StartHour;
            int endHour = model.EndHour;
            int columns = endHour - startHour;

            for (int day = 0; day < 7; day++)
            {
                DayOfWeek actualDay = (DayOfWeek)day;

                var availabilitiesForDay = _context.DoctorAvailabilities
                    .Where(a => a.DoctorId == doctor.Id && a.SpecificDate == null && a.DayOfWeek == actualDay)
                    .ToList();
                
                for (int col = 0; col < columns; col++)
                {
                    int hour = startHour + col;
                    bool isAvailable = availabilitiesForDay.Any(a => a.StartTime.Hours == hour && a.EndTime.Hours > hour);
                    model.Availability[day][col] = isAvailable;
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(WeeklyAvailabilityViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == userId);
            if (doctor == null)
            {
                return NotFound("No doctor found");
            }

            var existingAvailabities = _context.DoctorAvailabilities
                .Where(a => a.DoctorId == doctor.Id && a.SpecificDate == null)
                .ToList();
            _context.DoctorAvailabilities.RemoveRange(existingAvailabities);

            int startHour = model.StartHour;
            int endHour = model.EndHour;
            int columns = endHour - startHour;

            for (int day = 0; day < 7; day++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (model.Availability[day][col])
                    {
                        var availability = new DoctorAvailability
                        {
                            DoctorId = doctor.Id,
                            SpecificDate = null,
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = TimeSpan.FromHours(startHour + col),
                            EndTime = TimeSpan.FromHours(startHour + col + 1)
                        };
                        _context.DoctorAvailabilities.Add(availability);
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
