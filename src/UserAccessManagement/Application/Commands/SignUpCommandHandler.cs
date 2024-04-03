using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Application.Exceptions;
using UserAccessManagement.Domain;
using UserAccessManagement.Infrastructure.Data;
using UserAccessManagement.Infrastructure.External.Services.Employers;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;
using UserAccessManagement.Infrastructure.External.Services.Users;
using UserAccessManagement.Infrastructure.External.Services.Users.Models;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.External.Services.Users.Responses;

namespace UserAccessManagement.Application.Commands
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, bool>
    {
        private readonly IRepository<BenefitsEnrollment> _repository;
        private readonly IEmployerService _employerService;
        private readonly IUserService _userService;

        public SignUpCommandHandler(
           IRepositoryFactory<UserAccessManagementContext> repositoryFactory,
           IEmployerService employerService,
           IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(repositoryFactory);

            _employerService = employerService ?? throw new ArgumentNullException(nameof(userService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _repository = repositoryFactory.Repository<BenefitsEnrollment>();
        }

        public async Task<bool> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var benefitsEnrollment = await GetBenefitsEnrollmentAsync(request.Email, cancellationToken);

            if (benefitsEnrollment is not null)
            {
                await CreateUserAsync(benefitsEnrollment, request.Password, cancellationToken);
            }
            else
            {
                await CreateUserAsync(request, cancellationToken);
            }

            return true;
        }

        private async Task<BenefitsEnrollment> GetBenefitsEnrollmentAsync(string email, CancellationToken cancellationToken)
        {
            var query = _repository.SingleResultQuery().AndFilter(x => x.Email == email);

            var benefitsEnrollment = await _repository.SingleOrDefaultAsync(query, cancellationToken);

            return benefitsEnrollment;
        }

        private async Task<bool> CreateUserAsync(BenefitsEnrollment benefitsEnrollment, string password, CancellationToken cancellationToken)
        {
            var employer = await GetEmployerAsync(benefitsEnrollment.EmployerName, cancellationToken);

            var createUserRequest = new CreateUserRequest
            {
                Email = benefitsEnrollment.Email,
                Password = password,
                Country = benefitsEnrollment.Country,
                AccessType = AccessType.Employer.ToString().ToLowerInvariant(),
                FullName = benefitsEnrollment.FullName,
                EmployerId = employer.Id,
                BirthDate = benefitsEnrollment.BirthDate,
                Salary = benefitsEnrollment.Salary
            };

            var response = await _userService.CreateUserAsync(createUserRequest, cancellationToken);

            return response;
        }

        private async Task<RetrieveEmployerResponse> GetEmployerAsync(string employerName, CancellationToken cancellationToken)
        {
            var retrieveEmployersRequest = new RetrieveEmployersRequest
            {
                Name = employerName
            };

            var employers = await _employerService.GetEmployersAsync(retrieveEmployersRequest, cancellationToken);

            var employer = employers?.SingleOrDefault() ?? throw new NotFoundException($"Employer '{employerName}' was not found.");

            return employer;
        }

        private async Task<bool> CreateUserAsync(SignUpCommand request, CancellationToken cancellationToken)
        {
            await CheckIfEmailExistsAsync(request.Email, cancellationToken);

            var createUserRequest = new CreateUserRequest
            {
                Email = request.Email,
                Password = request.Password,
                Country = request.Country,
                AccessType = AccessType.DTC.ToString().ToLowerInvariant()
            };

            var response = await _userService.CreateUserAsync(createUserRequest, cancellationToken);

            return response;
        }

        private async Task CheckIfEmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            var users = await GetUsersAsync(email, cancellationToken);

            if (users is not null && users.Any())
            {
                throw new ConflictException($"Email '{email}' already exists.");
            }
        }

        private async Task<IEnumerable<RetrieveUserResponse>> GetUsersAsync(string email, CancellationToken cancellationToken)
        {
            var retrieveUsersRequest = new RetrieveUsersRequest
            {
                Email = email
            };

            var users = await _userService.GetUsersAsync(retrieveUsersRequest, cancellationToken);

            return users;
        }
    }
}
