using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models.Domain
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime DateSent { get; set; } = DateTime.Now;

        public bool IsSent { get; set; } = false;
    }
}
