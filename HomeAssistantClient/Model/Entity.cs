using Newtonsoft.Json;
using Refit;

namespace HomeAssistantClient.Model
{
    public class Entity
    {
        [AliasAs("entity_id")]
        [JsonProperty(PropertyName = "entity_id")]
        public string Id { get; set; }
    }
}
