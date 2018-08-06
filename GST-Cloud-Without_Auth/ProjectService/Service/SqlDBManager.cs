using ProjectService.Models;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectService.Service
{
    public class SqlDBManager
    {
        private SqlConnectionStringBuilder sqlcb = null;
        private String[] tableName;
        private String[] userTable;
        private string[] roleNameTable;
        private String[] backupTable;
        public SqlDBManager()
        {
            sqlcb = new SqlConnectionStringBuilder();
            sqlcb.DataSource = "gstdemo.database.windows.net";
            sqlcb.UserID = "utrcdev";
            sqlcb.Password = "Utrc1234";
            sqlcb.InitialCatalog = "ProjectCommission";

            tableName = new string[] { "ProjectTable", "MobileUploadedTable" };
            userTable = new string[] { "EngManager", "TechManager", "TechStaff" };
            roleNameTable = new string[] { "system supervisor", "engineering supervisor", "technical supervisor", "service engineer" };
            backupTable = new String[] { "BackupTable1, BackupTable2, BackupTable3" };
        }

        public Task<Boolean> CreateTable(string name, List<String> head)
        {
            String commandStr = String.Format("CREATE TABLE {0} (projectid varchar(100) primary key, projectname varchar(100), projecttype varchar(100), officezip varchar(100), createtime datetime, creatorid varchar(100), projectstatus int, projectinfo nvarchar(max) constraint [Content should be formatted as JSON] check (ISJSON(projectinfo)>0) );", name);
            return CrudProjectFromSqlAsync(commandStr);
        }

        // CRUD operation for backend service

        public Task<Boolean> CreateProjectAsync(ProjectModel model)
        {
            var json = JsonConvert.SerializeObject(model.ProjectInfo);
            String commandStr = String.Format("INSERT INTO {0} (projectid, projectname, projecttype, officezip, createtime, creatorid, projectstatus, projectinfo) VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', {7}, '{8}');", tableName[0], model.ProjectId, model.ProjectName, model.ProjectType, model.OfficeZip, model.CreateTime, model.CreatorID, model.ProjectStatus, json);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> ApproveProjectAsync(String Id)
        {
            String commandStr = String.Format("UPDATE {0} SET projectstatus=1 WHERE projectid={1};", tableName[0], Id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> RejectProjectAsync(String Id)
        {
            String commandStr = String.Format("UPDATE {0} SET projectstatus=2 WHERE projectid={1};", tableName[0], Id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> CloseProjectAsync(String Id)
        {
            String commandStr = String.Format("UPDATE {0} SET projectstatus=3 WHERE projectid={1};", tableName[0], Id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> DeleteProjectAsync(String Id)
        {
            String commandStr = String.Format("DELETE FROM {0} WHERE projectid={1};", tableName[0], Id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> UpdateProjectInfoAsync(ProjectInfoModel model)
        {
            var JsonInfo = JsonConvert.SerializeObject(model);
            String commandStr = String.Format("UPDATE {0} SET projectinfo = '{1}' WHERE projectid='{2}';", tableName[0], JsonInfo, model.Project.Id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> UpdateProjectInfo2Async(ProjectInfoModel model)
        {
            
            var JsonInfo = JsonConvert.SerializeObject(model);
            String commandStr = String.Format("UPDATE {0} SET projectinfo = '{1}' WHERE projectid='{2}';", tableName[0], JsonInfo, model.Project.Id);
            return CrudProjectFromSqlAsync(commandStr);
        }



        public Task<Boolean> CreateMobileUploadedRecord(String id)
        {
            var model = new MobileUploadedModel();
            var JsonInfo = "";
            DateTime now = DateTime.UtcNow;
            String commandStr = String.Format("INSERT INTO MobileUploadedTable (ProjectId, UploadedTime, ProjectInfo) VALUES ('{0}', '{1}', '{2}');", id, JsonInfo, now);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> CreateBackupRecord(String id)
        {
            String commandStr = String.Format("INSERT INTO BackupTable1 (ProjectId) VALUES ('{0}'); INSERT INTO BackupTable2 (ProjectId) VALUES ('{1}'); INSERT INTO BackupTable3 (ProjectId) VALUES ('{2}');", id, id, id);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public Task<Boolean> UploadFromMobile(MobileUploadedModel model)
        {
            var json = JsonConvert.SerializeObject(model.ProjectInfo);
            String commandStr = String.Format("UPDATE MobileUploadedTable SET UploadedTime = '{0}', projectinfo = '{1}' where ProjectId = '{2}';", model.UploadedTime, json, model.ProjectId);
            return CrudProjectFromSqlAsync(commandStr);
        }

        public async Task<MobileUploadedModel> DownloadMobileData(String id)
        {
            String commandStr = String.Format("SELECT * from {0} where ProjectId='{1}';", this.tableName[1], id);
            MobileUploadedModel result = new MobileUploadedModel();
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result = queryMobileDataToModel(reader);
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return result;
                }
            }
            return result;
        }

        public async Task<Boolean> CrudProjectFromSqlAsync(String commandStr)
        {
            Boolean result = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    result = true;
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return result;
                }
            }
            return result;
        }

        // Read project from database for UI display

        public async Task<ProjectModel> GetProjectByIdAsync(String id)
        {
            String commandStr = String.Format("SELECT * from {0} where ProjectId='{1}';", this.tableName[0], id);
            return await ReadOneProjectFromSqlAsync(commandStr);
        }

        // Technical Staff 
        public async Task<IEnumerable<ProjectModel>> GetProjectByUserAsync(UserModel user)
        {
            if (user.Role == RoleName.service_engineer)
            {
                String commandStr = String.Format("SELECT * from {0} where CreatorID='{1}';", this.tableName[0], user.Id);
                return await ReadProjectFromSqlAsync(commandStr);
            }

            if (user.Role == RoleName.technical_supervisor)
            {
                String commandStr1 = String.Format("SELECT * from ProjectTable, TechStaff where (ProjectTable.CreatorID = TechStaff.id AND TechStaff.TechManager = '{0}');", user.Id);
                //return await ReadProjectFromSqlAsync(commandStr);
                List<ProjectModel> result1 = await ReadProjectFromSqlAsync(commandStr1);
                String commandStr2 = String.Format("SELECT * from {0} where CreatorID='{1}';", this.tableName[0], user.Id);
                List<ProjectModel> result2 = await ReadProjectFromSqlAsync(commandStr2);
                List<ProjectModel> result = result1.Union(result2).ToList();
                return result;
                

            }

            if (user.Role == RoleName.engineering_supervisor)
            {
                String commandStr = String.Format("SELECT * from ProjectTable, TechStaff, TechManager where (ProjectTable.CreatorID = TechStaff.id AND TechStaff.TechManager = TechManager.id And TechManager.EngManager = '{0}'); ", user.Id, user.Id);
                return await ReadProjectFromSqlAsync(commandStr);
            }

            if (user.Role == RoleName.system_supervisor)
            {
                String commandStr = String.Format("SELECT * from {0};", this.tableName[0]);
                return await ReadProjectFromSqlAsync(commandStr);
            }

            else
            {
                String commandStr = "";
                return await ReadProjectFromSqlAsync(commandStr);
            }
        }

        public async Task<IEnumerable<ProjectModel>> GetAllProjectAsync()
        {
            String commandStr = String.Format("SELECT * from {0};", this.tableName[0]);
            return await ReadProjectFromSqlAsync(commandStr);
        }

        // Templete code for reading project from database
        public async Task<List<ProjectModel>> ReadProjectFromSqlAsync(String commandStr)
        {
            List<ProjectModel> result = new List<ProjectModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var dataModel = queryProjectResultToModel(reader);
                        result.Add(dataModel);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return result;
                }
            }
            return result;
        }

        public async Task<ProjectModel> ReadOneProjectFromSqlAsync(String commandStr)
        {
            ProjectModel result = new ProjectModel();
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result = queryProjectResultToModel(reader);
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return result;
                }
            }
            return result;
        }

        public async Task<ProjectInfoModel> ReadOneProjectInfoFromSqlAsync(String commandStr)
        {
            ProjectInfoModel result = new ProjectInfoModel();
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result = queryProjectInfoToModel(reader);
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return result;
                }
            }
            return result;
        }

        private ProjectModel queryProjectResultToModel(SqlDataReader reader)
        {
            var jsonString = reader["projectinfo"].ToString();
            var dateString = reader["CreateTime"].ToString();
            ProjectInfoModel model = JsonConvert.DeserializeObject<ProjectInfoModel>(jsonString);
            DateTime dateTime = Convert.ToDateTime(dateString);

            var result = new ProjectModel()
            {
                ProjectId = reader["ProjectId"].ToString(),
                ProjectName = reader["ProjectName"].ToString(),
                ProjectType = reader["ProjectType"].ToString(),
                OfficeZip = reader["OfficeZip"].ToString(),
                CreateTime = dateTime,
                CreatorID = reader["CreatorID"].ToString(),
                ProjectStatus = Convert.ToInt32(reader["ProjectStatus"]),
                ProjectInfo = model
            };
            return result;
        }

        private MobileUploadedModel queryMobileDataToModel(SqlDataReader reader)
        {
            var jsonString = reader["ProjectInfo"].ToString();
            var dateString = reader["UploadedTime"].ToString();
            ProjectInfoModel model = JsonConvert.DeserializeObject<ProjectInfoModel>(jsonString);
            DateTime dateTime = Convert.ToDateTime(dateString);

            var result = new MobileUploadedModel()
            {
                ProjectId = reader["ProjectId"].ToString(),
                ProjectInfo = model,
                UploadedTime = dateTime
            };
            return result;
        }

        private ProjectInfoModel queryProjectInfoToModel(SqlDataReader reader)
        {
            var jsonProject = reader["Project"].ToString();
            var jsonController = reader["Controller"].ToString();
            var jsondevice = reader["Device"].ToString();

            ProjectCfgModel project = JsonConvert.DeserializeObject<ProjectCfgModel>(jsonProject);
            ControllerModel controller = JsonConvert.DeserializeObject<ControllerModel>(jsonController);
            List<DeviceCfgApiModel> devices = JsonConvert.DeserializeObject<List<DeviceCfgApiModel>>(jsondevice);

            var result = new ProjectInfoModel()
            {
                Project = project,
                Controller = controller,
                Device = devices
            };
            return result;
        }

        // This part is for data back up
        public async Task<Boolean> MoveBackupTable(String Id)
        {
            String commandStr = String.Format("Update t3 SET t3.BackupDate = t2.BackupDate, t3.CreatorId = t2.CreatorId, t3.ProjectData = t2.ProjectData FROM BackupTable2 AS t2 INNER JOIN BackupTable3 AS t3 ON  t3.projectid = t2.projectid; Update t2 SET t2.BackupDate = t1.BackupDate, t2.CreatorId = t1.CreatorId, t2.ProjectData = t1.ProjectData FROM BackupTable2 AS t2 INNER JOIN BackupTable1 AS t1 ON  t1.projectid = t2.projectid; ");
            return await CrudProjectFromSqlAsync(commandStr);
        }

        public async Task<Boolean> UpdateBackupTable(BackupModel model)
        {
            String json = JsonConvert.SerializeObject(model.ProjectData);
            String commandStr = String.Format("Update Backuptable1 SET BackupDate = '{0}', CreatorID = '{1}', ProjectData = '{2}' WHERE ProjectId = '{3}';", model.BackupDate, model.CreatorId, json, model.ProjectId);
            return await CrudProjectFromSqlAsync(commandStr);
        }

        public async Task<ProjectInfoModel> RestoreDataFromTable(String id, int order)
        {
            
            String commandStr = String.Format("Select ProjectData from '{0}' where projectid = '{1}'; ", backupTable[order - 1], id);
            return await ReadOneProjectInfoFromSqlAsync(commandStr);
        }

    }
}

    
        