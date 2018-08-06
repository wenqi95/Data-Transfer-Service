using CfgTool.Models;
using CfgTool.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CfgTool.Controllers
{
    [Produces("application/json")]
    [Route("api/CfgTool")]
    public class CfgToolController : Controller
    {
        [HttpGet]
        [Route("Status")]
        public String Status()
        {
            return "Ready";
        }

        [HttpGet]
        [Route("GetTest")]
        public ProjectInfoModel GetTest()
        {
            return new ProjectInfoModel();
        }

        // POST api/DeviceCfgUpload
        [HttpPost]
        [Route("UploadInfo")]
        public ProjectInfoModel UploadInfo([FromBody] ProjectInfoModel model)
        {
            return CfgToolUtils.instance.UploadInfo(model);
        }

        [HttpPost]
        [Route("pushHandShakeToCFG/{id}")]
        public async Task<Boolean> pushHandShakeToCFG(String id, [FromBody] MobileConnStateModel model)
        {
            return await WebSocketHandler.instance.pushHandShakeToCFG(id, model);
        }

        [HttpPost]
        [Route("pushDownloadProcessToCFG/{id}")]
        public async Task<Boolean> pushDownloadProcessToCFG(String id, [FromBody] DownloadProcessModel model)
        {
            return await WebSocketHandler.instance.pushDownloadProcessToCFG(id, model);
        }

        [HttpPost]
        [Route("pushToCFGTool/{id}")]
        public async Task<Boolean> pushToCFGTool(String id, [FromBody] ProjectInfoModel model)
        {
            return await WebSocketHandler.instance.pushToCFGTool(id, model);
        }



    }
}
