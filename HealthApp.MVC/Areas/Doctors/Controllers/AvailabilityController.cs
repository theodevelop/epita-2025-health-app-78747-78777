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

            model.Exceptions = _context.DoctorExceptionAvailabilities
                .Where(e => e.DoctorId == doctor.Id)
                .OrderBy(e => e.StartDateTime)
                .ToList();

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


        public IActionResult Create()
        {
            var model = new ExceptionAvailabilityViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(22)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(ExceptionAvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == userId);
            if (doctor == null)
            {
                return NotFound();
            }

            // Combiner date et heure pour obtenir le DateTime complet
            DateTime newStart = model.StartDate.Date + model.StartTime;
            DateTime newEnd = model.EndDate.Date + model.EndTime;

            if (newEnd <= newStart)
            {
                ModelState.AddModelError("", "The end date/time must be later than the start date/time.");
                return View(model);
            }

            // Rechercher les exceptions existantes qui se chevauchent avec le nouvel intervalle
            var overlappingExceptions = _context.DoctorExceptionAvailabilities
                .Where(e => e.DoctorId == doctor.Id &&
                            e.StartDateTime < newEnd && e.EndDateTime > newStart)
                .ToList();

            // Si une exception identique existe, renvoyer une erreur
            if (overlappingExceptions.Any(e => e.StartDateTime == newStart && e.EndDateTime == newEnd))
            {
                ModelState.AddModelError("", "An identical exception already exists.");
                return View(model);
            }

            // Si des exceptions se chevauchent, les fusionner avec le nouvel intervalle
            if (overlappingExceptions.Any())
            {
                DateTime effectiveStart = newStart;
                DateTime effectiveEnd = newEnd;

                foreach (var ex in overlappingExceptions)
                {
                    if (ex.StartDateTime < effectiveStart)
                    {
                        effectiveStart = ex.StartDateTime;
                    }
                    if (ex.EndDateTime > effectiveEnd)
                    {
                        effectiveEnd = ex.EndDateTime;
                    }
                }

                // Supprimer les exceptions qui se chevauchent
                _context.DoctorExceptionAvailabilities.RemoveRange(overlappingExceptions);

                // Utiliser les bornes fusionnées
                newStart = effectiveStart;
                newEnd = effectiveEnd;
            }

            // Créer et enregistrer la nouvelle exception (fusionnée si applicable)
            var exceptionAvailability = new ExceptionAvailability
            {
                DoctorId = doctor.Id,
                StartDateTime = newStart,
                EndDateTime = newEnd
            };

            _context.DoctorExceptionAvailabilities.Add(exceptionAvailability);
            _context.SaveChanges();

            return RedirectToAction("Index", "Availability");
        }


        public IActionResult Edit(int id)
        {
            var exception = _context.DoctorExceptionAvailabilities.FirstOrDefault(e => e.Id == id);
            if (exception == null)
            {
                return NotFound();
            }

            var model = new ExceptionAvailabilityViewModel
            {
                Id = exception.Id,
                StartDate = exception.StartDateTime.Date,
                StartTime = exception.StartDateTime.TimeOfDay,
                EndDate = exception.EndDateTime.Date,
                EndTime = exception.EndDateTime.TimeOfDay
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ExceptionAvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var exception = _context.DoctorExceptionAvailabilities.FirstOrDefault(e => e.Id == model.Id);
            if (exception == null)
            {
                return NotFound();
            }

            exception.StartDateTime = model.StartDate.Date + model.StartTime;
            exception.EndDateTime = model.EndDate.Date + model.EndTime;

            if (exception.EndDateTime <= exception.StartDateTime)
            {
                ModelState.AddModelError("", "La date/heure de fin doit être postérieure à la date/heure de début.");
                return View(model);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Rechercher l'exception par son ID
            var exception = _context.DoctorExceptionAvailabilities.FirstOrDefault(e => e.Id == id);
            if (exception == null)
            {
                return NotFound();
            }

            // Supprimer l'exception et sauvegarder les modifications
            _context.DoctorExceptionAvailabilities.Remove(exception);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
