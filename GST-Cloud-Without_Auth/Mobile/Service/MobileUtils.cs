using Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Mobile.Service
{
    public class MobileUtils
    {
        static public MobileUtils instance = new MobileUtils();
        private String storageHost = "projectservice201808.azurewebsites.net";
        private int storagePort = 443;

        private String authorizeHost = "authorization201808.azurewebsites.net";
        private int authorizePort = 443;

        private MobileUtils()
        {
        }

        public Boolean isValidToken(String token)
        {
            String uri = authorizeUriWithPath("api/Authorize/isValid/" + token);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<Boolean>(retString);
        }

        public MobileUploadedModel UploadFromMobile(String id, ProjectInfoModel model)
        {
            DateTime now = DateTime.UtcNow;
            String ProjectId = id;
            MobileUploadedModel uploadedModel = new MobileUploadedModel
            {
                ProjectId = id,
                ProjectInfo = model,
                UploadedTime = now
            };

            String modelStr = JsonConvert.SerializeObject(uploadedModel);
            var data = Encoding.UTF8.GetBytes(modelStr);
            String uriStr = storageUriWithPath("api/ProjectService/UploadFromMobile");
            HttpWebRequest request = WebRequest.CreateHttp(uriStr);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            String responseStr = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<MobileUploadedModel>(responseStr);
        }

        public Double getExpireTime(String key)
        {
            String uri = authorizeUriWithPath("api/Authorize/getExpireTime/" + key);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<Double>(retString);
        }

        //Code added on 0805
        public String getKeyFromProjectId(String ProjectId)
        {
            String uri = authorizeUriWithPath("api/Authorize/GetKeyByProjectId/" + ProjectId);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.Method = "GET";
            string retString = getContentFromRequest(request);
            return JsonConvert.DeserializeObject<String>(retString);
        }

        public String storageUriWithPath(String path)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = this.storageHost;
            uriBuilder.Port = this.storagePort;
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

        public String getContentFromRequest(WebRequest request)
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
