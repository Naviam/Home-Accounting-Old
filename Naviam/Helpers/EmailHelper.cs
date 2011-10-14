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
        public static bool SendSmsAlert(string subject, string recipients, string message)
        {
            var sc = new SmtpClient("smtp.gmail.com")
                         {
                             EnableSsl = true,
                             Credentials =
                                 new NetworkCredential("alert@naviam.com", "ruinruin")
                         };
            try
            {
                sc.Send("sms@naviam.com", recipients, subject, message);
                return true;
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger("navSite");
                log.Debug(e.Message);
                throw;
            }
        }
    }
}