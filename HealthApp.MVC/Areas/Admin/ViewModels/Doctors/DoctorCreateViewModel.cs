using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Admin.ViewModels.Doctors
{
    public class DoctorCreateViewModel
    {
        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // Liste des IDs des spécialisations sélectionnées
        public int[] SelectedSpecializationIds { get; set; }
    }
}
