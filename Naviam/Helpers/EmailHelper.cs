using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using log4net;

namespace Naviam.WebUI.Helpers
{
    public class EmailHelper
    {
        /// <summary>
        /// Send SMS transaction alerts to Email
        /// </summary>
        /// <param name="subject">Subject of email</param>
        /// <param name="recipients">Whom message should be sent to</param>
        /// <param name="message">Body of email</param>
        public static bool SendMail(string subject, string recipients, string message, string from)
        {
            var sc = new SmtpClient("smtp.gmail.com")
                         {
                             EnableSsl = true,
                             Credentials =
                                 new NetworkCredential("alert@naviam.com", "ruinruin")
                         };
            try
            {
                sc.Send(from, recipients, subject, message);
                return true;
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger("navSite");
                log.Debug(e.Message);
                throw;
            }
        }

        //public static bool SendMail(string subject, string recipients, string message)
        //{
        //    try
        //    {
        //        var sc = new SmtpClient("smtp.gmail.com")
        //        {
        //            EnableSsl = true,
        //            Credentials =
        //                new NetworkCredential("alert@naviam.com", "ruinruin")
        //        };

        //        MailMessage mess = new MailMessage();
        //        mess.BodyEncoding = System.Text.Encoding.GetEncoding(1251);
        //        mess.To.Add(recipients);
        //        mess.From = new MailAddress(@"alert@naviam.com");
        //        mess.Subject = subject;
        //        mess.Body = message;
        //        sc.Send(mess);
        //    }
        //    catch (Exception e)
        //    {
        //        var log = LogManager.GetLogger("navSite");
        //        log.Debug(e.Message);
        //        throw;
        //    }
        //    return true;
        //}
    }
}