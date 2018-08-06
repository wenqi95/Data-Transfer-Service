using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class ControllerModel
    {
        [JsonProperty(PropertyName = "ControllerID")]
        public String ControllerID;

        [JsonProperty(PropertyName = "ControllerName")]
        public String ControllerName;

        [JsonProperty(PropertyName = "ControllerType")]
        public String ControllerType;

        [JsonProperty(PropertyName = "Description")]
        public String Description;

        [JsonProperty(PropertyName = "Count")]
        public int Count;

    }
}
