using Microsoft.AspNetCore.Authorization;
using Mobile.Models;
using Mobile.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Mobile.Controllers
{

    [Produces("application/json")]
    [Route("api/Mobile")]
    public class MobileController : Controller
    {
        [HttpGet]
        [Route("Status")]
        public string Status()
        {
            return "Ready";
        }

        [HttpPost]
        [Route("PushToReceiver/{id}")]
        public async Task<Boolean> PushToReceiverAsync(String id, [FromBody] ProjectInfoModel projectInfo)
        {
            var result = false;
            try
            {
                result = await WebSocketHandler.instance.PushAuthorizedInfoToReceiver(id, projectInfo);
            }
            catch (Exception e)
            {
                result = false;
            }
            finally
            {
            }
            return result;
                 
        }

        [HttpPost]
        [Route("pushHandShakeToReceiver/{id}")]
        public async Task<Boolean> pushHandShakeToReceiver(String id, [FromBody] CFGConnStateModel model)
        {
            return await WebSocketHandler.instance.pushHandShakeToReceiver(id, model);
        }

        [HttpPost]
        [Route("SendSetTimeToMobile/{key}")]
        public async Task<Boolean> SendSetTimeToMobile(String key, [FromBody] TokenModel model)
        {
            return await WebSocketHandler.instance.SendSetTimeToMobile(key, model);
        }

        [HttpPost]
        [Route("PushQueryInfoToMobile")]
        public async Task<Boolean> PushQueryInfoToMobile([FromBody] QueryModel model)
        {
            String projectid = model.ProjectId;
            String key = MobileUtils.instance.getKeyFromProjectId(projectid);
            return await WebSocketHandler.instance.PushQueryInfoToMobile(model, key);
        }


        [HttpPost]
        [Route("Close/{key}")]
        public async Task<Boolean> CloseWebSocketConnection(String key)
        {
            return await WebSocketHandler.instance.CloseWebSocketConnection(key);
        }
    }
}
