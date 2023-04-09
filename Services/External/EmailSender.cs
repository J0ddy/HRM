using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Threading.Tasks;

namespace Services.External
{
    public class EmailSender : IEmailSender
    {
        private readonly SendGridClient _sendGridClient;
        private readonly string _sender;
        private readonly string _senderName;

        public EmailSender(string APIKey, string sender, string senderName)
        {
            _sendGridClient = new SendGridClient(APIKey);
            _sender = sender;
            _senderName = senderName;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = new EmailAddress(_sender, _senderName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);
            await _sendGridClient.SendEmailAsync(msg);
        }
    }
}