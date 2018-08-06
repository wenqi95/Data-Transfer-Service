using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class DownloadProcessModel
    {
        [JsonProperty(PropertyName = "MobileDownloadProcess")]
        public double MobileDownloadProcess;
    }
}
