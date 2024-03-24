using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;

namespace UserAccessManagement.Infrastructure.External.Services.Employers
{
    public interface IEmployerService
    {
        Task<IEnumerable<RetrieveEmployerResponse>> GetEmployersAsync(
            RetrieveEmployersRequest request,
            CancellationToken cancellationToken = default);
    }
}
