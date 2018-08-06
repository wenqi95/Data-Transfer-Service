using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonService.Models
{
    public class MailSenderModel
    {
        [JsonProperty("receiver")]
        public string receiver { get; set; }
        [JsonProperty("content")]
        public string content { get; set; }
        [JsonProperty("subject")]
        public string subject { get; set; }
    }
}
