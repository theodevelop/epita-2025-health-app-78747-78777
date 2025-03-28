using System.Collections.Generic;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        // --- Compteurs globaux ---
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int CompletedAppointments { get; set; }

        // --- Listes d'objets ou notifications récentes ---
        public List<Appointment> RecentAppointments { get; set; }
        public List<Notification> Notifications { get; set; }

        // --- Comptage mensuel pour rendez-vous (ou autre) ---
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

        // --- Propriétés utiles pour Chart.js ---
        public List<string> ChartLabels
        {
            get
            {
                // Par exemple : "Jan", "Feb", etc.
                return new List<string> {
                    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
                };
            }
        }

        public List<int> ChartData
        {
            get
            {
                return new List<int> {
                    JanCount, FebCount, MarCount, AprCount, MayCount, JunCount,
                    JulCount, AugCount, SepCount, OctCount, NovCount, DecCount
                };
            }
        }
    }
}
