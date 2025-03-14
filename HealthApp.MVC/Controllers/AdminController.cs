using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data; // Assurez-vous que le namespace du contexte est correct
using HealthApp.MVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HealthApp.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            // Récupère la liste de tous les utilisateurs
            var users = _userManager.Users.ToList();
            return View(users);
        }

        public async Task<IActionResult> ViewAppointments()
        {
            // Récupère les rendez-vous en incluant les informations du patient et du médecin
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();
            return View(appointments);
        }

        // Vous pourrez ajouter d'autres actions pour annuler, éditer, etc.
    }
}
