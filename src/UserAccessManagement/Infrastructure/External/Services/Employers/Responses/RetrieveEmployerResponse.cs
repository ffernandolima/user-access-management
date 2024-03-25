using System.Text.Json.Serialization;

namespace UserAccessManagement.Infrastructure.External.Services.Employers.Responses
{
    public class RetrieveEmployerResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }
    }
}
