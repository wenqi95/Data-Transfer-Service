using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Route("api/Storage")]
    [CustomHttps]
    public class StorageController : Controller
    {
        private SqlDBManager sqlManager = null;

        [HttpGet]
        [Route("Status")]
        public String Status()
        {
            Console.WriteLine("Line");
            return "Ready";
        }

        //*************************
        //***For user management***
        //*************************
        [HttpGet]
        [Route("GetUsersByTableNum/{tablenum}")]
        public async Task<string> GetUsersByTableNum(int tableNum)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.ReadAllUsersAsync(tableNum);
        }

        [HttpGet]
        [Route("GetUsersBySupUsername")]
        public async Task<string> GetUsersBySupUsername(String username, int tableNum)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.ReadUsersByUsernamsAsync(username, tableNum);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] StorageRequestModel model)
        {
             if (sqlManager == null){
                 sqlManager = new SqlDBManager();
             }
             var result = await sqlManager.AddUserRelationAsync(model.sup, model.username, model.tableNum);
             if (result.Equals("0"))
                return BadRequest("Nothing has happened and something has gone wrong!");
             else
                return Ok(result + " success!");
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] StorageRequestModel model)
        {
             if (sqlManager == null){
                 sqlManager = new SqlDBManager();
             }
             var result = await sqlManager.UpdateUserRelationAsync(model.sup, model.username, model.tableNum);
             if (result.Equals("0"))
                 return BadRequest("Nothing has happened and something has gone wrong!");
             else
                 return Ok(result + " success!");
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromBody] StorageRequestModel model)
        {
             if (sqlManager == null){
                 sqlManager = new SqlDBManager();
             }
             var result = await sqlManager.DeleteUserAsync(model.username, model.tableNum);
             if (result.Equals("0"))
                 return BadRequest("Nothing has happened and something has gone wrong!");
             else
                 return Ok(result + " success!");
        }
    }
}
