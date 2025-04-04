using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Areas.Admin.ViewModels.Doctors;
using HealthApp.MVC.Areas.Admin.ViewModels;
using HealthApp.MVC.Models.Domain;


namespace HealthApp.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Doctors
        public async Task<IActionResult> Index(string searchTerm)
        {
            var doctors = _context.Doctors
                                  .Include(d => d.Specializations)
                                  .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                doctors = doctors.Where(d => d.FirstName.Contains(searchTerm)
                                            || d.LastName.Contains(searchTerm)
                                            || d.Specializations.Any(s => s.Name.Contains(searchTerm)
                                                                       || s.Name.Contains(searchTerm)));
            }

            return View(await doctors.ToListAsync());
        }

        // GET: Admin/Doctors/Create
        public IActionResult Create()
        {
            ViewBag.Specializations = _context.Specializations.ToList();
            return View();
        }

        // POST: Admin/Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    RoleType = "Doctor"
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Doctor");

                    var doctor = new Doctor
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        IdentityUserId = user.Id,
                        IdentityUser = user
                    };

                    if (model.SelectedSpecializationIds != null && model.SelectedSpecializationIds.Any())
                    {
                        doctor.Specializations = await _context.Specializations
                            .Where(s => model.SelectedSpecializationIds.Contains(s.Id))
                            .ToListAsync();
                    }

                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    int startHour = 9;
                    int endHour = 17;

                    // En .NET, DayOfWeek.Monday = 1 et DayOfWeek.Friday = 5
                    for (int day = 1; day <= 5; day++)
                    {
                        for (int hour = startHour; hour < endHour; hour++)
                        {
                            var availability = new DoctorAvailability
                            {
                                DoctorId = doctor.Id,
                                SpecificDate = null,
                                DayOfWeek = (DayOfWeek)day,
                                StartTime = TimeSpan.FromHours(hour),
                                EndTime = TimeSpan.FromHours(hour + 1)
                            };
                            _context.DoctorAvailabilities.Add(availability);
                        }
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            ViewBag.Specializations = _context.Specializations.ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await _context.Doctors
                                .Include(d => d.Specializations)
                                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound();

            var viewModel = new DoctorEditViewModel
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                SelectedSpecializationIds = doctor.Specializations.Select(s => s.Id).ToArray(),
                AllSpecializations = await _context.Specializations.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recharge la liste des spécialisations pour la vue
                model.AllSpecializations = await _context.Specializations.ToListAsync();
                return View(model);
            }

            var doctor = await _context.Doctors
                                .Include(d => d.Specializations)
                                .FirstOrDefaultAsync(d => d.Id == model.Id);
            if (doctor == null)
                return NotFound();

            // Mettre à jour les propriétés du Doctor
            doctor.FirstName = model.FirstName;
            doctor.LastName = model.LastName;
            doctor.Email = model.Email;

            // Mettre à jour les spécialisations
            doctor.Specializations.Clear();
            if (model.SelectedSpecializationIds != null && model.SelectedSpecializationIds.Any())
            {
                var selectedSpecs = await _context.Specializations
                    .Where(s => model.SelectedSpecializationIds.Contains(s.Id))
                    .ToListAsync();
                doctor.Specializations = selectedSpecs;
            }

            _context.Update(doctor);
            await _context.SaveChangesAsync();

            // Mettre à jour le compte ApplicationUser associé si nécessaire
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var user = await _userManager.FindByIdAsync(doctor.IdentityUserId);
                if (user != null)
                {
                    // Mettre à jour les informations de base du user
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    // Pour changer le mot de passe, nous générons un token et utilisons ResetPasswordAsync
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        model.AllSpecializations = await _context.Specializations.ToListAsync();
                        return View(model);
                    }
                    await _userManager.UpdateAsync(user);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Récupérer l'ApplicationUser associé
            var user = await _userManager.FindByIdAsync(doctor.IdentityUserId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "La suppression de l'utilisateur associé a échoué.");
                    return View("Error");
                }
            }

            _context.Doctors.Remove(doctor);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // L'entité n'existe probablement plus en base,
                // on peut loguer le problème et rediriger sans lever d'exception.
                // Vous pouvez aussi recharger l'entité pour vérifier.
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

    }
}
