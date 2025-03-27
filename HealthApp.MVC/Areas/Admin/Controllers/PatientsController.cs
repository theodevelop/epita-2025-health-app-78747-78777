using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Areas.Admin.ViewModels.Patients;
using System.Linq;
using System.Threading.Tasks;

namespace HealthApp.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context; 
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Patients
        public async Task<IActionResult> Index(string searchTerm)
        {
            var patients = _context.Patients.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                patients = patients.Where(p => p.FirstName.Contains(searchTerm)
                                              || p.LastName.Contains(searchTerm)
                                              || p.Email.Contains(searchTerm));
            }

            return View(await patients.ToListAsync());
        }

        // GET: Admin/Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Créer le compte ApplicationUser associé
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    RoleType = "Patient"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Patient");

                    var patient = new Patient
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Birthdate = model.Birthdate,
                        IdentityUserId = user.Id,
                        IdentityUser = user
                    };

                    _context.Patients.Add(patient);
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
            return View(model);
        }

        // GET: Admin/Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();

            var viewModel = new PatientEditViewModel
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Birthdate = patient.Birthdate
            };

            return View(viewModel);
        }


        // POST: Admin/Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PatientEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (patient == null)
                return NotFound();

            // Mise à jour des informations du patient
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.Email = model.Email;
            patient.Birthdate = model.Birthdate;

            _context.Update(patient);
            await _context.SaveChangesAsync();

            // Optionnel : mettre à jour les informations de l'utilisateur associé
            var user = await _userManager.FindByIdAsync(patient.IdentityUserId);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Patients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}