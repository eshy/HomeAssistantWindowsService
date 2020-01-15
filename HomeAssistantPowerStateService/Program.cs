using HomeAssistantClient;
using System.ServiceProcess;

namespace HomeAssistantPowerStateService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HomeAssistantService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
