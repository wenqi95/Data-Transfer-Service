using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserManagement.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
    {

        [HttpGet]
        [Route("Status")]
        public async Task<string> Status()
        {
            AADUserManager.AutoRenewAccessToken();
            return "Ready";
        }

        // GET: api/accesstoken
        [HttpGet()]
        [Route("graph")]
        public string GetGraphAccessToken()
        {
            return AADUserManager.getAccessToken();
        }

        // GET: api/accesstoken
        [HttpGet()]
        [Route("readComEngUsers")]
        public async Task<IActionResult> ReadComEngUsers()
        {
            var result = await AADUserManager.ReadAllComEngAsync();
            if (!result.StartsWith("error"))
            {
                return Ok(result);
            }
            else
                return BadRequest(result.Remove(0, 5));
        }

        // GET: test a user if it is existed
        [HttpGet()]
        [Route("testUser")]
        public async Task<IActionResult> TestUser(string username)
        {
            var result = await AADUserManager.hasThisUserRequestAsync(username);
            if (result.Equals("true"))
            {
                return BadRequest("This username exists.");
            }
            else if (result.Equals("false"))
            {
                return Ok();
            }
            else
                return BadRequest(result);
        }

        // POST: add a user in add
        [HttpPost]
        [Route("addUser")]
        public async Task<IActionResult> addNewUser([FromBody]Models.CreateUserRequestModel user)
        {
            var result = await AADUserManager.createNewUserRequestAsync(user);
            if (result.Equals("Created"))
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        // POST: update a user in add
        [HttpPost]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody]Models.AuthResponseUserModel user)
        {
            var result = await AADUserManager.UpdateUserRequestAsync(user);
            if (result.Equals("success"))
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        // GET api/accesstoken
        [HttpGet]
        [Route("authentication")]
        public async Task<IActionResult> Authentication(string username, string password)
        {
            var result = await AADUserManager.tokenByUserPassword(username, password);
            if (result != null)
                return Ok(JsonConvert.SerializeObject(result));
            else
                return Unauthorized();
        }

        // POST api/accesstoken
        [HttpPost]
        [Route("authentication")]
        public async Task<IActionResult> Authentication([FromBody]Models.AuthRequestUserModel user)
        {
            var result = await AADUserManager.tokenByUserPassword(user.username, user.password);
            if (result != null)
                return Ok(JsonConvert.SerializeObject(result));
            else
                return Unauthorized();
        }

        //// POST api/accesstoken
        //[HttpGet]
        //[Route("authTest")]
        //public async Task<IActionResult> Authentication1()
        //{
        //   var result = await AccessToken.UpdateUserRequestAsync(new AuthResponseUserModel("alex333","alex","Wu","abc@xyz.com","13725836914","104"));
        //   if (result.Equals("success"))
        //    {
        //        return Ok(result);
        //    }
        //    else
        //        return BadRequest(result);
        //   //return Unauthorized();
        //   // Console.WriteLine(user.username, ",", user.password);
        //   //return AccessToken.createNewUser(new NewUserModel());
        //}
    }
}
