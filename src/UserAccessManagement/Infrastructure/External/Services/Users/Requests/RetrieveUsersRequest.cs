namespace UserAccessManagement.Infrastructure.External.Services.Users.Requests
{
    public class RetrieveUsersRequest
    {
        public string Email { get; init; }
        public string EmployerId { get; init; }
    }
}
