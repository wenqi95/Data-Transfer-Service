using Newtonsoft.Json;
using System;

namespace CfgTool.Models
{
    public class DownloadProcessModel
    {
        [JsonProperty(PropertyName = "MobileDownloadProcess")]
        public double MobileDownloadProcess;
    }
}
