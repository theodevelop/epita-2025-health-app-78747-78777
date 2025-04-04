using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class EditAppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public string DoctorName { get; set; }

        [Required(ErrorMessage = "Please select a date and time for the appointment.")]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "Please provide a reason for the appointment.")]
        [StringLength(500, ErrorMessage = "The reason must be 500 characters or less.")]
        public string Reason { get; set; }
    }
}
