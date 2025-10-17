using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit.Text;

using System.Text;
using System.Threading.Tasks;
using EMRS.Application.Abstractions;

namespace EMRS.Infrastructure.Services
{
    public  class EmailService:IEmailService
    {

      

        public async Task SendVerificationEmailAsync(string toEmail, string verificationCode,
            int minutesToExpire)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EMRS Support", Environment.GetEnvironmentVariable("locbpse182530@fpt.edu.vn")));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "Email Verification - EMRS";


                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <html>
                    <body style='font-family:Arial, sans-serif;'>
                        <div style='max-width:600px;margin:auto;padding:20px;border-radius:10px;background-color:#f8f9fa;'>
                            <h2 style='color:#007bff;'>Welcome to EMRS!</h2>
                            <p>Dear user,</p>
                            <p>Your verification code is:</p>
                            <h1 style='color:#28a745;text-align:center;'>{verificationCode}</h1>
                            <p>This code will expire in <b>{minutesToExpire} minutes</b>.</p>
                            <p>Thank you,<br/>The EMRS Team</p>
                        </div>
                    </body>
                    </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    Environment.GetEnvironmentVariable("EMAIL_SMTPSERVER"),
                    int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT")),
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    Environment.GetEnvironmentVariable("EMAIL_SENDER")
                    , Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
