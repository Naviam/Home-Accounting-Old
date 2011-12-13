using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Web.Configuration;
using System.Net.Configuration;
using log4net;

namespace Naviam.NotificationCenter
{
    public class NotificationManager
    {
        private static volatile NotificationManager instance;
        private static object syncRoot = new Object();

        protected SmtpClient _smtpClient;
        protected const string MAIL_SETTINGS_SECTION_GROUP_NAME = "system.net/mailSettings";
        protected MailSettingsSectionGroup _mailSettings;

        private NotificationManager()
        {
            Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            _mailSettings = configurationFile.GetSectionGroup(MAIL_SETTINGS_SECTION_GROUP_NAME) as MailSettingsSectionGroup;

            _smtpClient = new SmtpClient(_mailSettings.Smtp.Network.Host)
            {
                EnableSsl = _mailSettings.Smtp.Network.EnableSsl,
                Credentials = new NetworkCredential(_mailSettings.Smtp.Network.UserName, _mailSettings.Smtp.Network.Password)
            };
        }

        public static NotificationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NotificationManager();
                    }
                }
                return instance;
            }
        }

        public bool SendSmsMail(string recipients, string message)
        {
            return SendSmsMail(recipients, message, false);
        }

        public bool SendSmsMail(string recipients, string message, bool useMessage)
        {
            string messageText = string.Empty;
            string contentID = string.Empty;
            string picPath = string.Empty;
            try
            {
                if (useMessage)
                {
                    contentID = Guid.NewGuid().ToString();
                    picPath = AppDomain.CurrentDomain.BaseDirectory + @"Content\Naviam_logo2.png";
                    messageText = String.Format("<html><body><img src=\"cid:{0}\"><div>{1}</div></body></html>", contentID, message);
                }
                else
                    messageText = String.Format("<html><body>{0}</body></html>", message);

                MailMessage mess = GetMailMessage(recipients, _mailSettings.Smtp.Network.UserName, "subject", messageText, picPath, true, contentID);
                SendMail(mess);
                return true;
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger("navSite");
                log.Debug(e.Message);
                throw;
            }

        }

        public bool SendConfirmationRegistrationMail(string recipients, string approveCode, string email)
        {
            try
            {
                string contentID = Guid.NewGuid().ToString();
                string messageText = GetConfirmationRegistrationMessage(approveCode, email, contentID);
                string picPath = AppDomain.CurrentDomain.BaseDirectory + @"Content\Naviam_logo2.png";
                MailMessage mess = GetMailMessage(recipients, _mailSettings.Smtp.Network.UserName, Naviam.WebUI.Resources.Mails.ConfirmationMailSubject, messageText, picPath, true, contentID);
                SendMail(mess);
                return true;
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger("navSite");
                log.Debug(e.Message);
                throw;
            }
        }

        private string GetConfirmationRegistrationMessage(string approveCode, string email, string contentID)
        {
            Uri url = HttpContext.Current.Request.Url;
            string path = url.Scheme + @"://" + url.Authority;
            string confirmationLink = string.Format(@"{2}/Account/Confirmation?acc={0}&email={1}", approveCode, email, path);
            string message = string.Format(Naviam.WebUI.Resources.Mails.ConfirmationMail, confirmationLink, approveCode);
            string messageText = String.Format("<html><body><img src=\"cid:{0}\"><div>{1}</div></body></html>", contentID, message);
            return messageText;
        }

        private MailMessage GetMailMessage(string recipients, string from, string subject, string message, string picturePath, bool isBodyHtml, string contentID)
        {
            MailMessage mess = new MailMessage();
            mess.IsBodyHtml = isBodyHtml;
            mess.BodyEncoding = System.Text.Encoding.UTF8;
            mess.SubjectEncoding = System.Text.Encoding.UTF8;
            mess.From = new MailAddress(from);
            mess.To.Add(recipients);
            mess.Subject = subject;
            if (!string.IsNullOrEmpty(picturePath))
            {
                Attachment attach = new Attachment(picturePath);
                attach.ContentId = contentID;
                mess.Attachments.Add(attach);
            }
            mess.Body = message;
            return mess;
        }

        private void SendMail(MailMessage message)
        {
            _smtpClient.Send(message);
        }
    }
}