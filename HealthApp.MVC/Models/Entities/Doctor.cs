using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Models.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        //[Required]
        //[Phone]
        //public string Phone { get; set; }

        [Required]
        public ICollection<Specialization> Specializations { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        // public string IdentityUserId { get; set; }
        // public ApplicationUser IdentityUser { get; set; }
    }
}
