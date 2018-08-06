using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class SignedModel
    {
        [JsonProperty(PropertyName = "ProjectInfo")]
        public ProjectInfoModel ProjectInfo;

        [JsonProperty(PropertyName = "Signiture")]
        public String Signiture;

        [JsonProperty(PropertyName = "PublicKey")]
        public String PublicKey;

    }
}
