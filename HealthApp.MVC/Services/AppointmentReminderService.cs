using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthApp.MVC.Data;
using Microsoft.EntityFrameworkCore;
using HealthApp.MVC.Services;
using HealthApp.MVC.Models.Domain;

public class AppointmentReminderService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;

    public AppointmentReminderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Exécute la tâche toutes les heures
        _timer = new Timer(SendReminders, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private async void SendReminders(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            Console.WriteLine("🔄 Checking for reminders...");
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = new EmailService();

            var now = DateTime.Now;
            var reminderWindowStart = now.AddHours(23.5);
            var reminderWindowEnd = now.AddMinutes(24.5);

            var appointments = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a =>
                    a.Status == AppointmentStatus.Approved &&
                    a.Date >= reminderWindowStart &&
                    a.Date <= reminderWindowEnd)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var patient = appointment.Patient;
                var doctor = appointment.Doctor;

                string subject = "⏰ Appointment Reminder - 24h Notice";
                string body = $"Dear {patient.FirstName},<br/><br/>" +
                              $"This is a reminder for your appointment with Dr. {doctor.FirstName} {doctor.LastName}.<br/><br/>" +
                              $"📅 Date: <strong>{appointment.Date:dddd, MMMM dd, yyyy at HH:mm}</strong><br/>" +
                              $"📝 Reason: {appointment.Reason}<br/><br/>" +
                              $"Please be on time.<br/><em>Health App Team</em>";

                // Send Email
                await emailService.SendEmailAsync(patient.Email, subject, body);

                // Log notification in DB
                var notification = new Notification
                {
                    Title = "Appointment Reminder",
                    Content = $"You have an appointment with Dr. {doctor.FirstName} {doctor.LastName} tomorrow at {appointment.Date:HH:mm}.",
                    DateSent = DateTime.Now,
                    IsSent = true
                };

                context.Notifications.Add(notification);
            }

            await context.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
