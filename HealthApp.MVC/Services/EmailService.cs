using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HealthApp.MVC.Services
{
    public class EmailService
    {
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string senderEmail = "fridgeupoff@gmail.com";
        private readonly string senderName = "Health App";
        private readonly string smtpUsername = "fridgeupoff@gmail.com";
        private readonly string smtpPassword = "whadboaydyjozqzk"; // Utilise un mot de passe d'application pour Gmail

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
