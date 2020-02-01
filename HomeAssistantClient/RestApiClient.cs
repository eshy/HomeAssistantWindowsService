using HomeAssistantClient.Model;
using Refit;
using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;

namespace HomeAssistantClient
{
    public class RestApiClient
    {
        private IHomeAssistantApi _apiClient;
        private string _baseUrl;
        private string _token;
        private ILogger _logger;
        private static readonly int _numberOfRetries = 15;

        public RestApiClient(string baseUrl, string token, ILogger logger)
        {
            _baseUrl = baseUrl;
            _token = token;
            _logger = logger;
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
            _logger?.LogTrace($"{nameof(ActivateScene)} - SceneId:{sceneId} - Start");

            HttpStatusCode[] httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.RequestTimeout, //408
                HttpStatusCode.NotFound, //404
                HttpStatusCode.InternalServerError, //500
                HttpStatusCode.BadGateway, //502
                HttpStatusCode.ServiceUnavailable, //503
                HttpStatusCode.GatewayTimeout //504
            };


            Policy.Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .WaitAndRetryAsync(_numberOfRetries, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (response, timeSpan, context) =>
                        {
                            _logger?.LogError(response.Exception, $"Retry wait for {timeSpan.TotalSeconds} after {response.Result.StatusCode} - {response.Exception.Message}");
                        }
                    )
                .ExecuteAsync(async () => await CallActivateSceneApi(sceneId))
                .Wait();

            _logger?.LogTrace($"{nameof(ActivateScene)} - SceneId:{sceneId} - Finished");
        }

        private async Task<HttpResponseMessage> CallActivateSceneApi(string sceneId)
        {
            _logger?.LogTrace($"{nameof(ActivateScene)} - SceneId:{sceneId} - Start");

            try
            {
                for (var i=0; i < 20; i++)
                {

                    _logger?.LogTrace($"{nameof(ActivateScene)} - SceneId:{sceneId} - Check Network {i}");
                    if (IsNetworkAvailable())
                    {
                        break;
                    }
                    Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(CallActivateSceneApi)} - SceneId:{sceneId} - Wait For Network - {ex.ToString()}");
            }

            try
            {
                var r = await ApiClient.ActivateScene(new Entity { Id = sceneId }, _token);
                _logger?.LogTrace($"{nameof(CallActivateSceneApi)} - SceneId:{sceneId} - Finished");
                return r;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(CallActivateSceneApi)} - SceneId:{sceneId} - {ex.ToString()}");
                throw ex;
            }

        }

        /// <summary>
        /// Indicates whether any network connection is available.
        /// Filter connections below a specified speed, as well as virtual network cards.
        /// </summary>
        /// <param name="minimumSpeed">The minimum speed required. Passing 0 will not filter connection using speed.</param>
        /// <returns>
        ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNetworkAvailable(long minimumSpeed=0)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // discard because of standard reasons
                if ((ni.OperationalStatus != OperationalStatus.Up) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;

                // this allow to filter modems, serial, etc.
                // I use 10000000 as a minimum speed for most cases
                if (ni.Speed < minimumSpeed)
                    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;

                return true;
            }
            return false;
        }
    }
}
