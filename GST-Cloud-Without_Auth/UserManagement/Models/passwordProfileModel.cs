using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class passwordProfileModel
    {
        public passwordProfileModel(string password)
        {
            this.password = password;
            this.forceChangePasswordNextSignIn = false;
        }
        [JsonProperty("password")]
        public string password { get; set; }
        [JsonProperty("forceChangePasswordNextSignIn")]
        public bool forceChangePasswordNextSignIn { get; set; }
    }
}
