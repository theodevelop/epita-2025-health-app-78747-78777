namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class DailyAvailabilityViewModel
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public List<DateTime> AvailableSlots { get; set; } = new List<DateTime>();
    }
}
