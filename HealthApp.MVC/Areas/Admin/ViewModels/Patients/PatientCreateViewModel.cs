using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Admin.ViewModels.Patients
{
    public class PatientCreateViewModel
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
        public DateTime Birthdate { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
