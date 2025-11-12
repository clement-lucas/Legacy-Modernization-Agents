using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace AgentFileDriver.Tests
{
    public class AgentFileInputTests : IDisposable
    {
        private readonly Mock<IAgentFileRepository> _repositoryMock;
        private readonly Mock<ILogger<AgentFileDriver>> _loggerMock;
        private readonly AgentFileDriver _driver;

        public AgentFileInputTests()
        {
            _repositoryMock = new Mock<IAgentFileRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<AgentFileDriver>>();
            _driver = new AgentFileDriver(_repositoryMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public async Task ExecuteAsync_OpenOperation_ShouldReturnStatusCode00_WhenOpenSucceeds()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "OPEN"
            };
            _repositoryMock.Setup(r => r.OpenAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("00");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.OpenAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_OpenOperation_ShouldReturnStatusCode23_WhenFileNotFound()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "OPEN"
            };
            _repositoryMock.Setup(r => r.OpenAsync())
                .ThrowsAsync(new AgentFileException("Agent file not found.", "23"));

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("23");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.OpenAsync(), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_CloseOperation_ShouldReturnStatusCode00_WhenCloseSucceeds()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "CLOSE"
            };
            _repositoryMock.Setup(r => r.CloseAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("00");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_CloseOperation_ShouldReturnStatusCode99_WhenExceptionThrown()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "CLOSE"
            };
            _repositoryMock.Setup(r => r.CloseAsync())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("99");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.CloseAsync(), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldReturnStatusCode00_AndAgentRecord_WhenFound()
        {
            // Arrange
            var agentCode = "A123456789";
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = agentCode
            };
            var expectedRecord = new AgentRecord(
                agentCode, "John Doe", "Addr1", "Addr2", "City", "ST", "12345", "Active", "Type1", "john@doe.com", "555-1234", "20220101", "20221231"
            );
            _repositoryMock.Setup(r => r.SearchAsync(agentCode)).ReturnsAsync(expectedRecord);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("00");
            result.AgentRecord.Should().BeEquivalentTo(expectedRecord);
            _repositoryMock.Verify(r => r.SearchAsync(agentCode), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldReturnStatusCode23_WhenAgentNotFound()
        {
            // Arrange
            var agentCode = "NOTFOUND";
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = agentCode
            };
            _repositoryMock.Setup(r => r.SearchAsync(agentCode)).ReturnsAsync((AgentRecord?)null);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("23");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.SearchAsync(agentCode), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldReturnStatusCode99_WhenRepositoryThrowsException()
        {
            // Arrange
            var agentCode = "EXCEPTION";
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = agentCode
            };
            _repositoryMock.Setup(r => r.SearchAsync(agentCode)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("99");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.SearchAsync(agentCode), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("INVALID")]
        public async Task ExecuteAsync_InvalidOperationType_ShouldReturnStatusCode99(string? operationType)
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = operationType
            };

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("99");
            result.AgentRecord.Should().BeNull();
            _loggerMock.VerifyLog(LogLevel.Warning, "Invalid operation type: {OperationType}", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldHandleNullAgentCode()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = null
            };
            _repositoryMock.Setup(r => r.SearchAsync(null)).ReturnsAsync((AgentRecord?)null);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("23");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.SearchAsync(null), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldHandleEmptyAgentCode()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = ""
            };
            _repositoryMock.Setup(r => r.SearchAsync("")).ReturnsAsync((AgentRecord?)null);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("23");
            result.AgentRecord.Should().BeNull();
            _repositoryMock.Verify(r => r.SearchAsync(""), Times.Once);
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_SearchOperation_ShouldPreserveCOBOLBusinessLogic_Status23OnNotFound()
        {
            // Arrange
            var agentCode = "UNKNOWN";
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = agentCode
            };
            _repositoryMock.Setup(r => r.SearchAsync(agentCode)).ReturnsAsync((AgentRecord?)null);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            // COBOL: "23" means not found
            result.StatusCode.Should().Be("23");
            result.AgentRecord.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldBeCaseInsensitiveOnOperationType()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "open"
            };
            _repositoryMock.Setup(r => r.OpenAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("00");
            _repositoryMock.Verify(r => r.OpenAsync(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldTrimOperationType()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "  SEARCH  ",
                AgentCode = "A123"
            };
            var expectedRecord = new AgentRecord(
                "A123", "Jane", "Addr1", "Addr2", "City", "ST", "54321", "Active", "Type2", "jane@doe.com", "555-4321", "20220101", "20221231"
            );
            _repositoryMock.Setup(r => r.SearchAsync("A123")).ReturnsAsync(expectedRecord);

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("00");
            result.AgentRecord.Should().BeEquivalentTo(expectedRecord);
            _repositoryMock.Verify(r => r.SearchAsync("A123"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogErrorAndSetStatusCode99_OnUnhandledException()
        {
            // Arrange
            var input = new AgentFileInput
            {
                OperationType = "OPEN"
            };
            _repositoryMock.Setup(r => r.OpenAsync()).ThrowsAsync(new Exception("Unhandled"));

            // Act
            var result = await _driver.ExecuteAsync(input);

            // Assert
            result.StatusCode.Should().Be("99");
            _loggerMock.VerifyLog(LogLevel.Error, "IN FLDRIVR1", Times.Once());
        }

        // Helper extension for verifying logger calls with message
        // This is needed because ILogger is hard to verify directly
    }

    internal static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, Times? times = null)
        {
            times ??= Times.AtLeastOnce();
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times.Value);
        }
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string format, Times? times = null)
        {
            times ??= Times.AtLeastOnce();
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(format)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                times.Value);
        }
    }
}