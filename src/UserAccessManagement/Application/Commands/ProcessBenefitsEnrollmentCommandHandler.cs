using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using FileHelpers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Application.Exceptions;
using UserAccessManagement.Application.Mappers;
using UserAccessManagement.Application.Models;
using UserAccessManagement.Domain;
using UserAccessManagement.Infrastructure.Data;
using UserAccessManagement.Infrastructure.External.Services.Employers;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;
using UserAccessManagement.Infrastructure.External.Services.Users;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.Helpers;
using UserAccessManagement.Infrastructure.Threading;

namespace UserAccessManagement.Application.Commands
{
    public class ProcessBenefitsEnrollmentCommandHandler : IRequestHandler<ProcessBenefitsEnrollmentCommand, bool>
    {
        private readonly ILogger<ProcessBenefitsEnrollmentCommandHandler> _logger;
        private readonly IUnitOfWork<UserAccessManagementContext> _unitOfWork;
        private readonly IValidator<BenefitsEnrollmentFileRecord> _validator;
        private readonly IRepository<BenefitsEnrollment> _repository;
        private readonly IEmployerService _employerService;
        private readonly IUserService _userService;
        private readonly IFileHelper _fileHelper;
        private readonly HashSet<string> _userIds;

        public ProcessBenefitsEnrollmentCommandHandler(
            ILogger<ProcessBenefitsEnrollmentCommandHandler> logger,
            IUnitOfWork<UserAccessManagementContext> unitOfWork,
            IValidator<BenefitsEnrollmentFileRecord> validator,
            IEmployerService employerService,
            IUserService userService,
            IFileHelper fileHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(unitOfWork));
            _employerService = employerService ?? throw new ArgumentNullException(nameof(userService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            _repository = unitOfWork.Repository<BenefitsEnrollment>();
            _userIds = [];
        }

        public async Task<bool> Handle(ProcessBenefitsEnrollmentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            using var employerLock = await AsyncLocker.LockAsync(request.EmployerName);

            try
            {
                var employer = await GetEmployerAsync(request.EmployerName, cancellationToken);

                var downloadedFile = await _fileHelper.DownloadFileAsync(request.File, cancellationToken: cancellationToken);

                await RemoveExistingEnrollmentsAsync(employer.Name, cancellationToken);

                await ProcessDownloadedFile(downloadedFile, employer, cancellationToken);

                await DeleteUsersAsync(employer.Id, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred while processing employer '{EmployerName}'", request.EmployerName);

                return false;
            }
        }

        private async Task ProcessDownloadedFile(string downloadedFile, RetrieveEmployerResponse employer, CancellationToken cancellationToken)
        {
            var readEngine = new FileHelperAsyncEngine<BenefitsEnrollmentFileRecord>(Encoding.UTF8);

            readEngine.ErrorManager!.ErrorLimit = int.MaxValue;
            readEngine.ErrorManager!.ErrorMode = ErrorMode.SaveAndContinue;

            var writeEngine = new FileHelperAsyncEngine<BenefitsEnrollmentProcessedRecord>(Encoding.UTF8);

            writeEngine.ErrorManager!.ErrorLimit = int.MaxValue;
            writeEngine.ErrorManager!.ErrorMode = ErrorMode.SaveAndContinue;

            var processedFile = _fileHelper.AddSuffix(downloadedFile, $"-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}");

            using (readEngine.BeginReadFile(downloadedFile))
            using (writeEngine.BeginWriteFile(processedFile))
            {
                foreach (BenefitsEnrollmentFileRecord record in readEngine)
                {
                    try
                    {
                        if (readEngine.ErrorManager.HasErrors)
                        {
                            foreach (var error in readEngine.ErrorManager.Errors.OrderBy(x => x.LineNumber))
                            {
                                writeEngine.WriteNext(new BenefitsEnrollmentProcessedRecord(error));
                            }

                            readEngine.ErrorManager.ClearErrors();
                        }

                        var result = await _validator.ValidateAsync(record, cancellationToken);

                        if (!result.IsValid)
                        {
                            writeEngine.WriteNext(new BenefitsEnrollmentProcessedRecord(record.LineNumber, result));
                        }
                        else
                        {
                            await UpdateUserAsync(record, employer.Id, cancellationToken);

                            var entity = BenefitsEnrollmentMapper.Map(record, employer.Name);

                            await _repository.AddAsync(entity, cancellationToken);

                            writeEngine.WriteNext(new BenefitsEnrollmentProcessedRecord(record.LineNumber, "Successfully Processed."));
                        }
                    }
                    catch (Exception ex)
                    {
                        writeEngine.WriteNext(new BenefitsEnrollmentProcessedRecord(record.LineNumber, ex));
                    }
                }

                if (readEngine.ErrorManager.HasErrors)
                {
                    foreach (var error in readEngine.ErrorManager.Errors.OrderBy(x => x.LineNumber))
                    {
                        writeEngine.WriteNext(new BenefitsEnrollmentProcessedRecord(error));
                    }

                    readEngine.ErrorManager.ClearErrors();
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            }
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

        private async Task RemoveExistingEnrollmentsAsync(string employerName, CancellationToken cancellationToken)
        {
            _repository.Remove(x => x.EmployerName == employerName);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }

        private async Task UpdateUserAsync(BenefitsEnrollmentFileRecord record, string employerId, CancellationToken cancellationToken)
        {
            var retrieveUsersRequest = new RetrieveUsersRequest
            {
                Email = record.Email,
                EmployerId = employerId
            };

            var users = await _userService.GetUsersAsync(retrieveUsersRequest, cancellationToken);

            var user = users?.SingleOrDefault();

            if (user is null)
            {
                return;
            }

            _userIds.Add(user.Id);

            var updateUserRequest = new UpdateUserRequest
            {
                Id = user.Id,
                UpdateProperties =
                [
                    new UpdateUserProperty
                    {
                        Field =  "country",
                        Value = record.Country,
                    },
                    new UpdateUserProperty
                    {
                        Field =  "salary",
                        Value = record.Salary?.ToString("C"),
                    }
                ]
            };

            await _userService.UpdateUserAsync(updateUserRequest, cancellationToken);
        }

        private async Task DeleteUsersAsync(string employerId, CancellationToken cancellationToken)
        {
            var deleteUsersRequest = new DeleteUsersRequest
            {
                Ids = [.. _userIds],
                EmployerId = employerId
            };

            await _userService.DeleteUsersAsync(deleteUsersRequest, cancellationToken);
        }
    }
}
