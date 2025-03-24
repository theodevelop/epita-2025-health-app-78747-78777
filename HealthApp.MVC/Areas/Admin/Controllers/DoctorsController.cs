using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Entities;
using System.Linq;
using System.Threading.Tasks;


namespace HealthApp.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Affiche la liste des médecins, avec possibilité de filtrer par nom ou spécialisation
        public async Task<IActionResult> Index(string searchTerm)
        {
            var doctors = _context.Doctors.Include(d => d.Specializations).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                doctors = doctors.Where(d => d.FirstName.Contains(searchTerm)
                    || d.LastName.Contains(searchTerm)
                    || d.Specializations.Any(s => s.Type.ToString().Contains(searchTerm)));
            }

            return View(await doctors.ToListAsync());
        }

        // Affiche le formulaire de création d'un nouveau médecin
        public IActionResult Create()
        {
            // Charge la liste des spécialisations pour la sélection multiple dans la vue
            ViewBag.Specializations = _context.Specializations.ToList();
            return View();
        }

        // Traite la soumission du formulaire de création
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor, int[] selectedSpecializationIds)
        {
            if (ModelState.IsValid)
            {
                if (selectedSpecializationIds != null && selectedSpecializationIds.Any())
                {
                    doctor.Specializations = await _context.Specializations
                        .Where(s => selectedSpecializationIds.Contains(s.Id))
                        .ToListAsync();
                }
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // En cas d'erreur, recharge les spécialisations
            ViewBag.Specializations = _context.Specializations.ToList();
            return View(doctor);
        }

        // Affiche le formulaire d'édition d'un médecin existant
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors.Include(d => d.Specializations)
                                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            ViewBag.Specializations = _context.Specializations.ToList();
            return View(doctor);
        }

        // Traite la soumission du formulaire d'édition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor, int[] selectedSpecializationIds)
        {
            if (id != doctor.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var doctorToUpdate = await _context.Doctors.Include(d => d.Specializations)
                                            .FirstOrDefaultAsync(d => d.Id == id);
                    if (doctorToUpdate == null)
                        return NotFound();

                    // Mise à jour des champs
                    doctorToUpdate.FirstName = doctor.FirstName;
                    doctorToUpdate.LastName = doctor.LastName;
                    doctorToUpdate.Email = doctor.Email;

                    // Mise à jour des spécialisations
                    doctorToUpdate.Specializations.Clear();
                    if (selectedSpecializationIds != null && selectedSpecializationIds.Any())
                    {
                        var specializations = await _context.Specializations
                            .Where(s => selectedSpecializationIds.Contains(s.Id))
                            .ToListAsync();
                        doctorToUpdate.Specializations = specializations;
                    }
                    _context.Update(doctorToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Specializations = _context.Specializations.ToList();
            return View(doctor);
        }

        // Affiche la confirmation de suppression d'un médecin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await _context.Doctors.Include(d => d.Specializations)
                                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // Traite la suppression confirmée
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}
