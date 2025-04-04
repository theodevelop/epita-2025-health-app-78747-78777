using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using HealthApp.MVC.Areas.Patients.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using HealthApp.MVC.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.MVC.Areas.Patients.Controllers
{
    [Area("Patients")]
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Récupérer l'utilisateur connecté
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == userId);
            if (patient == null)
                return NotFound("Patient non trouvé.");

            var appointmentsQuery = _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patient.Id);

            // Récupérer les rendez-vous du patient
            var upcomingAppointments = appointmentsQuery
                .Where(a => a.Date >= DateTime.Now && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.Date)
                .ToList();

            var pastAppointments = appointmentsQuery
                .Where(a => a.Date < DateTime.Now && a.Status != AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.Date)
                .ToList();

            var cancelledAppointments = appointmentsQuery
                .Where(a => a.Status == AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.Date)
                .ToList();

            var viewModel = new PatientDashboardViewModel
            {
                Patient = patient,
                UpcomingAppointments = upcomingAppointments,
                PastAppointments = pastAppointments,
                CancelledAppointments = cancelledAppointments,
                TotalAppointments = _context.Appointments.Count(a => a.PatientId == patient.Id)
            };

            return View(viewModel);
        }
    }
}
