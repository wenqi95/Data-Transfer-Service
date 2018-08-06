using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class StorageRequestModel
    {
        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("sup")]
        public string sup { get; set; }
        [JsonProperty("tableNum")]
        public int tableNum { get; set; }
    }
}
