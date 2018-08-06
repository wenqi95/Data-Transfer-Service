using ProjectService.Models;
using ProjectService.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ProjectService.Controllers
{
    [Produces("application/json")]
    [Route("api/ProjectService")]
    public class ProjectsController : Controller
    {
        private SqlDBManager sqlManager = null;
   
        [HttpGet]
        [Route("Status")]
        public string Status()
        {
            return "Ready";
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ProjectModel> Create([FromBody] ProjectModel model)
        {
            return await ProjectUtils.instance.Create(model);
        }

        [HttpPost("Approve/{id}")]
        public async Task<Boolean> Approve(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            return await sqlManager.ApproveProjectAsync(id);
        }

        [HttpPost("Reject/{id}")]
        public async Task<Boolean> Reject(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            return await sqlManager.RejectProjectAsync(id);
        }

        [HttpPost("Close/{id}")]
        public async Task<Boolean> Close(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            return await sqlManager.CloseProjectAsync(id);
        }

        // Upload Configation information to database
        [HttpPost]
        [Route("UpdateProjectInfo")]
        public async Task<ProjectInfoModel> UpdateProjectInfo([FromBody] ProjectInfoModel model)
        {

            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            if (await sqlManager.UpdateProjectInfoAsync(model))
            {
                return model;
            }
            else
            {
                return new ProjectInfoModel();
            }

        }

        [HttpGet("GetProjectStatus/{id}")]
        public async Task<int> GetProjectStatus(string id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            ProjectModel result = await sqlManager.GetProjectByIdAsync(id);
            return result.ProjectStatus;
        }

        [HttpGet("GetProjectById/{id}")]
        public async Task<ProjectModel> GetProjectById(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager(); 
            }
            return await sqlManager.GetProjectByIdAsync(id);
        }

        [HttpGet("GetAllProject")]
        public async Task<IEnumerable<ProjectModel>> GetAllProject()
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.GetAllProjectAsync();
        }

        [HttpPost("GetProjectByUser")]
        public async Task<IEnumerable<ProjectModel>> GetProjectByUser([FromBody] UserModel user)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.GetProjectByUserAsync(user);
        }

        // Get project id List for CFG tool  (code added on 0805)
        [HttpPost("GetProjectIdList")] 
        public async Task<IEnumerable<String>> GetProjectIdByUser([FromBody] UserModel user)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            IEnumerable<ProjectModel> ProjectList = await sqlManager.GetProjectByUserAsync(user);
            List<String> ProjectIdList = new List<String>();
            foreach (var project in ProjectList)
            {
                ProjectIdList.Add(project.ProjectId);
            }
            return ProjectIdList;
        }

        [HttpPost]
        [Route("UploadFromMobile")]
        public async Task<MobileUploadedModel> UploadFromMobile([FromBody] MobileUploadedModel model)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            if (await sqlManager.UploadFromMobile(model))
            {
                return model;
            }
            else
            {
                return new MobileUploadedModel();
            }
        }

        [HttpGet("DownloadMobileData/{id}")]
        public async Task<MobileUploadedModel> DownloadMobileData(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.DownloadMobileData(id);
        }

        [HttpPost("Backup")]
        public async Task<Boolean> BackupData([FromBody] BackupModel model)
        {
            return await ProjectUtils.instance.UpdateBackupData(model);
        }

        [HttpPost("Restore")]
        // order can be 1, 2, 3 
        public async Task<ProjectInfoModel> RestoreData(String id, int order)
        {
            return await ProjectUtils.instance.RestoreData(id, order);
        }



    }
}
