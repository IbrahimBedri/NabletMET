
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinForms_REST_Server
{
    public class TranscodeTask
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Process myProcess;
        public bool InProgress { get; set; }

        private string sourceName = @"";
        public string SourceName
        {
            get => sourceName;
            set
            {
                sourceName = value;
                ShortenSourceName = PathShortener(sourceName, 50);
            }
        }

        public string ShortenSourceName { get; set; }

        public string Preset { get; set; }
        public string OutputName { get; set; } = @"";

        public double Duration { get; set; }

        public string DurationFormatted
        {
            get
            {
                TimeSpan ts = TimeSpan.FromSeconds(Duration);
                return ts.ToString(@"hh\:mm\:ss\.fff");
            }
        }

        public string Status { get; set; }

        public string CurrentFps { get; set; }

        public int Progress { get; set; }

        public string Percentage { get; set; }

        public string SpentTime { get; set; }

        public bool? InitTask()
        {
            // get initial info for transcoding
            myProcess = new Process();
            myProcess.StartInfo.FileName = @"C:\Users\Ibrahim Bedri\Desktop\New folder (2)\bin\x64\Release\transcode.exe";
            myProcess.StartInfo.Arguments = @"-info json -i """ + sourceName + @""" -o """ + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testOutput.json") + @"""";
            // Allows to raise event when the process is finished
            myProcess.EnableRaisingEvents = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.RedirectStandardOutput = true;

            // Eventhandler wich fires when exited
            myProcess.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            myProcess.ErrorDataReceived += MyProcess_ErrorDataReceived;
            myProcess.Exited += new EventHandler(myProcess_Exited);
            // Starts the process
            isInfo = true;
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();
            Thread.Sleep(5000);
            myProcess.WaitForExit(10000);

            fileInfo = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testOutput.json"));
            var jSon = JObject.Parse(fileInfo);
            try
            {
                if (jSon.Property("sources").Values().Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            //List<string> infoList = fileInfo.Split('\n').ToList();
            //if (fileInfo.Contains(@"An error occured during initialization"))
            //{
            //    return null;
            //}
            //else
            //{
            //    // for quick tests only
            //    if (fileInfo.Length > 1 && infoList.Count() > 20)
            //    {
            //        fileInfo = fileInfo.Replace("Done.", "").Trim();
            //        fileInfo = fileInfo.Substring(fileInfo.IndexOf("{")).Replace(@"\""", @"""");
            //        //string info = 
            //        //foreach (string info in infoList)
            //        //{
            //        //    if (info.StartsWith("ME_PD_DURATION"))
            //        //    {
            //        //        string temp = info.Substring(info.IndexOf(':') + 1).Trim();
            //        //        string delimiter = temp.Substring(temp.IndexOf(':') + 1);
            //        //        string trueDelimiter = delimiter.Substring(0, delimiter.IndexOf(')'));
            //        //        Double.TryParse(trueDelimiter, out double delim);
            //        //        string strDuration = temp.Substring(0, temp.IndexOf(' ')).Trim();
            //        //        if (Double.TryParse(strDuration, out double tmpDuration))
            //        //        {
            //        //            Duration = tmpDuration / (double)delim;
            //        //            Status = "Ready";

            //        //            InProgress = false;
            //        //            return true;
            //        //        }
            //        //    }
            //        //}
            //        return true;
            //    }
            //}
            //return false;
        }

        public int ID;

        public string GetInfo()
        {
            return fileInfo;
        }
        internal JObject GetStat()
        {
            string fileContent = string.Empty;
            try
            {
                using (FileStream stream = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testStat" + ID + ".json"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                var jSon = JObject.Parse(fileContent);
                return jSon;
            }
            catch (Exception ex)
            {
                return new JObject();
            }
        }

        public void RunTask()
        {
            // start transcoding
            Progress = 0;
            myProcess = new Process();
            Preset = @"C:\Users\Ibrahim Bedri\Desktop\New folder (2)\presets\Avid\DNxHD_36_1080p25.txt";
            OutputName = @"C:\inetpub\wwwroot\files\TestRecordingTask_" + ID + ".mxf";
            //OutputName = @"c:\Temp\TestRecordingTask_" + ID + ".mxf";
            myProcess.StartInfo.FileName = @"C:\Users\Ibrahim Bedri\Desktop\New folder (2)\bin\x64\Release\transcode.exe";
            myProcess.StartInfo.Arguments = @"""" + Preset + @"""" +
                                            " -i " + @"""" + sourceName + @"""" + " -nwto 10000 -o " + @"""" + OutputName + @""" -json-stats """
                                            + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testStat" + ID + ".json") + @"""";
            // Allows to raise event when the process is finished
            myProcess.EnableRaisingEvents = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            myProcess.ErrorDataReceived += MyProcess_ErrorDataReceived;
            myProcess.Exited += new EventHandler(myProcess_Exited);
            // Starts the process
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();
            Status = "In Progress";
            InProgress = true;
            Logger.Info($"Task {ID} has been started");
        }

        bool isStopPressed = false;
        public void StopTask()
        {
            if (myProcess == null)
                return;
            // stop transcoding
            isStopPressed = true;
            if (AttachConsole((uint)myProcess.Id))
            {
                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, (uint)myProcess.Id))
                        return;
                }
                finally
                {
                    FreeConsole();
                }
            }
            Status = "Ready";
            InProgress = false;
            Logger.Info($"Task {ID} has been stopped");
        }

        private void MyProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (isInfo)
            {
                fileInfo += e.Data + "\n";
                return;
            }
            ProcessTranscodeInfo(e.Data);
        }

        private void ProcessTranscodeInfo(string transcodeInfo)
        {
            if (string.IsNullOrEmpty(transcodeInfo))
                return;
            try
            {
                string strPos = new Regex(@"POS=(.+?) ").Match(transcodeInfo).Value.Trim().Replace("POS=", "");
                CurrentFps = new Regex(@"CurFPS=(.+?) ").Match(transcodeInfo).Value.Trim().Replace("CurFPS=", "");
                string tmpTime = new Regex(@"ET=(.+?) ").Match(transcodeInfo).Value.Trim().Replace("ET=", "");
                SpentTime = tmpTime.Contains(',') ? tmpTime.Replace(",", "") : tmpTime;
                if (!string.IsNullOrEmpty(strPos) && Double.TryParse(strPos, out double pos))
                {
                    Progress = (int)(pos * 100.0 / (27000000.0 * Duration));
                    Percentage = (pos / (27000000.0 * Duration)).ToString("P2");
                }
            }
            catch { }
        }

        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            if (isInfo)
            {
                isInfo = false;
                return;
            }
            if (myProcess.ExitCode == 0)
            {
                Status = isStopPressed ? "Ready" : "Success";
            }
            else
            {
                Status = "Error";
            }
            InProgress = false;
            myProcess.Close();
            myProcess = null;
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (isInfo)
            {
                fileInfo += e.Data + "\n";
                return;
            }
            ProcessTranscodeInfo(e.Data);
        }

        bool isInfo = false;
        string fileInfo = string.Empty;
        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);
        enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx(
                       [Out] StringBuilder pszOut,
                       string szPath,
                       int cchMax,
                       int dwFlags);

        static string PathShortener(string path, int length)
        {
            StringBuilder sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }
    }
}
