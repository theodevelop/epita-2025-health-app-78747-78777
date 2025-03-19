using HealthApp.MVC.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models.Entities
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

        //[Required]
        //[Phone]
        //public string Phone { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        public string IdentityUserId { get; set; }
        public ApplicationUser IdentityUser { get; set; }
    }
}
