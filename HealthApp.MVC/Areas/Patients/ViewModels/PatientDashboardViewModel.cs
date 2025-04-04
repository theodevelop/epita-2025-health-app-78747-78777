using System;
using System.Collections.Generic;
using HealthApp.MVC.Models.Domain;
using HealthApp.MVC.Models.Entities;

namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class PatientDashboardViewModel
    {
        public Patient Patient { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<Appointment> PastAppointments { get; set; }
        public List<Appointment> CancelledAppointments { get; set; }
        public int TotalAppointments { get; set; }
    }
}
