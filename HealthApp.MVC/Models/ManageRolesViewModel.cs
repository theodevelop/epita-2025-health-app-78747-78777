using System.Collections.Generic;

namespace HealthApp.MVC.Models
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public IList<string> UserRoles { get; set; }    
    }
}
