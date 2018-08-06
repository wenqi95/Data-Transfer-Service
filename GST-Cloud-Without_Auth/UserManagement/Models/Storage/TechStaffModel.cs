using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace UserManagement.Models
{
    public class TechStaffModel
    {
        public TechStaffModel(string id, string tm)
        {
            this.id = id;
            this.TechManager = tm;
            this.EngManager = null;
            this.SysManager = null;
        }
        public TechStaffModel(string id, string tm, string em, string sm)
        {
            this.id = id;
            this.TechManager = tm;
            this.EngManager = em;
            this.SysManager = sm;
        }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("TechManager")]
        public string TechManager { get; set; }
        public string EngManager { get; set; }
        public string SysManager { get; set; }
    }
}
