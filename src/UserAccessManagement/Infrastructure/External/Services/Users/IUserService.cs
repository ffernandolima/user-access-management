using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.External.Services.Users.Responses;

namespace UserAccessManagement.Infrastructure.External.Services.Users
{
    public interface IUserService
    {
        Task<IEnumerable<RetrieveUserResponse>> GetUsersAsync(RetrieveUsersRequest request, CancellationToken cancellationToken = default);
        Task<RetrieveUserResponse> GetUserAsync(RetrieveUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// New API: Created a new route capable of deleting users on the UserService side who are not included in the request IDs.
        /// The users not included in the request IDs indicate that they are no longer present in the eligibility file.
        /// 
        /// Although it might not be the best approach to do so because it will be over HTTP, I do believe that other options could be considered:
        /// - Implementing queues and background processing on the UserService side.
        /// - Sending the request IDs through a message or using the path of a file in a blob storage containing the IDs.
        /// </summary>
        Task<bool> DeleteUsersAsync(DeleteUsersRequest request, CancellationToken cancellationToken = default);
    }
}
