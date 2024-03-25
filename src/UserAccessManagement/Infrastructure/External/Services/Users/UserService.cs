using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.External.Services.Users.Responses;

namespace UserAccessManagement.Infrastructure.External.Services.Users
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IEnumerable<RetrieveUserResponse>> GetUsersAsync(RetrieveUsersRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.GetAsync($"/users?email={request.Email}&employer_id={request.EmployerId}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var usersResponse = JsonSerializer.Deserialize<IEnumerable<RetrieveUserResponse>>(json);

            return usersResponse;
        }

        public async Task<RetrieveUserResponse> GetUserAsync(RetrieveUserRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.GetAsync($"/users/{request.Id}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var userResponse = JsonSerializer.Deserialize<RetrieveUserResponse>(json);

            return userResponse;
        }

        public async Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.PostAsJsonAsync("/users", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.PatchAsJsonAsync($"/users/{request.Id}", request.UpdateProperties, cancellationToken);

            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> DeleteUsersAsync(DeleteUsersRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, $"/users")
                {
                    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}
