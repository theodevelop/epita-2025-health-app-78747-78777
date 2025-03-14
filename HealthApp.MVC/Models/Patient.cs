using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class Patient
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
        public DateTime Birthdate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        public string IdentityUserId { get; set; }
    }
}
