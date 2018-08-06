﻿using Newtonsoft.Json;
using System;

namespace Mobile.Models
{
    public class MobileUploadedModel
    {
        [JsonProperty(PropertyName = "ProjectId")]
        public String ProjectId;

        [JsonProperty(PropertyName = "ProjectInfo")]
        public ProjectInfoModel ProjectInfo;

        [JsonProperty(PropertyName = "UploadedTime")]
        public DateTime UploadedTime;
    }
}
