using System;
using System.Net;
using System.Net.Mail;
using Common;
using Common.Interfaces;
using Server.Utils;

namespace Server
{
    public class EmailSender : IEmailService, IDisposable
    {
        private SmtpClient client;

        public EmailSender(string host, string username, string password, int port)
        {
            client = new SmtpClient
            {
                Port = port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Host = host,
                Credentials = new NetworkCredential(username, EncryptionUtils.Unprotect(password))
            };
        }

        #region IDisposable Members

        public void Dispose()
        {
            
                client?.Dispose();
                client = null;
            
        }

        #endregion

        #region IEmailService Members

        public void Send(string from, string to, string subject, string body)
        {
            var mail = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body
            };

            client.Send(mail);
        }

        #endregion
    }
}