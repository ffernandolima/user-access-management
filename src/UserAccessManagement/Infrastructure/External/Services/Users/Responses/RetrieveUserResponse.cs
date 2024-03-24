using System;
using System.Text.Json.Serialization;

namespace UserAccessManagement.Infrastructure.External.Services.Users.Responses
{
    public partial class RetrieveUserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; }

        [JsonPropertyName("country")]
        public string Country { get; init; }

        [JsonPropertyName("access_type")]
        public string AccessType { get; init; }

        [JsonPropertyName("full_name")]
        public string FullName { get; init; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; init; }

        [JsonPropertyName("birth_date")]
        public DateTime BirthDate { get; init; }

        [JsonPropertyName("salary")]
        public decimal Salary { get; init; }
    }
}
