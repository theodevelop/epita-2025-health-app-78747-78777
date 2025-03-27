namespace HealthApp.MVC.Areas.Admin.ViewModels.Doctors
{
    public class AvailabilityInputModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class UnavailabilityInputModel
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Reason { get; set; }
    }
}
