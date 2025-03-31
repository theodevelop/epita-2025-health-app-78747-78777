using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Areas.Doctors.ViewModels
{
    public class DashboardViewModel
    {
        public int UpcomingAppointmentsCount { get; set; }
        public int PendingAppointmentsCount { get; set; }
        public int FinishedAppointmentsCount { get; set; }
        public List<Appointment> WeeklyAppointments { get; set; } = new List<Appointment>();
    }
}
