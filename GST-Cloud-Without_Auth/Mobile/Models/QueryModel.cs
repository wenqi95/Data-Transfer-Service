using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class QueryModel
    {
        [JsonProperty(PropertyName = "ProjectId")]
        public String ProjectId;

        [JsonProperty(PropertyName = "ControllerId")]
        public String ControllerId;

        [JsonProperty(PropertyName = "LoopId")]
        public String LoopId;

        [JsonProperty(PropertyName = "DevOneCodeStart")]
        public String DevOneCodeStart;

        [JsonProperty(PropertyName = "DevOneCodeEnd")]
        public String DevOneCodeEnd;

    }
}
