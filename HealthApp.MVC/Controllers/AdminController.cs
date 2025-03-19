using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.ViewModels;
using System.Linq;
using HealthApp.MVC.ViewModels.Admin;


namespace HealthApp.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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

        //public AdminController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        //public IActionResult Index()
        //{
        //    return View();
        //}
        //public IActionResult Users()
        //{
        //    var users = _context.Users.ToList();
        //    return View(users);
        //}
        //public IActionResult UserRoles(string id)
        //{
        //    var user = _context.Users.FirstOrDefault(u => u.Id == id);
        //    var userRoles = _context.UserRoles.Where(ur => ur.UserId == id).ToList();
        //    var roles = _context.Roles.ToList();
        //    var viewModel = new UserRolesViewModel
        //    {
        //        User = user,
        //        UserRoles = userRoles,
        //        Roles = roles
        //    };
        //    return View(viewModel);
        //}
        //public IActionResult AddRole(string userId, string roleId)
        //{
        //    var userRole = new IdentityUserRole<string>
        //    {
        //        UserId = userId,
        //        RoleId = roleId
        //    };
        //    _context.UserRoles.Add(userRole);
        //    _context.SaveChanges();
        //    return RedirectToAction("UserRoles", new { id = userId });
        //}
        //public IActionResult RemoveRole(string userId, string roleId)
        //{
        //    var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
        //    _context.UserRoles.Remove(userRole);
        //    _context.SaveChanges();
        //    return RedirectToAction("UserRoles", new { id = userId });
        //}
    }
}
