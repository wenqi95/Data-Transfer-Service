﻿using Newtonsoft.Json;
using System;

namespace CfgTool.Models
{
    public class ProjectCfgModel
    {
        [JsonProperty(PropertyName = "Id")]
        public String Id;

        [JsonProperty(PropertyName = "ProjectName")]
        public String ProjectName;

        [JsonProperty(PropertyName = "ProjectType")]
        public String ProjectType;
    }
}
