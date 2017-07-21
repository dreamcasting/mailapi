using Microsoft.AspNetCore.Mvc;
using MailAPI.Services;
using System.Collections.Generic;
using MimeKit;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace MailAPI.Controllers
{
    //Spec
    //    { "AssociatedApplication": "<applicationNamefromAPIKeyRequest>", "Key" : "<APIkey>", "Subject" : "<subjectYouEscapedSafely>", "Body":"<bodyYouEscapedSafely>",
    //    "EmailAddresses": [
    //    {"From":"<address>"},
    //    {"To":"<address>"},
    //    {"To":"<address>"},
    //    {"Cc":"<address>"},
    //    {"Cc":"<address>"},
    //    {"Bcc":"<address>"},
    //    {"Bcc":"<address>"}
    //    ]
    //}
    [NotMapped]
    public class EmailAddresses
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
    }

    [NotMapped]
    public class EmailTransmission
    {
        public string AssociatedApplication { get; set; }
        public string Key { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<EmailAddresses> EmailAddresses { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentContent { get; set; }
    }


    public class Message : Controller
    {
        public EmailTransmission Transmission;
        private ApiKeyController _ApiKeyController;
        private readonly EmailRelay _relay;
        private bool _lockOut;
        private readonly IHostingEnvironment _hostingEnvironment;


        public string Email { get; set; }

        //This class expects a JSON string
        public Message(ApiKeyController apiKeycontroller, EmailRelay relay, IHostingEnvironment hostingEnvironment, EmailTransmission transmission)
        {
            Transmission = transmission;
            _ApiKeyController = apiKeycontroller;
            _relay = relay;
            _hostingEnvironment = hostingEnvironment;
        }

        private bool _LockOut(string associatedApplication, string apiKey)
        {
            return _ApiKeyController.Validate(associatedApplication, apiKey).Result;
        }

        public string SendEmail(EmailTransmission transmission)
        {
            if (_lockOut == false)
            {

                MimeMessage message = EmailFactoryService.BuildMessageBase(transmission.AssociatedApplication, transmission.EmailAddresses[0].From, transmission.Subject, transmission.Body);

                //Process CC, BCC, From, To
                foreach (var plainAddress in transmission.EmailAddresses)
                {

                    if (plainAddress.To != null)
                    {
                        message.To.Add(new MailboxAddress("", plainAddress.To));
                    }

                    //if (plainAddress.From != null)
                    //{
                    //    message.From.Add(new MailboxAddress("", plainAddress.From));
                    //}

                    if (plainAddress.Cc != null)
                    {
                        message.Cc.Add(new MailboxAddress("", plainAddress.Cc));
                    }

                    if (plainAddress.Bcc != null)
                    {
                        message.Bcc.Add(new MailboxAddress("", plainAddress.Bcc));
                    }

                }

                //Any Attachments?
                // create an image attachment for the file located at path
                if (transmission.AttachmentContent != null)
                {
                    byte[] file = Convert.FromBase64String(transmission.AttachmentContent);
                    var stream = new MemoryStream(file);
                    stream.Position = 0;
                    FileStream fileContents = new FileStream(transmission.AttachmentName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                    stream.CopyTo(fileContents);
                    
                    var attachment = new MimePart()
                    {
                        ContentObject = new ContentObject(fileContents),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(fileContents.Name)
                    };

                    //Alter message to include attachment
                    var body = new BodyBuilder { HtmlBody = transmission.Body };
                    body.Attachments.Add(attachment);
                    message.Body = body.ToMessageBody();


                    //var multipart = new Multipart("mixed");
                    //multipart.Add(new TextPart("html") { Text = transmission.Body });
                    //multipart.Add(attachment);
                    //message.Body = multipart;

                    var result = _relay.SendMail(message);
                    fileContents.Dispose();
                    try
                    {
                        System.IO.File.Delete(_hostingEnvironment.ContentRootPath + "\\" + transmission.AttachmentName);
                    }
                    catch (Exception ex)
                    {
                        return ex.InnerException.ToString();
                    }

                    return result;
                }
                else
                {
                    ////Build and send message
                    var body = new BodyBuilder { HtmlBody = transmission.Body };
                    message.Body = body.ToMessageBody();
                    return _relay.SendMail(message);
                }


            }
            else
            {
                return "Unauthorized application";
            }


        }

        [HttpGet]
        public string Index()
        {
            return "Access denied";
        }

        [HttpPost]
        public string Index([FromBody]dynamic email)
        {

            EmailTransmission transmission = new EmailTransmission();

            if (email != null)
            {

                //EmailTransmission transmission = JsonConvert.DeserializeObject<EmailTransmission>(email);

                //Have to manually bind - YES, I wanted this to be neater. No it isn't how I want it.
                //Newtonsoft had real trouble with a nested list. I had to do it manually for whatever reason.

                transmission.AssociatedApplication = email["AssociatedApplication"];
                transmission.Key = email["Key"];
                transmission.Subject = email["Subject"];
                transmission.Body = email["Body"];
                transmission.EmailAddresses = email["EmailAddresses"].ToObject<List<EmailAddresses>>();
                transmission.AttachmentName = email["AttachmentName"];
                transmission.AttachmentContent = email["AttachmentContent"];

                _lockOut = _LockOut(transmission.AssociatedApplication, transmission.Key);

            }
            else
            {
                _lockOut = true;
            }

            return SendEmail(transmission);

        }
    }
}
