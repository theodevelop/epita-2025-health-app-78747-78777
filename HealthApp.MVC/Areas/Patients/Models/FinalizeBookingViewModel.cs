using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Patients.Models
{
    public class FinalizeBookingViewModel
    {
        [Required]
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }

        [Required]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "Veuillez fournir une raison.")]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
