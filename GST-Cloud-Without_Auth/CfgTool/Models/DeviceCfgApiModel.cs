using Newtonsoft.Json;
using System;

namespace CfgTool.Models
{
    public class DeviceCfgApiModel
    {
        [JsonProperty(PropertyName = "LoopId")]
        public String LoopId;

        [JsonProperty(PropertyName = "ControllerId")]
        public String controllerId;

        [JsonProperty(PropertyName = "DevOneCode")]
        public String devOneCode;

        [JsonProperty(PropertyName = "DeviceNumber")]
        public String deviceNumber;

        [JsonProperty(PropertyName = "DeviceType")]
        public String deviceType;

        [JsonProperty(PropertyName = "DeviceTypeName")]
        public String deviceTypeName;

        [JsonProperty(PropertyName = "DevProperty")]
        public String devProperty;

        [JsonProperty(PropertyName = "Description")]
        public String description;

        [JsonProperty(PropertyName = "RegisteredFlag")]
        public String registeredFlag;
    }
}
