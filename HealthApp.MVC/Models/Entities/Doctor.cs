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
        public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public string IdentityUserId { get; set; }
        public ApplicationUser IdentityUser { get; set; }

        public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    }
}
