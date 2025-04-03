using System;
using System.Threading.Tasks;
using HealthApp.MVC.Data;
using HealthApp.MVC.Models.Domain;

namespace HealthApp.MVC.Services
{
    public class NotificationManager
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public NotificationManager(ApplicationDbContext context)
        {
            _context = context;
            _emailService = new EmailService(); // ou injecte-le si tu préfères
        }

        public async Task SendAndLogAsync(string toEmail, string subject, string body, string contentReference)
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);

            var notification = new Notification
            {
                Title = subject,
                Content = contentReference,
                IsSent = true,
                DateSent = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public bool HasNotificationBeenSent(string title, string reference)
        {
            return _context.Notifications.Any(n =>
                n.Title == title && n.Content.Contains(reference));
        }
    }
}
