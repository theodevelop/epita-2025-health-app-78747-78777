using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Areas.Doctors.ViewModels
{
    public class WeeklyAvailabilityViewModel
    {
        // Availability[day][hour]
        public bool[][] Availability { get; set; }

        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 22;

        public WeeklyAvailabilityViewModel()
        {
            int columns = EndHour - StartHour;
            Availability = new bool[7][];
            for (int i = 0; i < 7; i++)
            {
                Availability[i] = new bool[columns];
            }
        }

        public IEnumerable<ExceptionAvailability> Exceptions { get; set; }

    }
}
