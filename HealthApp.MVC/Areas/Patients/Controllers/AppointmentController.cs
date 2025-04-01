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
                else
                {
                    doctors = doctors.Where(d => d.FirstName.Contains(query) || d.LastName.Contains(query));
                }
            }

            var doctorList = doctors.ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DoctorList", doctorList);
            }
            return View(doctorList);
        }

        // GET: Patients/Appointment/DoctorSchedule?doctorId=1
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

        // GET: Patients/Appointment/FinalizeBooking?doctorId=1&selectedDate=...
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
        [HttpPost]
        public IActionResult FinalizeBooking(FinalizeBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var doctor = _context.Doctors.Find(model.DoctorId);
                if (doctor != null)
                {
                    model.DoctorName = doctor.FirstName + " " + doctor.LastName;
                }
                return View(model);
            }

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
            return RedirectToAction("Confirmation", new { id = appointment.Id });
        }

        // GET: Patients/Appointment/Confirmation/5
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
        public IActionResult List(AppointmentStatus? status)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
                return NotFound("Patient non trouvé.");

            var appointmentsQuery = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id);

            if (status.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.Status == status.Value);
            }

            ViewBag.FilterStatus = status?.ToString();
            var appointments = appointmentsQuery.OrderBy(a => a.Date).ToList();

            return View(appointments);
        }


        // GET: Patients/Appointment/Details/5
        public IActionResult Details(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: Patients/Appointment/Edit/5
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            if (appointment.Date <= DateTime.Now.AddHours(48))
            {
                TempData["Error"] = "Modification impossible moins de 48 heures avant le rendez-vous.";
                return RedirectToAction("Details", new { id = id });
            }

            var model = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.FirstName + " " + appointment.Doctor.LastName,
                SelectedDate = appointment.Date,
                Reason = appointment.Reason
            };

            ViewBag.Doctors = _context.Doctors.ToList();
            return View(model);
        }

        // POST: Patients/Appointment/Edit/5
        [HttpPost]
        public IActionResult Edit(EditAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = _context.Doctors.ToList();
                return View(model);
            }

            // Sécurité : s'assurer que l'ID est bien reçu
            if (model.Id == 0)
            {
                ModelState.AddModelError("", "ID du rendez-vous introuvable.");
                ViewBag.Doctors = _context.Doctors.ToList();
                return View(model);
            }

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == model.Id);
            if (appointment == null)
                return NotFound();

            // 48h avant vérif
            if (appointment.Date <= DateTime.Now.AddHours(48))
            {
                ModelState.AddModelError("", "Modification impossible moins de 48 heures avant le rendez-vous.");
                ViewBag.Doctors = _context.Doctors.ToList();
                return View(model);
            }

            appointment.DoctorId = model.DoctorId;
            appointment.Date = model.SelectedDate;
            appointment.Reason = model.Reason;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = appointment.Id });
        }

        // GET: Patients/Appointment/Cancel/5
        public IActionResult Cancel(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            if (appointment.Date <= DateTime.Now.AddHours(48))
            {
                TempData["Error"] = "Annulation impossible moins de 48 heures avant le rendez-vous.";
                return RedirectToAction("Details", new { id = id });
            }

            appointment.Status = AppointmentStatus.Cancelled;
            _context.Appointments.Update(appointment);
            _context.SaveChanges();

            return RedirectToAction("List");
        }

        private List<DateTime> GetAvailableSlots(int doctorId)
        {
            List<DateTime> slots = new List<DateTime>();
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

        