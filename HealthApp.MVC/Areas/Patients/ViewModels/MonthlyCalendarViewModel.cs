namespace HealthApp.MVC.Areas.Patients.ViewModels
{
    public class MonthlyCalendarViewModel
    {
        public DateTime Month { get; set; }
        public int DoctorId { get; set; }

        public List<List<CalendarDay?>> Weeks { get; set; } = new List<List<CalendarDay?>>();
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool HasAvailability { get; set; }
        public bool IsToday { get; set; }
    }
}
