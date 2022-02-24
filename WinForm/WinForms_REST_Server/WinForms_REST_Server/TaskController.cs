using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WinForms_REST_Server
{
    class TaskController : ApiController
    {
        public override string Key => "task";
        private List<TranscodeTask> TasksList;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public TaskController(List<TranscodeTask> tasksList) : base()
        {
            TasksList = tasksList;
        }

        private int taskCounter = 0;
        
        public override void ProcessRequest(HttpListenerResponse response, string method, string resource, string content)
        {
            (int index, string command, string[] args) = ParseResource(resource);
            Logger.Info($"Process Request {method}");
            switch (method)
            {
                case "GET" when command == "" && args.Length == 0: // api/task/1
                    GetStat(response, index);
                    break;
                // content is a body of a request, usually a JSON string
                // args is something that is added after the command
                case "POST" when command == "" && args.Length == 0: // api/title/title_text
                    CreateTask(response, content);
                    break;
                case "POST" when command == "start" && args.Length == 0:  // PUT -> POST 21.02.2022
                    StartTask(response, index);
                    break;
                case "POST" when command == "stop" && args.Length == 0:   // PUT -> POST 21.02.2022
                    StopTask(response, index);
                    break;
                default:
                    SendError(response, HttpStatusCode.BadRequest);
                    break;
            }
        }

        private void StopTask(HttpListenerResponse response, int index)
        {
            try
            {
                // here you can put a JSON string with a description of a modified state. This JSON should be parsed on the server and the required actions should be performed
                var task = TasksList.FirstOrDefault(x => x.ID == index);
                if (task == null)
                {
                    SendError(response, HttpStatusCode.BadRequest, "No task with this ID");
                }
                else
                {
                    task.StopTask();
                    SendResponse(response, JsonConvert.SerializeObject(task.OutputName));
                    Logger.Info($"StopTask OK with {task.OutputName}");
                }
            }
            catch (Exception ex)
            {
                SendError(response, HttpStatusCode.InternalServerError);
                Logger.Error($"StopTask Error" + ex.StackTrace);
            }
        }

        private void StartTask(HttpListenerResponse response, int index)
        {
            try
            {
                // here you can put a JSON string with a description of a modified state. This JSON should be parsed on the server and the required actions should be performed
                var task = TasksList.FirstOrDefault(x => x.ID == index);
                if (task == null)
                {
                    SendError(response, HttpStatusCode.BadRequest, "No task with this ID");
                    Logger.Warn($"No Task with {index} ID was found");
                }
                else
                {
                    task.RunTask();
                    SendResponse(response, JsonConvert.SerializeObject(task.OutputName)); 
                    Logger.Info($"StartTask OK with {task.OutputName}");
                }
            }
            catch (Exception ex)
            {
                SendError(response, HttpStatusCode.InternalServerError);
                Logger.Error($"StartTask Error" + ex.StackTrace);
            }
        }

        private void CreateTask(HttpListenerResponse response, string content)
        {
            try
            {
                content = "{" + content.Replace("url=", "'url':'").Replace("%3A", ":").Replace("%2F", "/") + "'}";
                // parse JSON to get URL
                JObject parsedContent = JObject.Parse(content);
                string _URL = (string)parsedContent["url"]; //System.Web.HttpUtility.UrlDecode(parsedContent.ToString())
                taskCounter++;
                TranscodeTask newTask = new TranscodeTask();
                newTask.ID = taskCounter;
                newTask.SourceName = _URL;
                if (newTask.InitTask() == true)
                { 
                    string info = newTask.GetInfo();
                    TasksList.Add(newTask);
                    JObject jObj = JObject.Parse(info);
                    jObj.Add("TaskID", newTask.ID.ToString());
                       
                    SendResponse(response, JsonConvert.SerializeObject(jObj, Formatting.Indented));
                    Logger.Info($"CreateTask OK with ID {newTask.ID}");
                }
                else
                {
                    SendError(response, HttpStatusCode.BadRequest, "Can't initialize the stream");
                }                
            }
            catch (Exception ex)
            {
                SendError(response, HttpStatusCode.InternalServerError);
                Logger.Error($"CreateTask Error" + ex.StackTrace);
            }
        }

        private void GetStat(HttpListenerResponse response, int index)
        {
            try
            {
                // here you can put a JSON string with a description of a modified state. This JSON should be parsed on the server and the required actions should be performed
                var task = TasksList.FirstOrDefault(x => x.ID == index);
                if (task == null)
                {
                    SendError(response, HttpStatusCode.BadRequest, "No task with this ID"); 
                    Logger.Warn($"No Task with {index} ID was found");
                }
                else
                {
                    var stat = task.GetStat();
                    SendResponse(response, JsonConvert.SerializeObject(stat, Formatting.Indented));
                    Logger.Info($"GetStat OK with {stat}");
                }
            }
            catch (Exception ex)
            {
                SendError(response, HttpStatusCode.InternalServerError);
                Logger.Error($"GetStat Error" + ex.StackTrace);
            }
        }
    }
}
