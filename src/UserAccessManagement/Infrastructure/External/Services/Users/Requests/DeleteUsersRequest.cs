using System.Text.Json.Serialization;

namespace UserAccessManagement.Infrastructure.External.Services.Users.Requests
{
    public class DeleteUsersRequest
    {
        /// <summary>
        /// Existing user ids on both sides: UserService and eligibility file. 
        /// These users should be kept on the database and the others deleted 
        /// (because they are no longer coming in the eligibility file).
        /// </summary>
        [JsonPropertyName("ids")]
        public string[] Ids { get; init; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; init; }
    }
}
