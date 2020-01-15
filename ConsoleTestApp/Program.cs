using HomeAssistantClient;
using System.Configuration;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            var token = ConfigurationManager.AppSettings["Token"];
            var restApiClient = new RestApiClient(baseUrl, token);
            var resumeScene = ConfigurationManager.AppSettings["ResumeScene"];
            restApiClient.ActivateScene(resumeScene);
        }
    }
}
