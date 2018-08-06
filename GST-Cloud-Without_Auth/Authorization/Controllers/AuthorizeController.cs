using Authorization.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Authorization.Models;
using System.Collections.Generic;

namespace Authorization.Controllers
{
    //[Authorize]
    [Produces("application/json")]
    [Route("api/Authorize")]
    public class AuthorizeController : Controller
    {
        [HttpGet]
        [Route("Status")]
        public String Status()
        {
            return "Ready";
        }

        // set tokon expire time for project
        [HttpGet]
        [Route("setTime")]
        public Double setExpireTime(String projectId, Double sec)
        {
            return TokenManager.instance.setExpireTime(projectId, sec);
        }

        [HttpGet]
        [Route("getExpireTime/{key}")]
        public Double getExpireTime(String key)
        {
            return TokenManager.instance.getExpireTime(key);
        }

        [HttpGet]
        [Route("GetKeyByProjectId/{projectid}")]
        public String queryKey(String projectid)
        {
            return TokenManager.instance.queryKey(projectid);
        }

        [HttpGet]
        [Route("generate/{projectId}")]
        public String generateAuthorizeToken(String projectId)
        {
            String tokenstr = TokenManager.instance.generateNewToken(projectId);
            return tokenstr;
        }

        // make tokon invalid
        [HttpGet]
        [Route("invalidate/{projectId}")]
        public String invalidateAuthorizeToken(String projectId)
        {
            return TokenManager.instance.invalidateToken(projectId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("isValid/{key}")]
        public Boolean isValid(String key)
        {
            return TokenManager.instance.isTokenValid(key);
        }
    }
}