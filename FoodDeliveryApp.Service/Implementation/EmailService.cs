using FoodDeliveryApp.Domain.Email;
using FoodDeliveryApp.Service.Interface;
using MimeKit;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using FoodDeliveryApp.Repository;

namespace FoodDeliveryApp.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        public EmailService() { }
        public async Task SendEmailMessage(EmailMessage message)
        {
            var emailmessage = new MimeMessage
            {
                Sender = new MailboxAddress("FoodieDeliveries", "gresalika66@gmail.com"),
                Subject = message.Subject

            };
            emailmessage.From.Add(new MailboxAddress("FoodieDeliveries", "gresalika66@gmail.com"));
            emailmessage.To.Add(new MailboxAddress(message.MailTo, message.MailTo));
            emailmessage.Body= new TextPart(MimeKit.Text.TextFormat.Plain) { Text =message.Content};
            try {

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    var socketOptions = SecureSocketOptions.Auto;

                    await smtp.ConnectAsync("smtp.gmail.com", 587, socketOptions);

                    if (!string.IsNullOrEmpty("gresalika66@gmail.com"))
                    {
                        await smtp.AuthenticateAsync("gresalika66@gmail.com", "ipoo mbuo nsnx ohch");
                    }
                    await smtp.SendAsync(emailmessage);


                    await smtp.DisconnectAsync(true);
                }


            } catch (SmtpException ex) {
                throw ex;
                    }

            _context.EmailMessages.Add(message);
            await _context.SaveChangesAsync();

           
        }
    }
}
