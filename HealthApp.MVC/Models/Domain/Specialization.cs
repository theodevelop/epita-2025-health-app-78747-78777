using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HealthApp.MVC.Models.Entities;

namespace HealthApp.MVC.Models.Domain
{
    public enum SpecializationType
    {
        GeneralPractice,
        Cardiology,
        Dermatology,
        Neurology,
        Pediatrics,
        Gynecology,
        Orthopedics,
        Ophthalmology,
        Gastroenterology,
        Pulmonology,
        Endocrinology,
        Oncology,
        Urology,
        Psychiatry,
        GeneralSurgery,
        Radiology,
        Rheumatology,
        Nephrology,
        Anesthesiology,
        InfectiousDiseases,
        InternalMedicine
    }

    public class Specialization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public SpecializationType Type { get; set; }

        public string Description { get; set; }

        public ICollection<Doctor> Doctors { get; set; }
    }
}
