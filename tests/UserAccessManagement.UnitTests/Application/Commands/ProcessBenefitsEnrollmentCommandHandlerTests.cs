using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Application.Commands;
using UserAccessManagement.Application.Models;
using UserAccessManagement.Domain;
using UserAccessManagement.Infrastructure.Data;
using UserAccessManagement.Infrastructure.External.Services.Employers;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;
using UserAccessManagement.Infrastructure.External.Services.Users;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.External.Services.Users.Responses;
using UserAccessManagement.Infrastructure.Helpers;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Commands
{
    public class ProcessBenefitsEnrollmentCommandHandlerTests
    {
        private readonly Mock<ILogger<ProcessBenefitsEnrollmentCommandHandler>> _mockLogger;
        private readonly Mock<IUnitOfWork<UserAccessManagementContext>> _mockUnitOfWork;
        private readonly Mock<IValidator<BenefitsEnrollmentFileRecord>> _mockValidator;
        private readonly Mock<IRepository<BenefitsEnrollment>> _mockRepository;
        private readonly Mock<IEmployerService> _mockEmployerService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileHelper> _mockFileHelper;

        private readonly ProcessBenefitsEnrollmentCommandHandler _commandHandler;

        public ProcessBenefitsEnrollmentCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ProcessBenefitsEnrollmentCommandHandler>>();
            _mockRepository = new Mock<IRepository<BenefitsEnrollment>>();

            _mockUnitOfWork = new Mock<IUnitOfWork<UserAccessManagementContext>>();
            _mockUnitOfWork.Setup(x => x.Repository<BenefitsEnrollment>())
              .Returns(_mockRepository.Object);

            _mockValidator = new Mock<IValidator<BenefitsEnrollmentFileRecord>>();
            _mockEmployerService = new Mock<IEmployerService>();
            _mockUserService = new Mock<IUserService>();
            _mockFileHelper = new Mock<IFileHelper>();

            _commandHandler = new ProcessBenefitsEnrollmentCommandHandler(
               _mockLogger.Object,
               _mockUnitOfWork.Object,
               _mockValidator.Object,
               _mockEmployerService.Object,
               _mockUserService.Object,
               _mockFileHelper.Object);
        }

        [Fact]
        public void Ctor_Throws_Exception_When_Any_Service_Is_NuLL()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ProcessBenefitsEnrollmentCommandHandler(
                    null,
                    _mockUnitOfWork.Object,
                    _mockValidator.Object,
                    _mockEmployerService.Object,
                    _mockUserService.Object,
                    _mockFileHelper.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessBenefitsEnrollmentCommandHandler(
                    _mockLogger.Object,
                    null,
                    _mockValidator.Object,
                    _mockEmployerService.Object,
                    _mockUserService.Object,
                    _mockFileHelper.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessBenefitsEnrollmentCommandHandler(
                    _mockLogger.Object,
                    _mockUnitOfWork.Object,
                    null,
                    _mockEmployerService.Object,
                    _mockUserService.Object,
                    _mockFileHelper.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessBenefitsEnrollmentCommandHandler(
                    _mockLogger.Object,
                    _mockUnitOfWork.Object,
                    _mockValidator.Object,
                    null,
                    _mockUserService.Object,
                    _mockFileHelper.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new ProcessBenefitsEnrollmentCommandHandler(
                    _mockLogger.Object,
                    _mockUnitOfWork.Object,
                    _mockValidator.Object,
                    _mockEmployerService.Object,
                    null,
                    _mockFileHelper.Object));

            Assert.Throws<ArgumentNullException>(() =>
               new ProcessBenefitsEnrollmentCommandHandler(
                   _mockLogger.Object,
                   _mockUnitOfWork.Object,
                   _mockValidator.Object,
                   _mockEmployerService.Object,
                   _mockUserService.Object,
                   null));
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Command_Is_Null()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _commandHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Returns_False_When_Employer_Is_Not_Found()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer1.csv",
                EmployerName = "Employer 1"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([]));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Country_Value_Validation_Fails()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer1.csv",
                EmployerName = "Employer 1"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "1", Name = "Employer 1" }]));

            _mockFileHelper.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer1.csv"));

            _mockFileHelper.Setup(x => x.AddSuffix(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer1-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");

            _mockRepository.Setup(x => x.Remove(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
               .Returns(0);

            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0))
                .Returns(Task.FromResult(0));

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<BenefitsEnrollmentFileRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ValidationResult(new ValidationFailure[] { new("Country", "The length of 'Country' must be 2 characters or fewer. You entered 6 characters.") })));

            _mockUserService.Setup(x => x.DeleteUsersAsync(It.IsAny<DeleteUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Email_Value_Validation_Fails()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer1-2.csv",
                EmployerName = "Employer 1"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "1", Name = "Employer 1" }]));

            _mockFileHelper.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer1-2.csv"));

            _mockFileHelper.Setup(x => x.AddSuffix(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer1-2-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");

            _mockRepository.Setup(x => x.Remove(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
               .Returns(0);

            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0))
                .Returns(Task.FromResult(0));

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<BenefitsEnrollmentFileRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ValidationResult(new ValidationFailure[] { new("Email", "'Email' is not a valid email address.") })));

            _mockUserService.Setup(x => x.DeleteUsersAsync(It.IsAny<DeleteUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Field_Value_Has_Invalid_Format()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer2.csv",
                EmployerName = "Employer 2"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "2", Name = "Employer 2" }]));

            _mockFileHelper.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer2.csv"));

            _mockFileHelper.Setup(x => x.AddSuffix(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer2-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");

            _mockRepository.Setup(x => x.Remove(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
               .Returns(0);

            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0))
                .Returns(Task.FromResult(0));

            _mockUserService.Setup(x => x.DeleteUsersAsync(It.IsAny<DeleteUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Valid_And_Saves_Benefits_Enrollment()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer3.csv",
                EmployerName = "Employer 3"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "3", Name = "Employer 3" }]));

            _mockFileHelper.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer3.csv"));

            _mockFileHelper.Setup(x => x.AddSuffix(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer3-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");

            _mockRepository.Setup(x => x.Remove(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
               .Returns(0);

            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0))
                .Returns(Task.FromResult(1));

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<BenefitsEnrollmentFileRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ValidationResult(Array.Empty<ValidationFailure>())));

            _mockUserService.Setup(x => x.GetUsersAsync(It.IsAny<RetrieveUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveUserResponse>>([]));

            _mockRepository.Setup(x => x.AddAsync(It.IsAny<BenefitsEnrollment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new BenefitsEnrollment
                {
                    Email = "test@test.com",
                    FullName = "John Doe",
                    Country = "BR",
                    BirthDate = new DateTime(1992, 05, 03),
                    Salary = 7000,
                    EmployerName = "Employer 3"
                }));

            _mockUserService.Setup(x => x.DeleteUsersAsync(It.IsAny<DeleteUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Valid_And_Updates_User_And_Saves_Benefits_Enrollment()
        {
            // Arrange
            var command = new ProcessBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer4.csv",
                EmployerName = "Employer 4"
            };

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "4", Name = "Employer 4" }]));

            _mockFileHelper.Setup(x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer4.csv"));

            _mockFileHelper.Setup(x => x.AddSuffix(It.IsAny<string>(), It.IsAny<string>()))
                .Returns($"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}Files{Path.DirectorySeparatorChar}Employer4-Processing-Report-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");

            _mockRepository.Setup(x => x.Remove(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
               .Returns(0);

            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0))
                .Returns(Task.FromResult(1));

            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<BenefitsEnrollmentFileRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ValidationResult(Array.Empty<ValidationFailure>())));

            _mockUserService.Setup(x => x.GetUsersAsync(It.IsAny<RetrieveUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveUserResponse>>([new() { Id = "1", Email = "test@test.com", EmployerId = "4" }]));

            _mockUserService.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _mockRepository.Setup(x => x.AddAsync(It.IsAny<BenefitsEnrollment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new BenefitsEnrollment
                {
                    Email = "test@test.com",
                    FullName = "John Doe",
                    Country = "BR",
                    BirthDate = new DateTime(1992, 05, 03),
                    Salary = 7000,
                    EmployerName = "Employer 4"
                }));

            _mockUserService.Setup(x => x.DeleteUsersAsync(It.IsAny<DeleteUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }
    }
}
