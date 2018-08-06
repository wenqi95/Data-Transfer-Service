using Newtonsoft.Json;
using System;

namespace Authorization.Models
{
    public class EmailModel
    {
        [JsonProperty(PropertyName = "receiver")]
        public String receiver;

        [JsonProperty(PropertyName = "content")]
        public String content;

        [JsonProperty(PropertyName = "subject")]
        public String subject;

    }
}
