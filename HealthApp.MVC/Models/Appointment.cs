using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public enum AppointmentStatus
    {
        Pending,    // En attente d'approbation
        Approved,   // Approuvé par le médecin
        Rejected,   // Rejeté par le médecin
        Cancelled,  // Annulé par le patient
        Completed   // Terminé
    }

    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }
        
        [Required]
        public string Reason { get; set; }
    }
}
