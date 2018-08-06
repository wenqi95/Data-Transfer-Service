using Newtonsoft.Json;
using System;

namespace CfgTool.Models
{
    public class MobileConnStateModel
    {
        [JsonProperty(PropertyName = "MobileConnState")]
        public String MobileConnState;
    }
}
