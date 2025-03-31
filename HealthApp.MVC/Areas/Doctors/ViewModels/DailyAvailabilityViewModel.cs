namespace HealthApp.MVC.Areas.Doctors.ViewModels
{
    public class DailyAvailabilityViewModel
    {
        public DateTime Date { get; set; }

        public bool[] Availability { get; set; }

        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 22;

        public DailyAvailabilityViewModel()
        {
            int columns = EndHour - StartHour;
            Availability = new bool[columns];
        }
    }
}
