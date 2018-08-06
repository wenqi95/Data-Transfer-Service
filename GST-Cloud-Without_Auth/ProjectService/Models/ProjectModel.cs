using Newtonsoft.Json;
using System;
using ProjectService.Service;

namespace ProjectService.Models
{
    public class ProjectModel
    { 
        [JsonProperty(PropertyName = "ProjectId")]
        public String ProjectId;

        [JsonProperty(PropertyName = "ProjectName")]
        public String ProjectName;

        [JsonProperty(PropertyName = "ProjectType")]
        public String ProjectType;

        [JsonProperty(PropertyName = "OfficeZip")]
        public String OfficeZip;

        [JsonProperty(PropertyName = "CreateTime")]
        public DateTime CreateTime;

        [JsonProperty(PropertyName = "CreatorID")]
        public String CreatorID;

        [JsonProperty(PropertyName = "ProjectStatus")]
        public int ProjectStatus;

        [JsonProperty(PropertyName = "ProjectInfo")]
        public ProjectInfoModel ProjectInfo;
    }
}
