using CfgTool.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CfgTool.Service
{
    public class CfgToolUtils
    {
        static public CfgToolUtils instance = new CfgToolUtils();
        private String storageHost = "projectservice201808.azurewebsites.net";
        private int storagePort = 443;

        private String authorizeHost = "authorization201808.azurewebsites.net";
        private int authorizePort = 443;

        private String downloadHost = "mobile201808.azurewebsites.net";
        private int downloadPort = 443;

        private CfgToolUtils()
        {
        }

        public ProjectInfoModel UploadInfo(ProjectInfoModel model)
        {
            String modelStr = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(modelStr);
            String uriStr = storageUriWithPath("api/ProjectService/UpdateProjectInfo");
            HttpWebRequest request = WebRequest.CreateHttp(uriStr);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            String responseStr = getContentFromRequest(request);

            return JsonConvert.DeserializeObject<ProjectInfoModel>(responseStr);
        }

        public Boolean isValidToken(String token)
        {
            String uri = authorizeUriWithPath("api/Authorize/isValid/" + token);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<Boolean>(retString);
        }

        public Double getExpireTime(String key)
        {
            String uri = authorizeUriWithPath("api/Authorize/getExpireTime/" + key);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<Double>(retString);
        }

        public Boolean pushToReceiver(String id, ProjectInfoModel model)
        {
            String uri = downloadUriWithPath("api/Mobile/PushToReceiver/" + id);
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

        public Boolean pushHandShakeToReceiver(String id, CFGConnStateModel model)
        {
            String uri = downloadUriWithPath("api/Mobile/pushHandShakeToReceiver/" + id);
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

        public String authorizeUriWithPath(String path)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = this.authorizeHost;
            uriBuilder.Port = this.authorizePort;
            uriBuilder.Path = path;

            return uriBuilder.Uri.OriginalString;
        }

        private String storageUriWithPath(String path)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = this.storageHost;
            uriBuilder.Port = this.storagePort;
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
    }
}
