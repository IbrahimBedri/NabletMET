using NLog.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForms_REST_Server
{
    public partial class MyForm1 : Form
    {
        private RestListener RestListener { get; set; }
        private string RestPrefix = "http://+:5973/";
        private List<TranscodeTask> TasksList = new List<TranscodeTask>();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MyForm1()
        {
            InitializeComponent();
        }


        internal bool SetRestListener(bool isEnabled)
        {
            Logger.Trace($"Setting REST Prefix to {RestPrefix}");
            if (isEnabled)
            {
                if (RestListener == null)
                {
                    RestListener = new RestListener(TasksList, RestPrefix);
                }
                return RestListener.Start();
            }
            else
            {
                RestListener?.Stop();
                return true;
            }
            
        }

        private void MyForm1_Load(object sender, EventArgs e)
        {

            var config = new NLog.Config.LoggingConfiguration();
            RichTextBoxTarget rtbTarget = new RichTextBoxTarget();
            rtbTarget.Layout = "${date:format=HH\\:MM\\:ss} ${message}";
            rtbTarget.ControlName = "myRichTextBox1";
            rtbTarget.FormName = "MyForm1";
            rtbTarget.UseDefaultRowColoringRules = true;
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(rtbTarget, NLog.LogLevel.Trace);
            RestListener = new RestListener(TasksList, RestPrefix);
            if (!SetRestListener(true))
            {
                // REST can't be started
                // inform the user about it                
            }
            //config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, rtbTarget);
        }
    }
}
