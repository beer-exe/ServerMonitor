using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ServerMonitorApp.Application.Common.Interfaces;

namespace ServerMonitorApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string message, bool isHtml = true)
        {
            MimeMessage? emailMessage = new MimeMessage();

            string? fromEmail = _configuration["EmailSettings:FromEmail"];
            string? fromName = _configuration["EmailSettings:FromName"];

            emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;

            BodyBuilder? bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = message;
            }
            else
            {
                bodyBuilder.TextBody = message;
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using SmtpClient? client = new SmtpClient();

            string? smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out int port) ? port : 587;
            string? smtpUser = _configuration["EmailSettings:SmtpUser"];
            string? smtpPass = _configuration["EmailSettings:SmtpPass"];

            try
            {
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(emailMessage);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}