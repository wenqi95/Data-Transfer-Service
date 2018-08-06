using UserManagement.Models;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text;

namespace UserManagement.Services
{
    public class SqlDBManager
    {
        private SqlConnectionStringBuilder sqlcb = null;
        private String[] tableName;
        private String[] userTable;
        private string[] roleNameTable;
        private string[] roleNameNewTable;
        public SqlDBManager()
        {
            sqlcb = new SqlConnectionStringBuilder();
            sqlcb.DataSource = "gstdemo.database.windows.net";
            sqlcb.UserID = "utrcdev";
            sqlcb.Password = "Utrc1234";
            sqlcb.InitialCatalog = "ProjectCommission";

            tableName = new string[] { "ProjectTable", "MobileUploadedTable"};
            userTable = new string[] { "EngManager", "TechManager", "TechStaff" };
            roleNameTable = new string[] {"SysManager", "EngManager", "TechManager", "TechStaff" };
            roleNameNewTable = new string[] { "system_supervisor", "engineering_supervisor", "technical_supervisor", "service_engineer", "commission_engineer" };
        }

        //*************************
        //***For user management***
        //*************************
        public Task<string> AddUserRelationAsync(string userSup, string userSub, int tableNum)
        {
            String commandStr = String.Format("INSERT INTO {0} VALUES ('{1}', '{2}')", userTable[tableNum], userSub, userSup);
            return ExecuteSQLHelper(commandStr, tableNum, false);
        }

        public Task<string> DeleteUserAsync(string user, int tableNum)
        {
            String commandStr = String.Format("DELETE FROM {0} WHERE id = '{1}' ", userTable[tableNum], user);
            return ExecuteSQLHelper(commandStr, tableNum, false);
        }

        public Task<string> UpdateUserRelationAsync(string userSup, string userSub, int tableNum)
        {
            String commandStr = String.Format("UPDATE {0} SET {1} = '{2}' WHERE id = '{3}'",
                userTable[tableNum], roleNameTable[tableNum], userSup, userSub);
            return ExecuteSQLHelper(commandStr, tableNum, false);
        }

        public Task<string> ReadAllUsersAsync(int tableNum)
        {
            String commandStr = String.Format("SELECT * FROM {0}", userTable[tableNum]);
            return ExecuteSQLHelper(commandStr, tableNum, true);
        }

        public Task<string> ReadUsersBySupUsernamesAsync(string user, int tableNum)
        {
            String commandStr = String.Format("SELECT * FROM {0} WHERE {1} = '{2}'", userTable[tableNum], roleNameTable[tableNum], user);
            return ExecuteSQLHelper(commandStr, tableNum, true);
        }

        public Task<string> ReadUsersByUsernamsAsync(string user, int roleNum)
        {
            string commandStr = "";
            if (roleNum == 3)
            {
                commandStr = String.Format("SELECT TechStaff.id AS {3}, TechManager.id AS {4}, EngManager.id AS {5}, EngManager.SysManager AS {6}"
                + " FROM TechStaff FULL OUTER JOIN TechManager"
                + " ON TechStaff.TechManager = TechManager.id FULL OUTER JOIN EngManager"
                + " ON TechManager.EngManager = EngManager.id"
                + " WHERE {0}.{1} = '{2}'", userTable[roleNum - 1], "id", user, roleNameNewTable[3], roleNameNewTable[2], roleNameNewTable[1], roleNameNewTable[0]);

            }
            else
            {
                commandStr = String.Format("SELECT TechStaff.id AS {3}, TechManager.id AS {4}, EngManager.id AS {5}, EngManager.SysManager AS {6}"
                + " FROM TechStaff FULL OUTER JOIN TechManager"
                + " ON TechStaff.TechManager = TechManager.id FULL OUTER JOIN EngManager"
                + " ON TechManager.EngManager = EngManager.id"
                + " WHERE {0}.{1} = '{2}'", userTable[roleNum], roleNameTable[roleNum], user, roleNameNewTable[3], roleNameNewTable[2], roleNameNewTable[1], roleNameNewTable[0]);
            }
            return ExecuteSQLHelper(commandStr, 3, true);
        }

        public async Task<string> ExecuteSQLHelper(String commandStr, int tableNum, bool flagHasResult)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlcb.ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(commandStr, connection);
                    // read data from it
                    if (flagHasResult)
                    {
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        string result = "";
                        switch (tableNum)
                        {
                            //EngManager - SysManager
                            case 0:
                                List<EngineerSupervisorModel> EMList = new List<EngineerSupervisorModel>();
                                //Read each record
                                while (reader.Read())
                                {
                                    EngineerSupervisorModel emtmp = new EngineerSupervisorModel(reader["id"].ToString(), reader[roleNameTable[0]].ToString());
                                    EMList.Add(emtmp);
                                }
                                result = JsonConvert.SerializeObject(EMList, Formatting.None);
                                break;
                            //TechManager - EngManager
                            case 1:
                                List<TechManagerModel> TMList = new List<TechManagerModel>();
                                while (reader.Read())
                                {
                                    TechManagerModel tmtmp = new TechManagerModel(reader["id"].ToString(), reader[roleNameTable[1]].ToString());
                                    TMList.Add(tmtmp);
                                }
                                result = JsonConvert.SerializeObject(TMList, Formatting.Indented);
                                break;
                            // TechStaff - TechManager
                            case 2:
                                List<TechStaffModel> TSList = new List<TechStaffModel>();
                                while (reader.Read())
                                {
                                    TechStaffModel tstmp = new TechStaffModel(reader["id"].ToString(), reader[roleNameTable[2]].ToString());
                                    TSList.Add(tstmp);
                                }
                                result = JsonConvert.SerializeObject(TSList, Formatting.Indented);
                                break;
                            default:
                                List<UserListModel> ULLists = new List<UserListModel>();
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        UserListModel tstmp = new UserListModel(reader[roleNameNewTable[3]].ToString(),
                                            reader[roleNameNewTable[2]].ToString(),
                                            reader[roleNameNewTable[1]].ToString(),
                                            reader[roleNameNewTable[0]].ToString());
                                        ULLists.Add(tstmp);
                                    }
                                }
                                reader.Close();
                                connection.Close();
                                return JsonConvert.SerializeObject(ULLists, Formatting.Indented);
                        }
                        reader.Close();
                        connection.Close();
                        return result;
                    }
                    // write data: insert / update / delete
                    else
                    {
                        int i = command.ExecuteNonQuery();
                        return i.ToString();
                    }
                }
            }
            catch (SqlException e)
            {
                var errocode = e.Number;
                if (errocode == 18487 || errocode == 18488)//Password need reset
                {
                    return "Database Password need reset";
                }
            }
            return null;
        }
        //*************************
        //*************************
        //*************************
    }
}
