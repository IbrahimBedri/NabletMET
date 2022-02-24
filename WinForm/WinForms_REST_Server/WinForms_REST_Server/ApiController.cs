using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WinForms_REST_Server
{
    public abstract class ApiController
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public abstract string Key { get; }

        public ApiController()
        {
        }

        public abstract void ProcessRequest(HttpListenerResponse response, string method, string resource, string content);

        protected (int index, string command, string[] args) ParseResource(string resource)
        {
            var match = Regex.Match(resource, @"(?<index>\d+)?/?(?<command>[\w-]+)?/?(?<args>[\w-%]+)?");
            int index = match.Groups.TryGetValue("index", out Group group) && int.TryParse(group.Value, out int value) ? value : -1;
            string command = match.Groups.ContainsKey("command") ? match.Groups["command"].Value : "";
            string[] args = match.Groups.ContainsKey("args") && !string.IsNullOrEmpty(match.Groups["args"].Value)
                          ? match.Groups["args"].Value.Split('/')
                          : Array.Empty<string>();
                          
            return (index, command, args.Select(a => WebUtility.UrlDecode(a)).ToArray());
        }

        protected void SendError(HttpListenerResponse response, HttpStatusCode code, string message = "")
        {
            SendResponse(response, message, code);
        }

        protected void SendResponse(HttpListenerResponse response, string message = "", HttpStatusCode code = HttpStatusCode.OK)
        {
            try
            {
                response.StatusCode = (int)code;
                if (!string.IsNullOrEmpty(message))
                {
                    response.AppendHeader("Access-Control-Allow-Origin", "*"); // 21.02.2022
                    response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    response.AppendHeader("Access-Control-Allow-Credentials", "true");
                    response.ContentType = "application/json";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message ?? "");
                    response.ContentLength64 = buffer.Length;
                    using Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                response.Close();
            }
            catch (Exception ex)
            {
                // log error
                Logger.Error("SendResponse Error: " + ex.StackTrace);
            }
        }
    }
}
