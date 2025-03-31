using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.Areas.Doctors.ViewModels;
using System.Security.Claims;

namespace HealthApp.MVC.Areas.Doctors.Controllers
{
    [Area("Doctors")]
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == userId); 
            if (doctor == null) 
            { 
                return NotFound("Doctor not found"); 
            }

            int doctorId = doctor.Id;
            var now = DateTime.Now;

            var allAppointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToList();

            var upcomingAppointmentsCount = allAppointments.Count(a => a.Date >= now && a.Status == AppointmentStatus.Approved);
            var finishedAppointmentsCount = allAppointments.Count(a => a.Status == AppointmentStatus.Completed);
            var pendingAppointmentsCount = allAppointments.Count(a => a.Status == AppointmentStatus.Pending);

            var weeklyAppointments = allAppointments
                .Where(a => a.Date >= now && a.Date <= now.AddDays(7))
                .OrderBy(a => a.Date)
                .ToList();
            
            var model = new DashboardViewModel
            {
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                FinishedAppointmentsCount = finishedAppointmentsCount,
                PendingAppointmentsCount = pendingAppointmentsCount,
                WeeklyAppointments = weeklyAppointments
            };

            return View(model);
        }
    }
}
