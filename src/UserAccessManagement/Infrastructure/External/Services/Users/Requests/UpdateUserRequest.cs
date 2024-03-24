using System.Text.Json.Serialization;

namespace UserAccessManagement.Infrastructure.External.Services.Users.Requests
{
    public class UpdateUserRequest
    {
        public string Id { get; init; }
        public UpdateUserProperty[] UpdateProperties { get; init; }
    }

    public class UpdateUserProperty
    {
        [JsonPropertyName("field")]
        public string Field { get; init; }

        [JsonPropertyName("value")]
        public string Value { get; init; }
    }
}
