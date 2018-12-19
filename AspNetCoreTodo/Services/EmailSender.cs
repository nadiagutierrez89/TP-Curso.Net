using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AspNetCoreTodo.Services {
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender {
        public Task SendEmailAsync (string email, string subject, string message) {
            SendMail(email,subject,message);
            return Task.CompletedTask;
        }

        private void SendMail (string email, string subject, string message) {
            try {
                MailMessage mail = new MailMessage ();
                SmtpClient SmtpServer = new SmtpClient ("smtp.gmail.com");

                mail.From = new MailAddress ("serpientofhope@gmail.com");
                mail.To.Add (email);
                mail.Subject = subject;
                mail.Body = message;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential ("MailSenderTP.dotnetNG", "MailSender01");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send (mail);
                Console.WriteLine("mail Send");
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString ());
            }
        }
    }
}