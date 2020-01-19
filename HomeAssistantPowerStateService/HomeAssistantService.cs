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
        private string logLocation;

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

            logLocation = ConfigurationManager.AppSettings["LogLocation"];

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
            var sceneKeyName = $"{powerStatus}Scene";
            LaunchScene(sceneKeyName);
            /*
            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeSuspend:
                    LaunchScene("ResumeSuspendScene");
                    break;
                case PowerBroadcastStatus.Suspend:
                    LaunchScene("SuspendScene");
                    break;
            }
            */
            return base.OnPowerEvent(powerStatus);
        }

        private void LaunchScene(string sceneKeyName)
        {
            try
            {
                WriteToFile($"Launch {sceneKeyName} {{0}}");
                EventLog.WriteEntry($"Launch {sceneKeyName}");

                var sceneName = ConfigurationManager.AppSettings[sceneKeyName];
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    restApiClient.ActivateScene(sceneName);
                }
            }
            catch (Exception ex)
            {
                WriteToFile($"Exception Launching {sceneKeyName} - {ex.Message} {{0}}");
                WriteToFile(ex.ToString());
            }

        }

        private void WriteToFile(string text)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["LogToFile"] ?? "false"))
                return;

            string path = Path.Combine(logLocation + "ServiceLog.txt");
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
    }
}
