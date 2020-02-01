using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using HomeAssistantPowerStateService;

namespace HomeAssistantClient
{
    public partial class HomeAssistantService : ServiceBase
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task mainTask = null;
        private TimeSpan WaitAfterSuccessInterval = TimeSpan.FromSeconds(60);
        private TimeSpan WaitAfterErrorInterval = TimeSpan.FromSeconds(120);
        private RestApiClient restApiClient;
        private ILogger _logger;

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

            var logLocation = ConfigurationManager.AppSettings["LogLocation"];
            if (bool.Parse(ConfigurationManager.AppSettings["LogToFile"] ?? "false"))
            {
                _logger = new TextFileLogger(logLocation);
            }

            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            var token = ConfigurationManager.AppSettings["Token"];
            restApiClient = new RestApiClient(baseUrl, token, _logger);
        }

        protected override void OnStart(string[] args)
        {
            _logger?.LogDebug("Home Assistant Service started");
            
            mainTask = new Task(Poll, cts.Token, TaskCreationOptions.LongRunning);
            mainTask.Start();
        }

        protected override void OnStop()
        {
            _logger?.LogDebug("Home Assistant Service stopped");
            cts.Cancel();
            mainTask.Wait();
        }

        protected override void OnPause()
        {
            _logger?.LogDebug("Home Assistant Service paused");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            _logger?.LogDebug("Home Assistant Service continued");
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
            _logger?.LogDebug($"In OnPowerEvent {powerStatus}");
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
                _logger?.LogDebug($"Launch {sceneKeyName}");
                EventLog.WriteEntry($"Launch {sceneKeyName}");

                var sceneName = ConfigurationManager.AppSettings[sceneKeyName];
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    _logger?.LogDebug($"Call API to Activate Scene {sceneName}");
                    restApiClient.ActivateScene(sceneName);
                    _logger?.LogDebug($"Call API to Activate Scene {sceneName} Done");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Exception Launching {sceneKeyName} - {ex.Message}");
            }

        }


    }
}
