using HealthApp.MVC.Areas.Patients.ViewModels;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthApp.MVC.Areas.Patients.Controllers
{
    [Area("Patients")]
    [Authorize(Roles = "Patient")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string query, string specialization, string status, DateTime? date)
        {
            // Récupérer le patient connecté
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
            {
                return NotFound("Patient non trouvé.");
            }

            // Construire la requête de base
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specializations)
                .Where(a => a.PatientId == patient.Id)
                .AsQueryable();

            // Filtrer par référence (ici on utilise l'ID converti en string)
            if (!string.IsNullOrEmpty(query))
            {
                appointmentsQuery = appointmentsQuery.Where(a =>
                    a.Id.ToString().Contains(query) ||
                    (a.Doctor != null &&
                     (a.Doctor.FirstName.Contains(query) || a.Doctor.LastName.Contains(query))));
            }

            // Filtrer par spécialisation
            if (!string.IsNullOrEmpty(specialization))
            {
                appointmentsQuery = appointmentsQuery.Where(a =>
                    a.Doctor.Specializations.Any(s => s.Type.ToString() == specialization));
            }

            // Filtrer par statut
            if (!string.IsNullOrEmpty(status))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.Status.ToString() == status);
            }

            // Filtrer par date (comparaison sur la partie date uniquement)
            if (date.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.Date.Date == date.Value.Date);
            }

            // Ordonner les rendez-vous par date
            var appointments = appointmentsQuery.OrderBy(a => a.Date).ToList();

            return View(appointments);
        }

        public IActionResult Book(string query)
        {
            var doctors = _context.Doctors
                .Include(d => d.Specializations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                string lowerQuery = query.ToLower();
                doctors = doctors.Where(d =>
                    d.FirstName.ToLower().Contains(lowerQuery) ||
                    d.LastName.ToLower().Contains(lowerQuery) ||
                    d.Specializations.Any(s => s.Name.ToLower().Contains(lowerQuery))
                );
            }

            var doctorList = doctors.ToList();

            // Si c'est une requête AJAX (tape d'un caractère), on renvoie la partial _DoctorList
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DoctorList", doctorList);
            }

            // Sinon, on renvoie la vue Book avec le modèle complet
            return View(doctorList);
        }

        [HttpGet]
        public IActionResult MonthlyCalendar(int doctorId, DateTime? month)
        {
            DateTime selectedMonth = month ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ViewBag.DoctorId = doctorId;
            var viewModel = BuildMonthlyCalendar(doctorId, selectedMonth);
            return View(viewModel);
        }


        private MonthlyCalendarViewModel BuildMonthlyCalendar(int doctorId, DateTime selectedMonth)
        {
            var viewModel = new MonthlyCalendarViewModel
            {
                DoctorId = doctorId,
                Month = new DateTime(selectedMonth.Year, selectedMonth.Month, 1),
            };

            DateTime firstDayOfMonth = viewModel.Month;
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int firstDayWeekday = (int)firstDayOfMonth.DayOfWeek;

            var weeks = new List<List<CalendarDay?>>();
            List<CalendarDay?> currentWeek = new List<CalendarDay?>();

            for (int i = 0; i < firstDayWeekday; i++)
            {
                currentWeek.Add(null);
            }

            for (DateTime date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var calendarDay = new CalendarDay
                {
                    Date = date,
                    HasAvailability = HasAvailabilityForDay(doctorId, date),
                    IsToday = date.Date == DateTime.Today
                };
                currentWeek.Add(calendarDay);
                if (currentWeek.Count == 7)
                {
                    weeks.Add(currentWeek);
                    currentWeek = new List<CalendarDay?>();
                }
            }

            if (currentWeek.Any())
            {
                while (currentWeek.Count < 7)
                {
                    currentWeek.Add(null);
                }

                weeks.Add(currentWeek);
            }

            viewModel.Weeks = weeks;
            return viewModel;
        }

        private bool HasAvailabilityForDay(int doctorId, DateTime date)
        {
            // On suppose ici que la disponibilité par défaut s'applique de 8h à 22h.
            // Vérifier d'abord si le docteur a une disponibilité par défaut pour ce jour.
            bool defaultAvailable = _context.DoctorAvailabilities.Any(a =>
                a.DoctorId == doctorId &&
                a.SpecificDate == null &&
                a.DayOfWeek == date.DayOfWeek);

            // Récupérer les exceptions qui couvrent ce jour
            var exceptions = _context.DoctorExceptionAvailabilities
                .Where(e => e.DoctorId == doctorId &&
                            e.StartDateTime.Date <= date.Date && e.EndDateTime.Date >= date.Date)
                .ToList();

            // Si une exception couvre la plage complète (de 8h à 22h), on considère que le docteur est absent pour la journée
            bool fullDayAbsence = exceptions.Any(e =>
                e.StartDateTime.TimeOfDay <= TimeSpan.FromHours(8) &&
                e.EndDateTime.TimeOfDay >= TimeSpan.FromHours(22));

            if (fullDayAbsence)
            {
                return false;
            }

            // Sinon, si le docteur a une disponibilité par défaut et qu'il n'y a pas d'absence totale, on retourne true.
            return defaultAvailable;
        }


        [HttpGet]
        public IActionResult DayDetails(DateTime date, int doctorId)
        {
            ViewBag.DoctorId = doctorId;
            var availableSlots = GetAvailableSlotsForDate(doctorId, date);
            return PartialView("_DayDetails", availableSlots);
        }

        private List<DateTime> GetAvailableSlotsForDate(int doctorId, DateTime date)
        {
            // On suppose ici que le docteur travaille par défaut de 8h à 22h, mais on va lire les plages définies dans DoctorAvailabilities.
            var defaultAvailabilities = _context.DoctorAvailabilities
                .Where(a => a.DoctorId == doctorId &&
                            a.SpecificDate == null &&
                            a.DayOfWeek == date.DayOfWeek)
                .AsEnumerable()  // Evaluation côté client pour le tri sur TimeSpan
                .OrderBy(a => a.StartTime)
                .ToList();

            if (!defaultAvailabilities.Any())
            {
                return new List<DateTime>();
            }

            // Récupérer les exceptions pour ce jour
            var exceptions = _context.DoctorExceptionAvailabilities
                .Where(e => e.DoctorId == doctorId &&
                            e.StartDateTime.Date <= date.Date &&
                            e.EndDateTime.Date >= date.Date)
                .ToList();

            List<DateTime> slots = new List<DateTime>();

            // Pour chaque plage de disponibilités par défaut, générer les créneaux disponibles
            foreach (var availability in defaultAvailabilities)
            {
                DateTime intervalStart = date.Date + availability.StartTime;
                DateTime intervalEnd = date.Date + availability.EndTime;

                for (DateTime slot = intervalStart; slot.AddHours(1) <= intervalEnd; slot = slot.AddHours(1))
                {
                    // Vérifier si ce créneau tombe dans une exception
                    bool isException = exceptions.Any(e =>
                        slot >= e.StartDateTime && slot.AddHours(1) <= e.EndDateTime);

                    if (!isException)
                    {
                        // Vérifier si le créneau n'est pas déjà réservé
                        bool slotBooked = _context.Appointments.Any(a =>
                            a.DoctorId == doctorId &&
                            a.Date == slot &&
                            a.Status != AppointmentStatus.Cancelled);

                        if (!slotBooked)
                        {
                            slots.Add(slot);
                        }
                    }
                }
            }

            return slots;
        }

        [HttpGet]
        public IActionResult FinalizeBooking(int doctorId, DateTime selectedDate)
        {
            // Récupérer le docteur concerné
            var doctor = _context.Doctors.Find(doctorId);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var model = new FinalizeBookingViewModel
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                SelectedDate = selectedDate
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult FinalizeBooking(FinalizeBookingViewModel model)
        {
            // Vérifier la validité du modèle
            if (!ModelState.IsValid)
            {
                // Réassigner le nom du docteur pour afficher le formulaire en cas d'erreur
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = $"{doctor.FirstName} {doctor.LastName}";
                }
                return View(model);
            }

            // Vérifier que le créneau n'est pas déjà réservé pour ce docteur
            bool slotBooked = _context.Appointments.Any(a =>
                a.DoctorId == model.DoctorId &&
                a.Date == model.SelectedDate &&
                a.Status != AppointmentStatus.Cancelled);

            if (slotBooked)
            {
                ModelState.AddModelError("", "The selected time slot is no longer available.");
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = $"{doctor.FirstName} {doctor.LastName}";
                }
                return View(model);
            }

            // Récupérer le patient connecté via l'ID de l'utilisateur
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Créer le rendez-vous avec le statut "Pending"
            var appointment = new Appointment
            {
                DoctorId = model.DoctorId,
                Doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == userId),
                Date = model.SelectedDate,
                Reason = model.Reason,
                Status = AppointmentStatus.Pending,
                PatientId = patient.Id,
                Patient = patient
            };

            _context.Appointments.Add(appointment);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while booking the appointment: " + ex.Message);
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = $"{doctor.FirstName} {doctor.LastName}";
                }
                return View(model);
            }

            // Rediriger vers la page de confirmation avec l'ID du rendez-vous
            return RedirectToAction("Confirmation", new { id = appointment.Id });
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            // Récupérer le rendez-vous en incluant le médecin pour affichage
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            // On inclut le docteur et ses spécialisations pour avoir toutes les informations nécessaires
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specializations)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }



        [HttpGet]
        public IActionResult ChangeSlot(int appointmentId, DateTime? month)
        {
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            DateTime selectedMonth = month ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var calendar = BuildMonthlyCalendar(appointment.DoctorId, selectedMonth);

            var model = new ChangeAppointmentSlotViewModel
            {
                AppointmentId = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                CurrentSlot = appointment.Date,
                Reason = appointment.Reason,
                Calendar = calendar
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ChangeSlot(ChangeAppointmentSlotViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Calendar = BuildMonthlyCalendar(model.DoctorId, new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
                return View(model);
            }

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == model.AppointmentId);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            appointment.Date = model.NewSlot;
            appointment.Reason = model.Reason;

            _context.SaveChanges();

            return RedirectToAction("Details", new { id = appointment.Id });
        }

        [HttpGet]
        public IActionResult GetAvailableSlotsForDay(DateTime date, int doctorId)
        {
            // Implémentez la logique pour récupérer les créneaux disponibles pour ce jour
            // Par exemple, en utilisant votre méthode existante GetAvailableSlotsForDate
            var slots = GetAvailableSlotsForDate(doctorId, date); // Retourne List<DateTime>

            // Convertir chaque créneau au format "HH:mm"
            var result = slots.Select(s => s.ToString("HH:mm")).ToList();
            return Json(result);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Vérifier la règle de 48 heures pour l'annulation
            if (appointment.Date <= DateTime.Now.AddHours(48))
            {
                TempData["Error"] = "Annulation impossible moins de 48 heures avant le rendez-vous.";
                return RedirectToAction("Details", new { id = id });
            }

            appointment.Status = AppointmentStatus.Cancelled;
            _context.Appointments.Update(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Votre rendez-vous a bien été annulé.";
            return RedirectToAction("Index");
        }

    }
}
