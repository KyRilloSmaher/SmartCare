using Microsoft.Extensions.Logging;
using MimeKit;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Domain.Helpers;
using MailKit.Net.Smtp;
using MimeKit;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.InfraStructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IClientRepository _userRepository;

        public EmailService(
            EmailSettings emailSettings,
            ILogger<EmailService> logger,
            IClientRepository userRepository)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                //sending the Message of passwordResetLink
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, true);
                    client.Authenticate(_emailSettings.FromEmail, _emailSettings.Password);
                    var bodybuilder = new BodyBuilder
                    {
                        HtmlBody = $"{message}",
                        TextBody = "welcome",
                    };
                    var Message = new MimeMessage
                    {
                        Body = bodybuilder.ToMessageBody()
                    };
                    Message.From.Add(new MailboxAddress("SmartCare Team", _emailSettings.FromEmail));
                    Message.To.Add(new MailboxAddress("testing", email));
                    Message.Subject = subject ?? "Not Submitted";
                    await client.SendAsync(Message);
                    await client.DisconnectAsync(true);
                }
                //end of sending email
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(string email, string callbackUrl)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"Attempted to send confirmation email to non-existent user: {email}");
                return false;
            }

            var subject = SystemMessages.SUBJECT_EMAIL_CONFIRMATION;
            var message = SystemMessages.CONFIRMATIONEMAIL_TEMPLATE
                                            .Replace("{{UserName}}", user.UserName)
                                            .Replace("{{CallbackUrl}}", callbackUrl)
                                            .Replace("{{Year}}", DateTime.Now.Year.ToString());
            return await SendEmailAsync(email, subject, message);
        }


        public async Task<bool> SendPasswordResetEmailAsync(string email, string subject, string code)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"Attempted to send password reset email to non-existent user: {email}");
                return false;
            }
            var Subject = SystemMessages.SUBJECT_PASSWORD_RESET;
            var message = SystemMessages.RESETPASSWORD_TEMPLATE
                                            .Replace("{{UserName}}", user.UserName)
                                            .Replace("{{Code}}", code)
                                            .Replace("{{Year}}", DateTime.Now.Year.ToString());
            return await SendEmailAsync(email, Subject, message);
        }
    }
}
