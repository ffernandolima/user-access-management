using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserAccessManagement.Application.Commands;
using UserAccessManagement.Application.Exceptions;
using UserAccessManagement.Domain;
using UserAccessManagement.Infrastructure.Data;
using UserAccessManagement.Infrastructure.External.Services.Employers;
using UserAccessManagement.Infrastructure.External.Services.Employers.Requests;
using UserAccessManagement.Infrastructure.External.Services.Employers.Responses;
using UserAccessManagement.Infrastructure.External.Services.Users;
using UserAccessManagement.Infrastructure.External.Services.Users.Requests;
using UserAccessManagement.Infrastructure.External.Services.Users.Responses;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Commands
{
    public class SignUpCommandHandlerTests
    {
        private readonly Mock<IRepositoryFactory<UserAccessManagementContext>> _mockRepositoryFactory;
        private readonly Mock<IRepository<BenefitsEnrollment>> _mockRepository;
        private readonly Mock<IEmployerService> _mockEmployerService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly SignUpCommandHandler _commandHandler;

        public SignUpCommandHandlerTests()
        {
            _mockRepository = new Mock<IRepository<BenefitsEnrollment>>();

            _mockRepositoryFactory = new Mock<IRepositoryFactory<UserAccessManagementContext>>();
            _mockRepositoryFactory.Setup(x => x.Repository<BenefitsEnrollment>())
                .Returns(_mockRepository.Object);

            _mockEmployerService = new Mock<IEmployerService>();
            _mockUserService = new Mock<IUserService>();

            _commandHandler = new SignUpCommandHandler(
                _mockRepositoryFactory.Object,
                _mockEmployerService.Object,
                _mockUserService.Object);
        }

        [Fact]
        public void Ctor_Throws_Exception_When_Any_Service_Is_NuLL()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
                new SignUpCommandHandler(null, _mockEmployerService.Object, _mockUserService.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new SignUpCommandHandler(_mockRepositoryFactory.Object, null, _mockUserService.Object));

            Assert.Throws<ArgumentNullException>(() =>
                new SignUpCommandHandler(_mockRepositoryFactory.Object, _mockEmployerService.Object, null));
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Command_Is_Null()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _commandHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Email_Already_Exists()
        {
            // Arrange
            var command = new SignUpCommand
            {
                Email = "test@test.com",
                Password = "x4xxfgKr5T*",
                Country = "BR"
            };

            var mockQuery = Mock.Of<ISingleResultQuery<BenefitsEnrollment>>();

            _mockRepository.Setup(x => x.SingleResultQuery())
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleResultQuery().AndFilter(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
                .Returns(mockQuery);

            _mockUserService.Setup(x => x.GetUsersAsync(It.IsAny<RetrieveUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveUserResponse>>([new() { Email = "test@test.com" }]));

            // Assert
            await Assert.ThrowsAsync<ConflictException>(async () =>
                await _commandHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Creates_DTC_User_And_Returns_True()
        {
            // Arrange
            var command = new SignUpCommand
            {
                Email = "test@test.com",
                Password = "x4xxfgKr5T*",
                Country = "BR"
            };

            var mockQuery = Mock.Of<ISingleResultQuery<BenefitsEnrollment>>();

            _mockRepository.Setup(x => x.SingleResultQuery())
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleResultQuery().AndFilter(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
                .Returns(mockQuery);

            _mockUserService.Setup(x => x.GetUsersAsync(It.IsAny<RetrieveUsersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveUserResponse>>([]));

            _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Employer_Is_Not_Found()
        {
            // Arrange
            var command = new SignUpCommand
            {
                Email = "test@test.com",
                Password = "x4xxfgKr5T*",
                Country = "BR"
            };

            var mockQuery = Mock.Of<ISingleResultQuery<BenefitsEnrollment>>();

            _mockRepository.Setup(x => x.SingleResultQuery())
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleResultQuery().AndFilter(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleOrDefaultAsync(It.IsAny<IQuery<BenefitsEnrollment>>(), It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(new BenefitsEnrollment { Email = "test@test.com" }));

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([]));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _commandHandler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Creates_Employer_User_And_Returns_True()
        {
            // Arrange
            var command = new SignUpCommand
            {
                Email = "test@test.com",
                Password = "x4xxfgKr5T*",
                Country = "BR"
            };

            var mockQuery = Mock.Of<ISingleResultQuery<BenefitsEnrollment>>();

            _mockRepository.Setup(x => x.SingleResultQuery())
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleResultQuery().AndFilter(It.IsAny<Expression<Func<BenefitsEnrollment, bool>>>()))
                .Returns(mockQuery);

            _mockRepository.Setup(x => x.SingleOrDefaultAsync(It.IsAny<IQuery<BenefitsEnrollment>>(), It.IsAny<CancellationToken>()))
               .Returns(Task.FromResult(new BenefitsEnrollment { Email = "test@test.com" }));

            _mockEmployerService.Setup(x => x.GetEmployersAsync(It.IsAny<RetrieveEmployersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<RetrieveEmployerResponse>>([new() { Id = "1" }]));

            _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }
    }
}
