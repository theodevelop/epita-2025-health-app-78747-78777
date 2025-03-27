using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace HealthApp.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalPatients = _context.Patients.Count(),
                TotalDoctors = _context.Doctors.Count(),
                TotalAppointments = _context.Appointments.Count(),
                PendingAppointments = _context.Appointments.Count(a => a.Status == AppointmentStatus.Pending),
                ApprovedAppointments = _context.Appointments.Count(a => a.Status == AppointmentStatus.Approved),
                CancelledAppointments = _context.Appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                CompletedAppointments = _context.Appointments.Count(a => a.Status == AppointmentStatus.Completed),
                Notifications = _context.Notifications
                                        .OrderByDescending(n => n.DateSent)
                                        .Take(5)
                                        .ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Doctors(string searchTerm)
        {
            var doctors = _context.Doctors.Include(d => d.Specializations).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                doctors = doctors.Where(d => d.FirstName.Contains(searchTerm) ||
                                               d.LastName.Contains(searchTerm) ||
                                               d.Specializations.Any(s => s.Type.ToString().Contains(searchTerm)))
                                   .ToList();
            }

            return View(doctors);
        }
    }
}
