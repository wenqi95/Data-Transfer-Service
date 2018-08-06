using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class EngineerSupervisorModel
    {
        public EngineerSupervisorModel(string id, string sm)
        {
            this.id = id;
            this.SysManager = sm;
        }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("SysManager")]
        public string SysManager { get; set; }
    }
}
