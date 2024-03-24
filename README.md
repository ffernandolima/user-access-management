# user-access-management

Implementation of the [backend take-home assignment](https://github.com/OriginFinancial/origin-backend-take-home-assignment).

## Non-functional requirements

[New API](https://github.com/ffernandolima/user-access-management/blob/e29e3fd8ace95aaf19108d79f86cfe8c976decda/src/UserAccessManagement/Infrastructure/External/Services/Users/IUserService.cs#L24): Created a new route capable of deleting users on the UserService side who are not included in the request IDs.
The users not included in the request IDs indicate that they are no longer present in the eligibility file.

Although it might not be the best approach to do so because it will be over HTTP, I do believe that other options could be considered:
- Implementing queues and background processing on the UserService side.
- Sending the request IDs through a message or using the path of a file in a blob storage containing the IDs.
