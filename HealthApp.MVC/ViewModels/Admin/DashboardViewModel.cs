using System.Collections.Generic;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<Notification> Notifications { get; set; }
    }
}
