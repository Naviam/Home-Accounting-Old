using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using log4net;
using System.Net.Mime;
using System.Web.UI.WebControls;

namespace Naviam.WebUI.Helpers
{
    public class EmailHelper
    {
        /*
                 public static bool SendMail(string subject, string recipients, string message, string from)
        {
            var sc = new SmtpClient("smtp.gmail.com")
                         {
                             EnableSsl = true,
                             Credentials =
                                 //new NetworkCredential("alert@naviam.com", "ruinruin")
                                 new NetworkCredential("pavel.mironchik@gmail.com", "pvtqUjhsysx150")
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
         */


        /// <summary>
        /// Send SMS transaction alerts to Email
        /// </summary>
        /// <param name="subject">Subject of email</param>
        /// <param name="recipients">Whom message should be sent to</param>
        /// <param name="message">Body of email</param>
        public static bool SendMail(string subject, string recipients, string message, string from)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Content\Naviam_logo1.png";
            return SendMail(subject, recipients, message, from, path);
        }

        public static bool SendMail(string subject, string recipients, string message, string from, string picPath)
        {
            //return true;

            var sc = new SmtpClient("smtp.gmail.com")
                         {
                             EnableSsl = true,
                             Credentials =
                                 //new NetworkCredential("alert@naviam.com", "ruinruin")
                                 new NetworkCredential("pavel.mironchik@gmail.com", "pvtqUjhsysx150")
                         };
            try
            {
                //MailDefinition md = new MailDefinition();
                MailMessage mess = new MailMessage();
                mess.IsBodyHtml = true;
                mess.BodyEncoding = System.Text.Encoding.GetEncoding(1251);
                mess.From = new MailAddress(from);
                mess.To.Add(recipients);
                mess.Subject = subject;
                Attachment attach = new Attachment(picPath);// mess.Attachments.Add( AddAttachment("d:\\test.gif"); 
                string contentID = Guid.NewGuid().ToString();
                attach.ContentId = contentID;
                mess.Body = String.Format("<html><body><img src=\"cid:{0}\"><div>{1}</div></body></html>", contentID, message);
                mess.Attachments.Add(attach);
                sc.Send(mess);
                //sc.Send(from, recipients, subject, message);
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