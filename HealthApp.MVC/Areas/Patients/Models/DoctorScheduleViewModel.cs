using System;
using System.Collections.Generic;
using HealthApp.MVC.Models.Entities;

namespace HealthApp.MVC.Areas.Patients.Models
{
    public class DoctorScheduleViewModel
    {
        public Doctor Doctor { get; set; }
        public List<DateTime> AvailableSlots { get; set; }
    }
}
