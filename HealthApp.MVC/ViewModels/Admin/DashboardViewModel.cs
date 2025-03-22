using System.Collections.Generic;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<Appointment> RecentAppointments { get; set; }
        public List<Notification> Notifications { get; set; }

        // For Chart.js data:
        public int JanCount { get; set; }
        public int FebCount { get; set; }
        public int MarCount { get; set; }
        public int AprCount { get; set; }
        public int MayCount { get; set; }
        public int JunCount { get; set; }
        public int JulCount { get; set; }
        public int AugCount { get; set; }
        public int SepCount { get; set; }
        public int OctCount { get; set; }
        public int NovCount { get; set; }
        public int DecCount { get; set; }



    }
}
