using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace TrackerLibrary
{
    public static class EmailLogic
    {
        public static void SendEmail(string toAddress, string subject, string body)
        {
            SendEmail(new List<string>{toAddress}, new List<string>(), subject, body);
        }

        public static void SendEmail(List<string> toAddresses, List<string>bccAddresses, string subject, string body)
        {
            MailAddress fromMailAddress = new MailAddress(GlobalConfig.LookupAppConfigKey("senderEmail"), GlobalConfig.LookupAppConfigKey("senderDisplayName"));

            MailMessage mail = new MailMessage();
            toAddresses.ForEach(a => mail.To.Add(a));
            bccAddresses.ForEach(bcc => mail.Bcc.Add(bcc));
            mail.From = fromMailAddress;
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();

            client.Send(mail);
        }
    }
}
