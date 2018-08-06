using Mobile.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GSTSecurity;

namespace Mobile.Service
{
    public class WebSocketHandler
    {
        static public WebSocketHandler instance = new WebSocketHandler();

        private Dictionary<String, WebSocket> webSocketMap = null;


        private WebSocketHandler() 
        {
            webSocketMap = new Dictionary<String, WebSocket>();
        }

        public async Task Handler(HttpContext context, Func<Task> next)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                String tokenStr = context.Request.Path.Value;
                if (tokenStr.Length != 0)
                {
                    tokenStr = tokenStr.Remove(0, 1);
                    if (tokenStr == "ws" || tokenStr == "test" || (tokenStr.Length != 0 && isValidToken(tokenStr)))
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        if (webSocketMap.ContainsKey(tokenStr))
                        {
                            webSocketMap[tokenStr].Abort();
                            webSocketMap.Remove(tokenStr);
                        }
                        webSocketMap.Add(tokenStr, webSocket);

                        await sendTokenExpireTime(webSocket, tokenStr);
                        await WebSocketMessageHandlerAsync(context, webSocket, tokenStr);
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        public async Task WebSocketMessageHandlerAsync(HttpContext httpContext, WebSocket webSocket, String key)
        {
            var buffer = new byte[1024 * 80];
            byte[] resultBuffer = new byte[0];
            byte[] oldBuffer = new byte[0];
            var size = 0;
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue && isValidToken(key))
            {
                String resultStr = "";
                try
                {
                    var newBufferSegment = new ArraySegment<byte>(buffer, 0, result.Count);
                    var newBuffer = newBufferSegment.ToArray();
                    size = size + result.Count;
                    resultBuffer = new byte[size];
                    Array.Copy(oldBuffer, 0, resultBuffer, 0, oldBuffer.Length);
                    Array.Copy(newBuffer, 0, resultBuffer, size - result.Count, result.Count);
                    oldBuffer = new byte[size];
                    oldBuffer = resultBuffer;

                    if (result.EndOfMessage)
                    {
                        resultStr = "OK";
                        var deviceStr = Encoding.UTF8.GetString(resultBuffer);

                        if (isHandShakeMessage(deviceStr))
                        {
                            var handShakeMsgModel = JsonConvert.DeserializeObject<MobileConnStateModel>(deviceStr);
                            String id = findTokenFromWebSocket(webSocket);
                            Boolean pushMsg = pushHandShakeToCFG(id, handShakeMsgModel);

                            resultBuffer = null;
                            oldBuffer = new byte[0];
                            size = 0;
                        }

                        if (isDownloadProcessMessage(deviceStr))
                        {
                            var downloadProcessModel = JsonConvert.DeserializeObject<DownloadProcessModel>(deviceStr);
                            String id = findTokenFromWebSocket(webSocket);
                            Boolean push = pushDownloadProcessToCFG(id, downloadProcessModel);

                            resultBuffer = null;
                            oldBuffer = new byte[0];
                            size = 0;
                        }

                        else
                        {
                            var devices = JsonConvert.DeserializeObject<ProjectInfoModel>(deviceStr);
                            var model = MobileUtils.instance.UploadFromMobile(devices.Project.Id, devices);
                            String id = findTokenFromWebSocket(webSocket);
                            Boolean push = pushToCFGTool(id, devices);

                            resultBuffer = null;
                            oldBuffer = new byte[0];
                            size = 0;
                        }
                    }

                }
                catch (Exception e)
                {
                    resultStr = "No connection or data format error";
                    resultBuffer = null;
                    oldBuffer = new byte[0];
                    size = 0;
                }
                finally
                {
                    var byteArray = Encoding.UTF8.GetBytes(resultStr);

                    await webSocket.SendAsync(new ArraySegment<byte>(byteArray), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }

            await sendCloseInfo(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private WebSocket findWebSocketInstance(String token)
        {
            if (webSocketMap == null)
            {
                return null;
            }

            return webSocketMap[token];
        }

        private Boolean isValidToken(String token)
        {
            return MobileUtils.instance.isValidToken(token);
        }

        private String findTokenFromWebSocket(WebSocket ws)
        {
            String token = "";
            foreach (var id in webSocketMap.Keys)
            {
                if (webSocketMap[id] == ws)
                {
                    token = id;
                    break;
                }
            }
            return token;
        }

        private Boolean pushHandShakeToCFG(String id, MobileConnStateModel model)
        {
            return MobileUtils.instance.pushHandShakeToCFG(id, model);
        }

        private Boolean pushDownloadProcessToCFG(String id, DownloadProcessModel model)
        {
            return MobileUtils.instance.pushDownloadProcessToCFG(id, model);
        }

        private Boolean pushToCFGTool(String id, ProjectInfoModel model)
        {
            return MobileUtils.instance.pushToCFGTool(id, model);
        }

        public async Task<Boolean> PushAuthorizedInfoToReceiver(String key, ProjectInfoModel info)
        {
            var deviceStr = JsonConvert.SerializeObject(info);
            String sig = instance.Signiture(deviceStr);
            String pKey = instance.PublicKey();

            SignedModel model = new SignedModel
            {
                ProjectInfo = info,
                Signiture = sig,
                PublicKey = pKey
            };

            String modelStr = JsonConvert.SerializeObject(model);
            var byteArray = Encoding.UTF8.GetBytes(modelStr);

            if (this.webSocketMap.ContainsKey(key) && isValidToken(key))
            {
                await this.webSocketMap[key].SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        // code added on 0805
        public async Task<Boolean> PushQueryInfoToMobile(QueryModel model, String key)
        {
            var queryModelStr = JsonConvert.SerializeObject(model);
            var byteArray = Encoding.UTF8.GetBytes(queryModelStr);

            if (this.webSocketMap.ContainsKey(key) && isValidToken(key))
            {
                await this.webSocketMap[key].SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Boolean> pushHandShakeToReceiver(String key, CFGConnStateModel model)
        {
            var ModelStr = JsonConvert.SerializeObject(model);
            var byteArray = Encoding.UTF8.GetBytes(ModelStr);

            if (this.webSocketMap.ContainsKey(key) && isValidToken(key))
            {
                await this.webSocketMap[key].SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Boolean> SendSetTimeToMobile(String key, TokenModel model)
        {
            var tokenModelStr = JsonConvert.SerializeObject(model);
            var byteArray = Encoding.UTF8.GetBytes(tokenModelStr);

            if (this.webSocketMap.ContainsKey(key) && isValidToken(key))
            {
                await this.webSocketMap[key].SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task sendTokenExpireTime(WebSocket webSocket, String key)
        {
            //Double sec = MobileUtils.instance.getExpireTime(key);
            //var secStr = sec.ToString();
            //int min = (int)sec / 60;
            //var infoStr = "Connection is successful! Token will expire in " + min + " minutes";
            //var byteArray = Encoding.UTF8.GetBytes(secStr);
            //await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);

            Double sec = MobileUtils.instance.getExpireTime(key);
            TokenModel newToken = new TokenModel
            {
                id = key,
                expireTime = sec
            };
            var newTokenStr = JsonConvert.SerializeObject(newToken);
            var byteArray = Encoding.UTF8.GetBytes(newTokenStr);
            await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task sendCloseInfo(WebSocket webSocket)
        {
            var infoStr = "Token expired, conection will be closed";
            var byteArray = Encoding.UTF8.GetBytes(infoStr);
            await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<Boolean> CloseWebSocketConnection(String key)
        {
            if (this.webSocketMap.ContainsKey(key))
            {
                var buffer = new byte[1024 * 80];
                WebSocketReceiveResult result = await webSocketMap[key].ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                await this.webSocketMap[key].CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        public String Signiture(String deviceStr)
        {
            string keyfileloc = @"example.pem";
            string passw = @"adriaan";
            KeyManager km = new KeyManager(keyfileloc, passw);
            ConfigCert ce = new ConfigCert(km);

            byte[] byte_array = Encoding.UTF8.GetBytes(deviceStr);
            Configuration sample = new Configuration(byte_array);
            String sig = ce.SignatureBase64(sample);
            return sig;
        }

        public String PublicKey()
        {
            string keyfileloc = @"example.pem";
            string passw = @"adriaan";
            KeyManager km = new KeyManager(keyfileloc, passw);
            ConfigCert ce = new ConfigCert(km);

            String key = ce.PublicKeyBase64();
            return key;
        }

        public Boolean isHandShakeMessage(String str)
        {
            var dataOject = Newtonsoft.Json.Linq.JObject.Parse(str);
            if (dataOject.Property("MobileConnState") != null)
                return true;
            else
                return false;
        }

        public Boolean isDownloadProcessMessage(String str)
        {
            var dataOject = Newtonsoft.Json.Linq.JObject.Parse(str);
            if (dataOject.Property("MobileDownloadProcess") != null)
                return true;
            else
                return false;
        }

    }
}
