using HealthApp.MVC.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models.Domain
{
    public enum AppointmentStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Completed
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
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
