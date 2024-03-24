using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;

namespace UserAccessManagement.Infrastructure.External.Services.Employers
{
    public class EmployerService : IEmployerService
    {
        private readonly HttpClient _httpClient;

        public EmployerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IEnumerable<RetrieveEmployerResponse>> GetEmployersAsync(
            RetrieveEmployersRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await _httpClient.GetAsync($"/employers?name={request.Name}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var employersResponse = JsonSerializer.Deserialize<IEnumerable<RetrieveEmployerResponse>>(json);

            return employersResponse;
        }
    }
}
