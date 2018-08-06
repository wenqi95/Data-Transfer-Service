using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CfgTool.Models;
using System;using Newtonsoft.Json.Linq;

using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace CfgTool.Service
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
            var buffer = new byte[1024 * 4];
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
                            var handShakeMsgModel = JsonConvert.DeserializeObject<CFGConnStateModel>(deviceStr);
                            String id = findTokenFromWebSocket(webSocket);
                            Boolean pushMsg = pushHandShakeToReceiver(id, handShakeMsgModel);

                            resultBuffer = null;
                            oldBuffer = new byte[0];
                            size = 0;
                        }
                        else
                        {
                            var devices = JsonConvert.DeserializeObject<ProjectInfoModel>(deviceStr);
                            var model = CfgToolUtils.instance.UploadInfo(devices);
                            String id = findTokenFromWebSocket(webSocket);
                            Boolean push = pushToReceiver(id, devices);

                            resultBuffer = null;
                            oldBuffer = new byte[0];
                            size = 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    resultStr = "No connection or data format error, try again later";
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

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private Boolean isValidToken(String token)
        {
            return CfgToolUtils.instance.isValidToken(token);
        }

        private WebSocket findWebSocketInstance(String token)
        {
            if (webSocketMap == null)
            {
                return null;
            }

            return webSocketMap[token];
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

        private Boolean pushToReceiver(String id, ProjectInfoModel model)
        {
            return CfgToolUtils.instance.pushToReceiver(id, model);
        }

        private Boolean pushHandShakeToReceiver(String id, CFGConnStateModel model)
        {
            return CfgToolUtils.instance.pushHandShakeToReceiver(id, model);
        }

        public async Task sendTokenExpireTime(WebSocket webSocket, String key)
        {
            Double sec = CfgToolUtils.instance.getExpireTime(key);
            var secStr = JsonConvert.SerializeObject(sec);
            var infoStr = "Connection is successful! Token will expire in " + secStr + "s";
            var byteArray = Encoding.UTF8.GetBytes(infoStr);
            await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task sendCloseInfo(WebSocket webSocket)
        {
            var infoStr = "Token expired, the conection will be closed";
            var byteArray = Encoding.UTF8.GetBytes(infoStr);
            await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public Boolean isHandShakeMessage(String str)
        {
            var dataOject = Newtonsoft.Json.Linq.JObject.Parse(str);
            if (dataOject.Property("CFGConnState") != null)
                return true;
            else
                return false;
        }

        public async Task<Boolean> pushHandShakeToCFG(String key, MobileConnStateModel model)
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

        public async Task<Boolean> pushDownloadProcessToCFG(String key, DownloadProcessModel model)
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

        public async Task<Boolean> pushToCFGTool(String key, ProjectInfoModel model)
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

    }
}
