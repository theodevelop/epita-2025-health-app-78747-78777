using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class FinalizeBookingViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        public string DoctorName { get; set; }

        [Required]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "Please provide a reason for your appointment.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; }
    }
}
