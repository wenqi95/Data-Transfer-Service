using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserManagement.Models
{
    public class CreateUserByGraphRequestModel
    {
        public CreateUserByGraphRequestModel(CreateUserRequestModel user, string tenant)
        {
            this.accountEnabled = true;
            this.displayName = user.username;
            this.mailNickName = user.username;
            this.givenName = user.givenName;
            this.surname = user.surname;
            this.mobilePhone = user.mobilePhone;
            this.officeLocation = user.officeLocation;
            this.jobTitle = user.email;
            this.passwordProfile = new passwordProfileModel(user.password);
            this.userPrincipalName = user.username + "@" + tenant;
        }
        [JsonProperty("accountEnabled")]
        public bool accountEnabled { get; set; }
        [JsonProperty("displayName")]
        public string displayName { get; set; }
        [JsonProperty("mailNickName")]
        public string mailNickName { get; set; }
        [JsonProperty("givenName")]
        public string givenName { get; set; }
        [JsonProperty("surname")]
        public string surname { get; set; }
        [JsonProperty("mobilePhone")]
        public string mobilePhone { get; set; }
        [JsonProperty("jobTitle")]
        public string jobTitle { get; set; }
        [JsonProperty("officeLocation")]
        public string officeLocation { get; set; }
        [JsonProperty("passwordProfile")]
        public passwordProfileModel passwordProfile { get; set; }
        [JsonProperty("userPrincipalName")]
        public string userPrincipalName { get; set; }
    }
}
