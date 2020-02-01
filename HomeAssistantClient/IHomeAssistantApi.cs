using HomeAssistantClient.Model;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace HomeAssistantClient
{
    public interface IHomeAssistantApi
    {
        [Post("/api/services/scene/turn_on")]
        Task<HttpResponseMessage> ActivateScene([Body]Entity entity, [Header("Authorization")] string authorization);
    }
}
