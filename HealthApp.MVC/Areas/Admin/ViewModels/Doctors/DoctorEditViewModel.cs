using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HealthApp.MVC.Areas.Admin.ViewModels.Doctors;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.Models.Entities;

namespace HealthApp.MVC.Areas.Admin.ViewModels
{
    public class DoctorEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        // Les IDs des spécialisations sélectionnées dans le formulaire
        public int[]? SelectedSpecializationIds { get; set; }
        public IEnumerable<Specialization>? AllSpecializations { get; set; }
    }
}
