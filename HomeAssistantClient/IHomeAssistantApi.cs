using HomeAssistantClient.Model;
using Refit;
using System.Threading.Tasks;

namespace HomeAssistantClient
{
    public interface IHomeAssistantApi
    {
        [Post("/api/services/scene/turn_on")]
        Task ActivateScene([Body]Entity entity, [Header("Authorization")] string authorization);
    }
}
