using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HealthApp.MVC.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        // Holds the IDs of the selected specializations in the form
        public List<int> SelectedSpecializationIds { get; set; } = new List<int>();

        // List of all available specializations to populate the dropdown or multi-select
        public List<Specialization> Specializations { get; set; } = new List<Specialization>();
    }
}
