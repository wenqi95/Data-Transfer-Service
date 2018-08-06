using ProjectService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectService.Service
{
    public class ProjectUtils
    {
        private SqlDBManager sqlManager = null;
        static public ProjectUtils instance = new ProjectUtils();

        //map of office and date to swift number
        Dictionary<String, int> projIdMap = null;

        private ProjectUtils()
        {
            projIdMap = new Dictionary<string, int>();
        }

        public async Task<ProjectModel> Create(ProjectModel model)
        {
            String ID = generateProjectId(model.OfficeZip);
            ProjectModel project = new ProjectModel
            {
                ProjectId = ID,
                ProjectName = model.ProjectName,
                ProjectType = model.ProjectType,
                OfficeZip = model.OfficeZip,
                CreateTime = DateTime.UtcNow,
                CreatorID = model.CreatorID,
                ProjectStatus = 0,
                ProjectInfo = new ProjectInfoModel()
            };
            return await CreateProject(project);
        }

        public String generateProjectId(String officeZip)
        {
            DateTime now = DateTime.UtcNow;
            String date = now.ToString("yyyyMMdd").Substring(2, 4);
            String projIdStr = String.Concat(officeZip, date);
            if (!projIdMap.ContainsKey(projIdStr))
            {
                projIdMap.Add(projIdStr, 0);
            }
            projIdMap[projIdStr] += 1;
            String swiftNum = projIdMap[projIdStr].ToString().PadLeft(3, '0');
            String projectId = string.Concat(projIdStr, swiftNum);
            return projectId;
        }

        private async Task<ProjectModel> CreateProject(ProjectModel model)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }

            if (await sqlManager.CreateProjectAsync(model))
            {
                await sqlManager.CreateMobileUploadedRecord(model.ProjectId);
                await sqlManager.CreateBackupRecord(model.ProjectId);
                return model;
            }
            else
            {
                return new ProjectModel();
            }
        }

        public async Task<Boolean> UpdateBackupData(BackupModel modelInput)
        {
            BackupModel model = new BackupModel
            {
                ProjectId = modelInput.ProjectId,
                BackupDate = DateTime.UtcNow,
                CreatorId = modelInput.CreatorId,
                ProjectData = modelInput.ProjectData
            };

            await MoveBackupTable(model.ProjectId);
            return await UpdateBackupTable(model);
        }

        public async Task<Boolean> MoveBackupTable(String id)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.MoveBackupTable(id);
        }

        public async Task<Boolean> UpdateBackupTable(BackupModel model)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.UpdateBackupTable(model);
        }

        public async Task<ProjectInfoModel> RestoreData(String id, int order)
        {
            if (sqlManager == null)
            {
                sqlManager = new SqlDBManager();
            }
            return await sqlManager.RestoreDataFromTable(id, order);
        }
    }
}
