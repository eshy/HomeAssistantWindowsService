using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace HomeAssistantClient
{
    public partial class HomeAssistantService : ServiceBase
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task mainTask = null;
        private TimeSpan WaitAfterSuccessInterval = TimeSpan.FromSeconds(60);
        private TimeSpan WaitAfterErrorInterval = TimeSpan.FromSeconds(120);
        private RestApiClient restApiClient;
        public HomeAssistantService()
        {
            InitializeComponent();
            CanHandlePowerEvent = true;

            //Setup Service
            ServiceName = "HomeAssistantPowerStateService";
            CanStop = true;
            CanPauseAndContinue = true;

            //Setup logging
            AutoLog = false;


            ((ISupportInitialize)EventLog).BeginInit();
            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, "Application");
            }
            ((ISupportInitialize)EventLog).EndInit();

            EventLog.Source = ServiceName;
            EventLog.Log = "Application";

            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            var token = ConfigurationManager.AppSettings["Token"];
            restApiClient = new RestApiClient(baseUrl, token);
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Home Assistant Service started {0}");
            
            mainTask = new Task(Poll, cts.Token, TaskCreationOptions.LongRunning);
            mainTask.Start();
        }

        protected override void OnStop()
        {
            WriteToFile("Home Assistant Service stopped {0}");
            cts.Cancel();
            mainTask.Wait();
        }

        protected override void OnPause()
        {
            WriteToFile("Home Assistant Service paused {0}");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            WriteToFile("Home Assistant Service continued {0}");
            base.OnContinue();
        }

        private void Poll()
        {
            var cancellation = cts.Token;
            var interval = TimeSpan.Zero;
            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    //TODO: Send API Call if Device Status 
                    //SendMQTTEvent("5");
                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = WaitAfterSuccessInterval;
                }
                catch (Exception ex)
                {
                    //Log the exception
                    interval = WaitAfterErrorInterval;
                }
            }
        }


        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            WriteToFile($"In OnPowerEvent {powerStatus} {{0}}");
            EventLog.WriteEntry($"In OnPowerEvent {powerStatus}");
            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeSuspend:
                    //TODO: Send API Call to Home Assistant
                    WriteToFile("Launch Resume Scene {0}");
                    EventLog.WriteEntry("Launch Resume Scene");
                    var resumeScene = ConfigurationManager.AppSettings["ResumeScene"];
                    restApiClient.ActivateScene(resumeScene);
                    //SendMQTTEvent("1");
                    break;
                case PowerBroadcastStatus.Suspend:
                    //TODO: Send API Call to Home Assistant
                    WriteToFile("Launch Suspend Scene {0}");
                    EventLog.WriteEntry("Launch Suspend Scene");
                    var suspendScene = ConfigurationManager.AppSettings["SuspendScene"];
                    restApiClient.ActivateScene(suspendScene);
                    //SendMQTTEvent("0");
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }

        private void WriteToFile(string text)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["LogToFile"] ?? "false"))
                return;

            string path = @"D:\Dev\HAService\ServiceLog.txt";
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
    }
}
