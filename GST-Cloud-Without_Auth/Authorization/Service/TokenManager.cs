using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft;
using Authorization.Models;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace Authorization.Service
{
    public class TokenManager
    {
        static public TokenManager instance = new TokenManager();

 
        Dictionary<String, TokenModel> tokenMap = null; // Map token string to TokenModel
        Dictionary<String, String> projectMap = null;  // Map ProjectId to token string

        private String emailHost = "gstaadgettoken.azurewebsites.net";
        private int emailPort = 443;

        private String downloadHost = "mobile201808.azurewebsites.net";
        private int downloadPort = 443;

        private TokenManager()
        {
            tokenMap = new Dictionary<String, TokenModel>();
            projectMap = new Dictionary<String, String>();
        }

        public String generateNewToken(String projId)
        {
            clearExpireToken();
            clearOldToken(projId);
            String guid = Guid.NewGuid().ToString();
            TokenModel token = new TokenModel()
            {
                id = guid,
                generateTime = DateTime.UtcNow,
                expireTime = 1800,
                projectId = projId
            };

            tokenMap[token.id] = token;
            projectMap[projId] = token.id;

            return token.id;
        }

        public double setExpireTime(String projectId, double sec)
        {
            String key = projectMap[projectId];
            TokenModel tokon = tokenMap[key];
            tokon.expireTime = sec;
            //sendSetTimeToMobile(key, tokon);
            return tokon.expireTime;
        }

        public double getExpireTime(String key)
        {
            //return tokenMap[key].expireTime;
            if (isTokenValid(key))
            {
                var interval = DateTime.UtcNow.Subtract(tokenMap[key].generateTime).TotalSeconds;
                var expireTime = tokenMap[key].expireTime - interval;
                if (expireTime < 0) return 0;
                return expireTime;
            }
            return 0;

        }

        // Clear expire token in token map
        public void clearExpireToken()
        {
            var allKeys = new List<String>(tokenMap.Keys);
            foreach (var oneKey in allKeys)
            {
                if (!isTokenValid(oneKey))
                {
                    projectMap.Remove(tokenMap[oneKey].projectId);
                    tokenMap.Remove(oneKey);
                }
            }
        }

        public String queryKey(String projectid)
        {
            if (projectMap.ContainsKey(projectid))
            {
                String key = projectMap[projectid];
                if (isTokenValid(key))
                {
                    return key;
                }
                else
                {
                    return "The token is invalid";
                }
            }
            else
            {
                return "The token is invalid";
            }
        }

        public void clearOldToken(String projId)
        {
            if (projectMap.ContainsKey(projId))
            {
                String oldTokenStr = projectMap[projId];
                tokenMap.Remove(oldTokenStr);
            }
        }

        public Boolean tokenExist(String key)
        {
            return tokenMap.ContainsKey(key);
        }

        public Boolean isTokenValid(String key)
        {
            if (!tokenExist(key))
            {
                return false;
            }

            var interval = DateTime.UtcNow.Subtract(tokenMap[key].generateTime).TotalSeconds;
            if (interval > tokenMap[key].expireTime)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // make tokon invalid
        public String invalidateToken(String projectId)
        {
            String key = projectMap[projectId];
            tokenMap.Remove(key);
            projectMap.Remove(projectId);
            String result = "The connection has been closed";
            return result;
        }

        public String getAuthorizedProject(String key)
        {
            return tokenMap[key].projectId;
        }

        public Boolean sendSetTimeToMobile(String key, TokenModel model)
        {
            String uri = downloadUriWithPath("api/Mobile/SendSetTimeToMobile/" + key);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.ContentType = "application/json";
            String modelStr = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(modelStr);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<Boolean>(retString);
        }

        public String downloadUriWithPath(String path)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = this.downloadHost;
            uriBuilder.Port = this.downloadPort;
            uriBuilder.Path = path;

            return uriBuilder.Uri.OriginalString;
        }

        private String getContentFromRequest(WebRequest request)
        {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string retString = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();

            return retString;
        }

        public String emailUriWithPath(String path)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = this.emailHost;
            uriBuilder.Port = this.emailPort;
            uriBuilder.Path = path;

            return uriBuilder.Uri.OriginalString;
        }

        public String sendEmail(EmailModel model)
        {
            String uri = emailUriWithPath("api/accesstoken/SendMail");
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.ContentType = "application/json";
            String modelStr = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(modelStr);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            string retString = getContentFromRequest(request);
            return retString;
        }


    }
}
