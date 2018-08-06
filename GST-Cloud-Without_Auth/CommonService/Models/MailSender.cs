using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http;

namespace CommonService.Models
{
    public class MailSender
    {
        //***********SendGridKey*************
        private static string sendgridKey = "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";

        public static async Task<string> SendMailAsync(string receiver, string sub,string content)
        {
            return await SendMailExecute(receiver, sub, content);
        }

        static async Task<string> SendMailExecute(string receiver, string sub, string content)
        {
            var apiKey = sendgridKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("admin@example.com", "admin");
            var subject = sub;
            var to = new EmailAddress(receiver);
            var plainTextContent = content;
            var htmlContent = content;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return "success";
            }
            else
                return await response.Body.ReadAsStringAsync();
        }
    } 
}
