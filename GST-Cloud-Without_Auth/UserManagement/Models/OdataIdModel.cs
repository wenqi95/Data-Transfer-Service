using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class OdataIdModel
    {
        public OdataIdModel (string OdataId)
        {
            this.odataId = OdataId;
        }
        [JsonProperty("@odata.id")]
        public string odataId { get; set; }
    }
}
