using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
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

        //[Required]
        //public ICollection<Specialization> Specializations { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
