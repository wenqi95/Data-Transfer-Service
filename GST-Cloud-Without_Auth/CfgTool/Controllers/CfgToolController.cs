using CfgTool.Models;
using CfgTool.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
        
    }
}
