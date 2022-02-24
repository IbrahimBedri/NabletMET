using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace WinForms_REST_Server
{
    class RestListener
    {
        private readonly List<TranscodeTask> TaskList;
        private readonly List<ApiController> _controllers;
        private CancellationTokenSource _cts;
        private HttpListener _listener;
        private string _prefix;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string Prefix
        {
            get => _prefix;
            set
            {
                Regex r = new Regex(@"^http://[^/]+?(?<port>:\d+)+(|\/?)$",
                                        RegexOptions.None, TimeSpan.FromMilliseconds(150));
                Match m = r.Match(value);
                if (m.Success)
                {
                    SetPrefix(value);
                }
            }
        }

        public RestListener(List<TranscodeTask> tasksList, string prefix)
        {            
            _prefix = prefix;
            TaskList = tasksList;
            // define prefix

            _controllers = new List<ApiController>(){
                new TaskController(TaskList)
            };
        }

        public bool Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_prefix);
                _listener.Start();
            }
            catch (HttpListenerException e)
            {
                try
                {

                    if (e.ErrorCode == 5)
                    {
                        Process.Start(new ProcessStartInfo("netsh.exe", $"http add urlacl url={_prefix} sddl=D:(A;;GX;;;S-1-1-0)") { Verb = "runas", UseShellExecute = true });
                        _listener = new HttpListener();
                        _listener.Prefixes.Add(_prefix);
                        _listener.Start();
                    }
                    else
                    {
                        // can't start -- the parameter is incorrect
                        return false;
                    }
                }
                catch
                {
                    // can't register url -- return
                    return false;
                }
            }
            catch (Exception ex)
            {
                // can't register url -- return
                return false;
            }
            Task.Run(() =>
            {
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        NonblockingListener();
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1);
                    }
                }
            });
            return true;
        }

        public void NonblockingListener()
        {
            IAsyncResult result = _listener.BeginGetContext(ListenerCallback, _listener);
            result.AsyncWaitHandle.WaitOne();
        }

        public void ListenerCallback(IAsyncResult result)
        {

            if (_cts == null || _cts.IsCancellationRequested) return;
            _listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = _listener.EndGetContext(result);
            ProcessRequest(context);
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            string method = context.Request.HttpMethod;
            // check for Authorization in the request
            //if (!context.Request.Headers.AllKeys.Contains("Authorization"))
            //{
            //    SendError(response, HttpStatusCode.Unauthorized);
            //    return;
            //}
            //foreach (var key in context.Request.Headers.AllKeys)
            //{
            //    if (key.ToString() == "Authorization")
            //    {
            //        string apiToken = context.Request.Headers.GetValues(key)[0];
            //        // check API tocken for authorization access
            //        if (!string.IsNullOrEmpty(apiToken))
            //        {
            //            SendError(response, HttpStatusCode.Unauthorized);
            //            return;
            //        }
            //    }
            //}
            string url = context.Request.RawUrl?.Trim('/') ?? "";
            string content = null;
            if (context.Request.ContentLength64 > 0)
            {
                using var reader = new StreamReader(context.Request.InputStream);
                content = reader.ReadToEnd();
            }

            var match = Regex.Match(url, @"api/(?<controller>\w+)/?(?<resource>.*)?");
            if (match.Success && _controllers.SingleOrDefault(c => c.Key == match.Groups["controller"].Value?.ToLower()) is ApiController controller)
            {
                controller.ProcessRequest(response, method, match.Groups["resource"].Value ?? "", content);
            }
            else
            {
                SendError(response, HttpStatusCode.BadRequest);
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            ListenerCallback(null);
            _listener.Close();
            _cts?.Dispose();
            _cts = null;
        }

        private void SetPrefix(string prefix)
        {
            if (_cts == null)
            {
                _prefix = prefix;
            }
            else
            {
                Stop();
                try
                {
                    Process.Start(new ProcessStartInfo("netsh.exe", $"http add urlacl url={prefix} sddl=D:(A;;GX;;;S-1-1-0)") { Verb = "runas", UseShellExecute = true });
                    Process.Start(new ProcessStartInfo("netsh.exe", $"http delete urlacl url={_prefix}") { Verb = "runas", UseShellExecute = true });
                    _prefix = prefix;
                }
                catch { }
                if (!Start())
                {
                    MessageBox.Show("Error on setting REST Prefix");
                }
            }
        }

        private void SendError(HttpListenerResponse response, HttpStatusCode code)
        {
            try
            {
                response.StatusCode = (int)code;
                response.Close();
            }
            catch (Exception ex)
            {
                // log the error
                Logger.Error($"SendError RestListener Error" + ex.StackTrace);
            }
        }
    }
}
