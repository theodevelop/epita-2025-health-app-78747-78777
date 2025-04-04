using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class ChangeAppointmentSlotViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime CurrentSlot { get; set; }

        [Required(ErrorMessage = "Please select a new appointment slot.")]
        public DateTime NewSlot { get; set; }

        [Required(ErrorMessage = "Please provide a reason.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; }

        public MonthlyCalendarViewModel? Calendar { get; set; }
    }
}
