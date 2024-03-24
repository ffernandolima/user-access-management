using Moq;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UserAccessManagement.Application.Commands;
using Xunit;

namespace UserAccessManagement.UnitTests.Application.Commands
{
    public class RequestBenefitsEnrollmentCommandHandlerTests
    {
        private readonly Mock<ChannelWriter<RequestBenefitsEnrollmentCommand>> _mockProducer;
        private readonly RequestBenefitsEnrollmentCommandHandler _commandHandler;

        public RequestBenefitsEnrollmentCommandHandlerTests()
        {
            _mockProducer = new Mock<ChannelWriter<RequestBenefitsEnrollmentCommand>>();
            _commandHandler = new RequestBenefitsEnrollmentCommandHandler(_mockProducer.Object);
        }

        [Fact]
        public void Ctor_Throws_Exception_When_Producer_Service_Is_NuLL()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new RequestBenefitsEnrollmentCommandHandler(null));
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Command_Is_Null()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _commandHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Writes_Item_And_Returns_True()
        {
            // Arrange
            var command = new RequestBenefitsEnrollmentCommand
            {
                File = "/blobstorage/employer1.csv",
                EmployerName = "Employer 1"
            };

            _mockProducer.Setup(x => x.WriteAsync(It.IsAny<RequestBenefitsEnrollmentCommand>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
        }
    }
}
