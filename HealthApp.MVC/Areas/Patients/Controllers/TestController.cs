using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Data;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Areas.Patients.Controllers
{
    [Area("Patients")]
    [Route("Patients/Test")]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ForceReminder")]
        public async Task<IActionResult> ForceReminder()
        {
            var now = DateTime.Now;
            var windowStart = now.AddMinutes(2);
            var windowEnd = now.AddMinutes(10);

            var emailService = new EmailService();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a =>
                    a.Status == AppointmentStatus.Approved &&
                    a.Date >= windowStart &&
                    a.Date <= windowEnd)
                .ToListAsync();

            int sent = 0;

            foreach (var appointment in appointments)
            {
                string subject = "⏰ Appointment Reminder - 24h Notice";
                string body = $"Dear {appointment.Patient.FirstName},<br/><br/>" +
                              $"This is a reminder for your appointment with Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}.<br/><br/>" +
                              $"📅 Date: <strong>{appointment.Date:dddd, MMMM dd, yyyy at HH:mm}</strong><br/>" +
                              $"📝 Reason: {appointment.Reason}<br/><br/>" +
                              $"Please be on time.<br/><em>Health App Team</em>";

                await emailService.SendEmailAsync(appointment.Patient.Email, subject, body);

                _context.Notifications.Add(new HealthApp.MVC.Models.Domain.Notification
                {
                    Title = "Appointment Reminder",
                    Content = $"You have an appointment with Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName} tomorrow at {appointment.Date:HH:mm}.",
                    DateSent = DateTime.Now,
                    IsSent = true
                });

                sent++;
            }

            await _context.SaveChangesAsync();

            return Content($"✅ {sent} reminder(s) sent manually.");
        }
    }
}
