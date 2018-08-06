using System;
using System.Collections.Generic;

namespace Authorization.Models
{
    public class TokenModel
    {
        public String id;
        public DateTime generateTime;
        public Double expireTime;
        public String projectId;
    }
}
