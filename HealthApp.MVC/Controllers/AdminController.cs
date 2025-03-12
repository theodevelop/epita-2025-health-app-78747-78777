using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HealthApp.MVC.Models;


namespace HealthApp.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Liste des utilisateurs
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users;
            return View(users);
        }

        // Modifie le rôle d'un utilisateur
        [HttpGet("Admin/ManageRoles/{id}")]
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("L'ID de l'utilisateur est manquant.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable.");
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.Select(r => r.Name).ToList(),
                UserRoles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRoles(string id, string role)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role))
            {
                return BadRequest("L'ID ou le rôle est manquant.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction("Users");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
