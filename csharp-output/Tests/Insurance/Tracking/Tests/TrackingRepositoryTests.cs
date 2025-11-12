using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insurance.Tracking;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Insurance.Tracking.Tests
{
    public class TrackingRepositoryTests : IDisposable
    {
        private readonly Mock<DbConnection> _mockConnection;
        private readonly Mock<DbCommand> _mockCommand;
        private readonly Mock<DbDataReader> _mockReader;
        private readonly TrackingRepository _repository;

        public TrackingRepositoryTests()
        {
            _mockConnection = new Mock<DbConnection>();
            _mockCommand = new Mock<DbCommand>();
            _mockReader = new Mock<DbDataReader>();

            // Setup CreateCommand to return our mock command
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            _repository = new TrackingRepository(_mockConnection.Object);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public async Task GetTrackingRecordAsync_ShouldReturnRecord_WhenRecordExists()
        {
            // Arrange
            var policyNumber = "PN123";
            var expectedRecord = new TrackingRecord(
                PolicyNumber: policyNumber,
                NotifyDate: "20240601",
                Status: "A",
                AddTimestamp: new DateTime(2024, 6, 1, 10, 0, 0),
                UpdateTimestamp: new DateTime(2024, 6, 1, 12, 0, 0)
            );

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);

            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockCommand.SetupProperty(c => c.CommandText);
            _mockCommand.SetupProperty(c => c.CommandType);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockReader.Object);

            // Setup reader to return one record
            var readCount = 0;
            _mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockReader.Setup(r => r.GetString(0)).Returns(expectedRecord.PolicyNumber);
            _mockReader.Setup(r => r.GetString(1)).Returns(expectedRecord.NotifyDate);
            _mockReader.Setup(r => r.GetString(2)).Returns(expectedRecord.Status);
            _mockReader.Setup(r => r.GetDateTime(3)).Returns(expectedRecord.AddTimestamp);
            _mockReader.Setup(r => r.GetDateTime(4)).Returns(expectedRecord.UpdateTimestamp);

            // Act
            var result = await _repository.GetTrackingRecordAsync(policyNumber);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedRecord);
        }

        [Fact]
        public async Task GetTrackingRecordAsync_ShouldReturnNull_WhenNoRecordExists()
        {
            // Arrange
            var policyNumber = "PN999";
            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockReader.Object);

            _mockReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.GetTrackingRecordAsync(policyNumber);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task InsertTrackingRecordAsync_ShouldReturnZero_WhenInsertSucceeds()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _repository.InsertTrackingRecordAsync(record);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task InsertTrackingRecordAsync_ShouldReturnMinusOne_WhenInsertFails()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            // Act
            var result = await _repository.InsertTrackingRecordAsync(record);

            // Assert
            result.Should().Be(-1);
        }

        [Fact]
        public async Task InsertTrackingRecordAsync_ShouldReturnDbExceptionErrorCode_WhenDbExceptionThrown()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var dbEx = new TestDbException(12345);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbEx);

            // Act
            var result = await _repository.InsertTrackingRecordAsync(record);

            // Assert
            result.Should().Be(12345);
        }

        [Fact]
        public async Task UpdateTrackingRecordAsync_ShouldReturnZero_WhenUpdateSucceeds()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _repository.UpdateTrackingRecordAsync(record);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateTrackingRecordAsync_ShouldReturnMinusOne_WhenUpdateFails()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            // Act
            var result = await _repository.UpdateTrackingRecordAsync(record);

            // Assert
            result.Should().Be(-1);
        }

        [Fact]
        public async Task UpdateTrackingRecordAsync_ShouldReturnDbExceptionErrorCode_WhenDbExceptionThrown()
        {
            // Arrange
            var record = new TrackingRecord("PN123", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var dbEx = new TestDbException(54321);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbEx);

            // Act
            var result = await _repository.UpdateTrackingRecordAsync(record);

            // Assert
            result.Should().Be(54321);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConnectionIsNull()
        {
            // Arrange
            DbConnection nullConnection = null;

            // Act
            Action act = () => new TrackingRepository(nullConnection);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetTrackingRecordAsync_ShouldHandleNullOrEmptyPolicyNumber(string policyNumber)
        {
            // Arrange
            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockReader.Object);

            _mockReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.GetTrackingRecordAsync(policyNumber);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task InsertTrackingRecordAsync_ShouldHandleNullFields()
        {
            // Arrange
            var record = new TrackingRecord(null, null, null, DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _repository.InsertTrackingRecordAsync(record);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateTrackingRecordAsync_ShouldHandleNullFields()
        {
            // Arrange
            var record = new TrackingRecord(null, null, null, DateTime.UtcNow, DateTime.UtcNow);

            var parameters = new Mock<DbParameterCollection>();
            _mockCommand.Setup(c => c.Parameters).Returns(parameters.Object);
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<DbParameter>().Object);

            _mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _repository.UpdateTrackingRecordAsync(record);

            // Assert
            result.Should().Be(0);
        }

        // Integration test for database operation (requires a real DbConnection, skipped in CI)
        [Fact(Skip = "Integration test - requires real database")]
        public async Task InsertAndGetTrackingRecord_IntegrationTest()
        {
            // Arrange
            // Replace with actual DbConnection for integration test
            using var connection = /* new SqlConnection("your-connection-string") */;
            var repository = new TrackingRepository(connection);

            var record = new TrackingRecord("PN999", "20240601", "A", DateTime.UtcNow, DateTime.UtcNow);

            // Act
            var insertResult = await repository.InsertTrackingRecordAsync(record);
            var fetchedRecord = await repository.GetTrackingRecordAsync("PN999");

            // Assert
            insertResult.Should().Be(0);
            fetchedRecord.Should().NotBeNull();
            fetchedRecord.PolicyNumber.Should().Be("PN999");
        }

        // Helper class to simulate DbException with ErrorCode
        private class TestDbException : DbException
        {
            public override int ErrorCode { get; }
            public TestDbException(int errorCode)
            {
                ErrorCode = errorCode;
            }
        }
    }
}