using System;
using System.Collections.Generic;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text;

namespace MailAPI.Services
{



    public class EmailFactoryService
    {
        /// <summary>
        /// Creates HTML-enabled Email Message Object for Sending through Email System. Does not escape HTML safely for technical reasons. This should be done prior to calling factory.
        /// </summary>
        /// <param name="associatedApplication">The calling application name, as it registered for the API</param>
        /// <param name="subject">String containing email subject.</param>
        /// <param name="from">Email address the message is from</param>
        /// <param name="body">Escaped HTML string containing message body.</param>
        /// <param name="to">List of emails to send to</param>
        /// <param name="cc">List of CCs to send to</param>
        /// <param name="bcc">List of BCCs to send to</param>
        /// <returns>MimeMessage Object needing destination address(es) added.</returns>
        public static MimeMessage Email(string associatedApplication, string subject, string from, string body, List<string> to, List<string> cc = null, List<string> bcc = null)
        {
            MimeMessage mail = BuildMessageBase(associatedApplication, subject, from, body);

            to.Add(from);

            ProcessTo(mail, to);

            return mail;
        }


        /// <summary>
        /// Attaches CCs to email messages
        /// </summary>
        /// <returns>MimeMessage object</returns>
        internal static MimeMessage BuildMessageBase(string associatedApplication, string from, string subject, string body)
        {
            MimeMessage mail = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart("text/html", body)
            };

            mail.From.Add(new MailboxAddress(associatedApplication, from));
            //Change this to have the correct domain
            mail.Headers.Add("Message-Id", String.Format("<{0}@{1}>", Guid.NewGuid().ToString(), "domain.com"));

            return mail;
        }


        /// <summary>
        /// Processes adding list of email addresses to the to field.
        /// </summary>
        /// <param name="mail">MimeMessage object</param>
        /// <param name="to">List of emails to send to</param>
        /// <returns>Modified MimeMessage object.</returns>
        internal static MimeMessage ProcessTo(MimeMessage mail, List<string> to)
        {
            foreach (string address in to)
            {
                mail.To.Add(new MailboxAddress("Recipient", address));
            }

            return mail;
        }



        /// <summary>
        /// Attaches CCs to email messages
        /// </summary>
        /// <param name="mail">Messages being handled</param>
        /// <param name="ccTo">CC to be attached</param>
        /// <returns>MimeMessage object</returns>
        internal static MimeMessage ProcessCCs(MimeMessage mail, MailboxAddress ccTo)
        {
            mail.Cc.Add(ccTo);

            return mail;
        }

        /// <summary>
        /// Attaches CCs to email messages
        /// </summary>
        /// <param name="mail">Messages being handled</param>
        /// <param name="ccTo">List of CCs to be attached</param>
        /// <returns>MimeMessage object</returns>
        internal static MimeMessage ProcessCCs(MimeMessage mail, List<MailboxAddress> ccTo)
        {
            foreach (MailboxAddress cc in ccTo)
            {
                mail.Cc.Add(cc);
            }

            return mail;
        }


        /// <summary>
        /// Attaches CCs to email messages
        /// </summary>
        /// <param name="mail">Messages being handled</param>
        /// <param name="BccTo">BCC to be attached</param>
        /// <returns>MimeMessage object</returns>
        internal static MimeMessage ProcessBCCs(MimeMessage mail, MailboxAddress BccTo)
        {
            mail.Cc.Add(BccTo);

            return mail;
        }

        /// <summary>
        /// Attaches CCs to email messages
        /// </summary>
        /// <param name="mail">Messages being handled</param>
        /// <param name="BccTo">List of BCCs to be attached</param>
        /// <returns>MimeMessage object</returns>
        internal static MimeMessage ProcessBCCs(MimeMessage mail, List<MailboxAddress> BccTo)
        {
            foreach (MailboxAddress Bcc in BccTo)
            {
                mail.Cc.Add(Bcc);
            }

            return mail;
        }
    }


    /// <summary>
    /// Creates the SMTP Client for use with the Email Factory. Can be disposed of to avoid connection poaching. Singleton implementation.
    /// </summary>
    public class EmailRelay
    {
        public static bool MailSent { get; private set; }
        public static SmtpClient Client { get; private set; }
        public static EmailRelay Instance { get; private set; }
        public static string Error { get; private set; }

        static EmailRelay()
        {
            Instance = new EmailRelay();
            Client = _Client();

            //Client.SendCompleted += Client_SendCompleted;

            MailSent = false;
            //Error.Clear();
        }

        internal static SmtpClient _Client()
        {
            return new SmtpClient();
        }

        /// <summary>
        /// Sends email via Email Relay. 
        /// </summary>
        /// <param name="email">EMimeMessage object from Email Factory</param>
        /// <returns>Type: String with true for success or an error message with details for failure.</returns>
        public string SendMail(MimeMessage email)
        {
            //Documentation at: http://www.mimekit.net/docs/html/T_MailKit_Net_Smtp_SmtpClient.htm
            try
            {
                Client.Connect("server address", 25, MailKit.Security.SecureSocketOptions.StartTls); //Add server address
                Client.Send(email);
                Client.Disconnect(true);
                MailSent = true;
                return MailSent.ToString();

            }
            catch (SmtpCommandException ex)
            {
                //Silent failure to prevent recursion - no error screen issued

                string emailFailed = ex.Mailbox.ToString().Replace("<", "");
                emailFailed = emailFailed.Replace(">", "");

                StringBuilder errorString = new StringBuilder();

                errorString.Append(" Failure for " + emailFailed + ". " + ex.Message + ".");
                errorString.Append("<br/>");

                Error = errorString.ToString();
                MailSent = false;
                return Error;
            }
        }

        //Handle SMTP errors
        //private static void Barracuda_SendCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    MailSent = true;
        //}

        //public void Dispose()
        //{
        //    DisposeInstance();
        //}

        //public static void DisposeInstance()
        //{
        //    if (Instance != null)
        //    {
        //        Instance.Dispose();
        //        Instance = null;
        //    }
        //}
    }


    ////Due to network security this can only be tested on a testing server and higher level machines
    //public class EmailService : IIdentityMessageService
    //{
    //    public async Task SendAsync(IdentityMessage message)
    //    {
    //        MimeMessage mail = EmailFactory.Email(message.Subject, ("<html>" + message.Body + "</html>"));
    //        mail.To.Add(message.Destination.ToString());

    //        BarracudaRelay barracuda = new BarracudaRelay();
    //        await barracuda.SendMail(mail);

    //    }
    //}

}