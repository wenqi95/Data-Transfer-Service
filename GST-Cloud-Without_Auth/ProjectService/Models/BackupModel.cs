using Newtonsoft.Json;
using System;
using ProjectService.Service;

namespace ProjectService.Models
{
    public class BackupModel
    { 
        [JsonProperty(PropertyName = "ProjectId")]
        public String ProjectId;

        [JsonProperty(PropertyName = "BackupDate")]
        public DateTime BackupDate;

        [JsonProperty(PropertyName = "CreatorId")]
        public String CreatorId;

        [JsonProperty(PropertyName = "ProjectData")]
        public ProjectInfoModel ProjectData;
    }
}
