using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class CreateUserRequestModel
    {
        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("password")]
        public string password { get; set; }
        [JsonProperty("givenName")]
        public string givenName { get; set; }
        [JsonProperty("surname")]
        public string surname { get; set; }
        [JsonProperty("mobilePhone")]
        public string mobilePhone { get; set; }
        [JsonProperty("email")]
        public string email { get; set; }
        [JsonProperty("officeLocation")]
        public string officeLocation { get; set; }
    }
}
