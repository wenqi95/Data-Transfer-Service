using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommonService.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CommonService.Controllers
{
    [Route("api/common")]
    public class CommonController : Controller
    {

        [HttpPost]
        [Route("SendMail")]
        public async Task<IActionResult> SendMail([FromBody]Models.MailSenderModel mailSenderModel)
        {
            var result = await MailSender.SendMailAsync(mailSenderModel.receiver, mailSenderModel.subject, mailSenderModel.content);
            if (result.Equals("success"))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost]
        [Route("SendSMS")]
        public async Task<IActionResult> SendSMS([FromBody]Models.SMSSenderModel mailSenderModel)
        {
            var result = await SMSSender.SendSMSAsync(mailSenderModel.receiver, mailSenderModel.content);
            if (result.Equals("success"))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        //// Test
        //[HttpGet]
        //[Route("Test")]
        //public async Task<IActionResult> Authentication1()
        //{
        //    var result = await SMSSender.SendSMSAsync("+8615626138309", "hello");
        //    return Ok();
        //    //var result = await MailSender.SendMailAsync("liuyingy@utrc.utc.com", "test", "test");
        //    //if (result.Equals("success"))
        //    //{
        //    //    return Ok(result);
        //    //}
        //    //else
        //    //    return BadRequest(result);
        //    //return Unauthorized();
        //    // Console.WriteLine(user.username, ",", user.password);
        //    //return AccessToken.createNewUser(new NewUserModel());
        //}
    }
}
