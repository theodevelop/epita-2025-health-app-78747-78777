using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using HealthApp.MVC.Areas.Patients.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.MVC.Areas.Patients.Controllers
{
    [Area("Patients")]
    [Authorize(Roles = "Patient")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients/Appointment/Book
        // Affiche la barre de recherche et la liste des médecins filtrés
        public IActionResult Book(string query, string searchMode)
        {
            var doctors = _context.Doctors
                .Include(d => d.Specializations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                if (searchMode == "Specialization")
                {
                    doctors = doctors.Where(d => d.Specializations.Any(s => s.Name.Contains(query)));
                }
                else  // Par défaut, recherche par nom/prénom
                {
                    doctors = doctors.Where(d => d.FirstName.Contains(query) || d.LastName.Contains(query));
                }
            }

            var doctorList = doctors.ToList();

            // Si la requête est AJAX, renvoyer la vue partielle
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DoctorList", doctorList);
            }

            // Sinon, renvoyer la vue complète
            return View(doctorList);
        }


        // GET: Patients/Appointment/DoctorSchedule?doctorId=1
        // Affiche le planning du médecin (créneaux disponibles)
        public IActionResult DoctorSchedule(int doctorId)
        {
            var doctor = _context.Doctors
                .Include(d => d.Specializations)
                .FirstOrDefault(d => d.Id == doctorId);
            if (doctor == null)
                return NotFound();

            var availableSlots = GetAvailableSlots(doctorId);

            var viewModel = new DoctorScheduleViewModel
            {
                Doctor = doctor,
                AvailableSlots = availableSlots
            };

            return View(viewModel);
        }

        // GET: Patients/Appointment/FinalizeBooking?doctorId=1&selectedDate=2025-04-01T09:00:00
        // Affiche le formulaire pour finaliser la réservation
        public IActionResult FinalizeBooking(int doctorId, DateTime selectedDate)
        {
            var doctor = _context.Doctors.Find(doctorId);
            if (doctor == null)
                return NotFound();

            var model = new FinalizeBookingViewModel
            {
                DoctorId = doctorId,
                DoctorName = doctor.FirstName + " " + doctor.LastName,
                SelectedDate = selectedDate
            };
            return View(model);
        }

        // POST: Patients/Appointment/FinalizeBooking
        // Enregistre le rendez-vous après validation du formulaire
        [HttpPost]
        public IActionResult FinalizeBooking(FinalizeBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-remplir le nom du médecin pour réafficher la vue correctement
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = doctor.FirstName + " " + doctor.LastName;
                }
                return View(model);
            }

            // Vérifier que le créneau est toujours disponible
            bool exists = _context.Appointments.Any(a =>
                a.DoctorId == model.DoctorId &&
                a.Date == model.SelectedDate &&
                a.Status != AppointmentStatus.Cancelled);

            if (exists)
            {
                ModelState.AddModelError("", "Créneau déjà réservé.");
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = doctor.FirstName + " " + doctor.LastName;
                }
                return View(model);
            }

            // Récupérer l'ID du patient connecté
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
            {
                return NotFound("Patient non trouvé.");
            }

            var appointment = new Appointment
            {
                DoctorId = model.DoctorId,
                Date = model.SelectedDate,
                Reason = model.Reason,
                Status = AppointmentStatus.Pending,
                PatientId = patient.Id
            };

            _context.Appointments.Add(appointment);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erreur lors de l'enregistrement : " + ex.Message);
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = doctor.FirstName + " " + doctor.LastName;
                }
                return View(model);
            }

            // Après enregistrement, rediriger vers l'action List qui affiche tous les rendez-vous du patient
            return RedirectToAction("List");
        }

        // GET: Patients/Appointment/Confirmation/5
        // Affiche le récapitulatif du rendez-vous
        public IActionResult Confirmation(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: Patients/Appointment/List
        // Affiche la liste des rendez-vous du patient connecté
        public IActionResult List(int? year, int? month)
        {
            DateTime now = DateTime.Now;
            // Si aucune valeur n'est fournie, on utilise l'année et le mois en cours
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Récupérer le patient connecté
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
            {
                return NotFound("Patient non trouvé.");
            }

            // Récupérer les rendez-vous du mois sélectionné
            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id
                            && a.Date >= firstDayOfMonth
                            && a.Date <= lastDayOfMonth)
                .ToList();

            // Passer les paramètres sélectionnés à la vue
            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;

            return View(appointments);
        }


        // Méthode privée pour générer les créneaux disponibles pour les 7 prochains jours
        private List<DateTime> GetAvailableSlots(int doctorId)
        {
            List<DateTime> slots = new List<DateTime>();
            // Exemple : horaires de travail de 9h à 17h avec des créneaux de 30 minutes
            for (int day = 0; day < 7; day++)
            {
                DateTime dayStart = DateTime.Today.AddDays(day).AddHours(9);
                DateTime dayEnd = DateTime.Today.AddDays(day).AddHours(17);
                for (DateTime slot = dayStart; slot < dayEnd; slot = slot.AddMinutes(30))
                {
                    if (slot > DateTime.Now)
                    {
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
    }
}
