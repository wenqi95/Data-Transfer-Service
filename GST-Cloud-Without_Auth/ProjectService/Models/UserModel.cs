using Newtonsoft.Json;
using System;

namespace ProjectService.Models
{
    public class UserModel
    {
        [JsonProperty(PropertyName = "Id")]
        public String Id;
        
        [JsonProperty(PropertyName = "Role")]
        public RoleName Role;
    }

    public enum RoleName
    {
        commission_engineer,
        service_engineer,
        technical_supervisor,
        engineering_supervisor,
        system_supervisor
    }
}