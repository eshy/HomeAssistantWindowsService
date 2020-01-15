using HomeAssistantClient.Model;
using Refit;

namespace HomeAssistantClient
{
    public class RestApiClient
    {
        private IHomeAssistantApi _apiClient;
        private string _baseUrl;
        private string _token;
        public RestApiClient(string baseUrl, string token)
        {
            _baseUrl = baseUrl;
            _token = token;
        }
        public IHomeAssistantApi ApiClient { 
            get
            {
                _apiClient = _apiClient ?? RestService.For<IHomeAssistantApi>(_baseUrl);
                return _apiClient;
            }
        }
        public void ActivateScene(string sceneId)
        {
            //var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            //var token = ConfigurationManager.AppSettings["Token"];
            ApiClient.ActivateScene(new Entity { Id = sceneId }, _token).Wait();
        }
    }
}
