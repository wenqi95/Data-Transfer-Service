using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mobile.Models
{
    public class ProjectInfoModel
    {
        [JsonProperty(PropertyName = "Project")]
        public ProjectCfgModel Project;

        [JsonProperty(PropertyName = "Controller")]
        public ControllerModel Controller;

        [JsonProperty(PropertyName = "Device")]
        public List<DeviceCfgApiModel> Device;
    }
}
