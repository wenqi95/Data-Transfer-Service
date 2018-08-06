using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class MobileConnStateModel
    {
        [JsonProperty(PropertyName = "MobileConnState")]
        public String MobileConnState;
    }
}
