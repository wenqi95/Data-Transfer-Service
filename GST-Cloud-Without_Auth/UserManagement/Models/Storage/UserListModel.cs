using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class UserListModel
    {
        public UserListModel(string se, string ts, string es, string ss)
        {
            this.service_engineer = se;
            this.technical_supervisor = ts;
            this.engineering_supervisor = es;
            this.system_supervisor = ss;
        }
        [JsonProperty("service_engineer")]
        public string service_engineer { get; set; }
        [JsonProperty("technical_supervisor")]
        public string technical_supervisor { get; set; }
        [JsonProperty("engineering_supervisor")]
        public string engineering_supervisor { get; set; }
        [JsonProperty("system_supervisor")]
        public string system_supervisor { get; set; }

    }
}
