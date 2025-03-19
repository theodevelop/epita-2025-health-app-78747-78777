using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(20)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        public string RoleType { get; set; }
    }
}
