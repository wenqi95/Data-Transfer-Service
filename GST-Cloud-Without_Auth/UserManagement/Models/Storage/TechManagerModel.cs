using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace UserManagement.Models
{
    public class TechManagerModel
    {
        public TechManagerModel(string id, string em)
        {
            this.id = id;
            this.EngManager = em;
        }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("EngManager")]
        public string EngManager { get; set; }
    }
}
