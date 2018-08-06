using System;
using System.Collections.Generic;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommonService.Models
{
    public class SMSSender
    {
        private static string accountSid = "ACc535ae2edcb4ee69002d984c3b513f83";
        private static string authToken = "98c8a65688d630861ff46c64fafae221";
        private static string fromPhoneNumber = "+14437752019";

        public static async Task<string> SendSMSAsync(string recevier, string content)
        {
            TwilioClient.Init(accountSid, authToken);

            var to = new PhoneNumber(recevier);
            var message = await MessageResource.CreateAsync(
                to,
                from: new PhoneNumber(fromPhoneNumber),
                body: content);

            if (message.ErrorCode == null)
            {
                return "success";
            }
            else
            {
                return message.ErrorMessage;
            }
        }
    }
}
