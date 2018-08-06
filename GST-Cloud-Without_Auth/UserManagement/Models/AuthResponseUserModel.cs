using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class AuthResponseUserModel
    {
        //public AuthResponseUserModel() { }
        //public AuthResponseUserModel(string username, string gn, string sn, string email, string phone, string office){
        //    this.username = username;
        //    this.givenName = gn;
        //    this.surname = sn;
        //    this.email = email;
        //    this.mobilePhone = phone;
        //    this.officeLocation = office;
        //}

        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("roleName")]
        public string roleName { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("givenName")]
        public string givenName { get; set; }
        [JsonProperty("surname")]
        public string surname { get; set; }
        [JsonProperty("email")]
        public string email { get; set; }
        [JsonProperty("mobilePhone")]
        public string mobilePhone { get; set; }
        [JsonProperty("officeLocation")]
        public string officeLocation { get; set; }
        [JsonProperty("OAuthToken")]
        public OAuthResult OAuthToken { get; set; }
    }
}
