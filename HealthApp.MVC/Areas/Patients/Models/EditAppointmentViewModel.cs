using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Areas.Patients.Models
{
    public class EditAppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        // Facultatif : pour affichage dans le formulaire (champ caché ou readonly)
        [Required]
        public string DoctorName { get; set; }

        [Required(ErrorMessage = "La date est requise.")]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "Le motif est requis.")]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
