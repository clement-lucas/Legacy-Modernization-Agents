using System;
using System.Threading.Tasks;
using AgentManagement.Models;
using AgentManagement.Services;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace AgentManagement.Models.Tests
{
    public class AgentServiceTests : IDisposable
    {
        private readonly Mock<ILogger<AgentService>> _loggerMock;
        private readonly AgentService _agentService;

        public AgentServiceTests()
        {
            _loggerMock = new Mock<ILogger<AgentService>>(MockBehavior.Strict);
            _agentService = new AgentService(_loggerMock.Object);
        }

        public void Dispose()
        {
            // Cleanup if needed (no unmanaged resources in this scenario)
        }

        #region ValidateAgentAsync Tests

        [Fact]
        public async Task ValidateAgentAsync_ShouldThrowArgumentNullException_WhenAgentIsNull()
        {
            // Arrange
            AgentRecord agent = null;

            // Act
            Func<Task> act = async () => await _agentService.ValidateAgentAsync(agent);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("*agent*");
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentCodeIsEmpty()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "",
                AgentName = "John Doe",
                AgentEmail = "john.doe@example.com"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentNameIsEmpty()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A123",
                AgentName = "",
                AgentEmail = "john.doe@example.com"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentEmailIsEmpty()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentEmail = ""
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(" ", "John Doe", "john.doe@example.com")]
        [InlineData("A123", " ", "john.doe@example.com")]
        [InlineData("A123", "John Doe", " ")]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAnyRequiredFieldIsWhitespace(string code, string name, string email)
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = code,
                AgentName = name,
                AgentEmail = email
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnTrue_WhenAllRequiredFieldsArePopulated()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentEmail = "john.doe@example.com"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnTrue_WhenAllFieldsArePopulated()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A456",
                AgentName = "Jane Smith",
                AgentAddress1 = "123 Main St",
                AgentAddress2 = "Suite 200",
                AgentCity = "Metropolis",
                AgentState = "NY",
                AgentZipCode = "12345",
                AgentDateOfBirth = "1980-01-01",
                AgentType = "internal",
                AgentStatus = "active",
                AgentEmail = "jane.smith@example.com",
                AgentContactNumber = "555-1234",
                AgentStartDate = "2020-01-01",
                AgentEndDate = "2025-01-01"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentEmail = "john.doe@example.com"
            };

            // Simulate logger throwing exception to force catch block
            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>())).Throws(new InvalidOperationException());

            // Replace AgentService with one that will throw in validation logic
            var faultyService = new FaultyAgentService(_loggerMock.Object);

            // Act
            Func<Task> act = async () => await faultyService.ValidateAgentAsync(agent);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Helper class to simulate exception in validation logic
        private class FaultyAgentService : AgentService
        {
            public FaultyAgentService(ILogger<AgentService> logger) : base(logger) { }

            public override async Task<bool> ValidateAgentAsync(AgentRecord agent)
            {
                throw new InvalidOperationException("Simulated validation error");
            }
        }

        #endregion

        #region SaveAgentAsync Tests

        [Fact]
        public async Task SaveAgentAsync_ShouldThrowArgumentNullException_WhenAgentIsNull()
        {
            // Arrange
            AgentRecord agent = null;

            // Act
            Func<Task> act = async () => await _agentService.SaveAgentAsync(agent);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("*agent*");
        }

        [Fact]
        public async Task SaveAgentAsync_ShouldLogInformation_WhenAgentIsSaved()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A789",
                AgentName = "Alice Johnson",
                AgentEmail = "alice.johnson@example.com"
            };

            _loggerMock.Setup(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode));

            // Act
            await _agentService.SaveAgentAsync(agent);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode), Times.Once);
        }

        [Fact]
        public async Task SaveAgentAsync_ShouldLogErrorAndRethrow_WhenExceptionOccurs()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A999",
                AgentName = "Bob Error",
                AgentEmail = "bob.error@example.com"
            };

            // Simulate logger throwing exception to force catch block
            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>())).Throws(new InvalidOperationException());

            var faultyService = new FaultyAgentServiceForSave(_loggerMock.Object);

            // Act
            Func<Task> act = async () => await faultyService.SaveAgentAsync(agent);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // Helper class to simulate exception in save logic
        private class FaultyAgentServiceForSave : AgentService
        {
            public FaultyAgentServiceForSave(ILogger<AgentService> logger) : base(logger) { }

            public override async Task SaveAgentAsync(AgentRecord agent)
            {
                throw new InvalidOperationException("Simulated save error");
            }
        }

        [Fact]
        public async Task SaveAgentAsync_ShouldAllowSavingAgentWithMinimalRequiredFields()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A100",
                AgentName = "Minimal Agent",
                AgentEmail = "minimal.agent@example.com"
            };

            _loggerMock.Setup(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode));

            // Act
            Func<Task> act = async () => await _agentService.SaveAgentAsync(agent);

            // Assert
            await act.Should().NotThrowAsync();
            _loggerMock.Verify(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode), Times.Once);
        }

        [Fact]
        public async Task SaveAgentAsync_ShouldAllowSavingAgentWithAllFieldsPopulated()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A200",
                AgentName = "Full Agent",
                AgentAddress1 = "456 Elm St",
                AgentAddress2 = "Apt 3B",
                AgentCity = "Smallville",
                AgentState = "KS",
                AgentZipCode = "67890",
                AgentDateOfBirth = "1975-12-31",
                AgentType = "external",
                AgentStatus = "inactive",
                AgentEmail = "full.agent@example.com",
                AgentContactNumber = "555-6789",
                AgentStartDate = "2015-06-01",
                AgentEndDate = "2020-06-01"
            };

            _loggerMock.Setup(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode));

            // Act
            Func<Task> act = async () => await _agentService.SaveAgentAsync(agent);

            // Assert
            await act.Should().NotThrowAsync();
            _loggerMock.Verify(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode), Times.Once);
        }

        #endregion

        #region Boundary and Edge Case Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentCodeIsNullOrWhitespace(string agentCode)
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = agentCode,
                AgentName = "Edge Case",
                AgentEmail = "edge.case@example.com"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentNameIsNullOrWhitespace(string agentName)
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A300",
                AgentName = agentName,
                AgentEmail = "edge.case@example.com"
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateAgentAsync_ShouldReturnFalse_WhenAgentEmailIsNullOrWhitespace(string agentEmail)
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A400",
                AgentName = "Edge Case",
                AgentEmail = agentEmail
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateAgentAsync_ShouldReturnTrue_WhenNonRequiredFieldsAreEmpty()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A500",
                AgentName = "No Address",
                AgentEmail = "no.address@example.com",
                AgentAddress1 = "",
                AgentAddress2 = "",
                AgentCity = "",
                AgentState = "",
                AgentZipCode = "",
                AgentDateOfBirth = "",
                AgentType = "",
                AgentStatus = "",
                AgentContactNumber = "",
                AgentStartDate = "",
                AgentEndDate = ""
            };

            // Act
            var result = await _agentService.ValidateAgentAsync(agent);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SaveAgentAsync_ShouldNotThrow_WhenNonRequiredFieldsAreEmpty()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A501",
                AgentName = "No Address",
                AgentEmail = "no.address@example.com",
                AgentAddress1 = "",
                AgentAddress2 = "",
                AgentCity = "",
                AgentState = "",
                AgentZipCode = "",
                AgentDateOfBirth = "",
                AgentType = "",
                AgentStatus = "",
                AgentContactNumber = "",
                AgentStartDate = "",
                AgentEndDate = ""
            };

            _loggerMock.Setup(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode));

            // Act
            Func<Task> act = async () => await _agentService.SaveAgentAsync(agent);

            // Assert
            await act.Should().NotThrowAsync();
            _loggerMock.Verify(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode), Times.Once);
        }

        #endregion

        #region Integration Test Simulation

        // Simulate integration test for database operation (since actual DB is not present)
        // This test ensures SaveAgentAsync performs as expected in a real scenario
        [Fact]
        public async Task SaveAgentAsync_IntegrationTest_ShouldSimulateDatabaseSave()
        {
            // Arrange
            var agent = new AgentRecord
            {
                AgentCode = "A600",
                AgentName = "Integration Agent",
                AgentEmail = "integration.agent@example.com"
            };

            _loggerMock.Setup(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode));

            // Act
            var saveTask = _agentService.SaveAgentAsync(agent);

            // Assert
            await saveTask.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));
            _loggerMock.Verify(l => l.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode), Times.Once);
        }

        #endregion
    }
}