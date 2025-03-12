using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime DateSent { get; set; } = DateTime.Now;

        // Indique si la notification a déjà été envoyée
        public bool IsSent { get; set; }
    }
}
