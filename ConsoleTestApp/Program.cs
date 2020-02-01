using HomeAssistantClient;
using System;
using System.Configuration;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                var token = ConfigurationManager.AppSettings["Token"];
                var restApiClient = new RestApiClient(baseUrl, token, null);
                var resumeScene = ConfigurationManager.AppSettings["ResumeSuspendScene"];
                restApiClient.ActivateScene(resumeScene);
            }
            catch (Exception ex)
            {
            }

        }
    }
}
