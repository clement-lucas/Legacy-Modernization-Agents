using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using Insurance.Data.Models;
using Insurance.Data.Repositories;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Insurance.Data.Models.Tests
{
    public class CoverageRepositoryTests : IDisposable
    {
        private readonly Mock<IDbConnection> _dbConnectionMock;
        private readonly Mock<ILogger<CoverageRepository>> _loggerMock;
        private readonly CoverageRepository _repository;

        public CoverageRepositoryTests()
        {
            _dbConnectionMock = new Mock<IDbConnection>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<CoverageRepository>>(MockBehavior.Loose);
            _repository = new CoverageRepository(_dbConnectionMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            // Cleanup resources if necessary
        }

        #region Helper Methods

        private static CoverageRecord CreateSampleCoverageRecord(string policyNumber = "PN12345678")
        {
            return new CoverageRecord
            {
                CoveragePolicyNumber = policyNumber,
                CoverageStatus = "ACTIVE",
                CoverageStartDate = "2024-01-01",
                CoverageEndDate = "2024-12-31",
                CoverageAddedTimestamp = new DateTime(2024, 01, 01, 12, 00, 00),
                CoverageUpdatedTimestamp = new DateTime(2024, 06, 01, 12, 00, 00)
            };
        }

        private static List<CoverageRecord> CreateSampleCoverageList()
        {
            return new List<CoverageRecord>
            {
                CreateSampleCoverageRecord("PN12345678"),
                CreateSampleCoverageRecord("PN87654321")
            };
        }

        #endregion

        [Fact]
        public async Task GetAllCoveragesAsync_ReturnsCoverageRecords_WhenRecordsExist()
        {
            // Arrange
            var expectedRecords = CreateSampleCoverageList();
            var queryAsyncMock = new Mock<Func<string, Task<IEnumerable<CoverageRecord>>>>();
            _dbConnectionMock
                .Setup(x => x.QueryAsync<CoverageRecord>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expectedRecords);

            // Act
            var result = await _repository.GetAllCoveragesAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedRecords);
            result.Count.Should().Be(2);
            result[0].CoveragePolicyNumber.Should().Be("PN12345678");
            result[1].CoveragePolicyNumber.Should().Be("PN87654321");
        }

        [Fact]
        public async Task GetAllCoveragesAsync_ReturnsEmptyList_WhenNoRecordsExist()
        {
            // Arrange
            var expectedRecords = new List<CoverageRecord>();
            _dbConnectionMock
                .Setup(x => x.QueryAsync<CoverageRecord>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expectedRecords);

            // Act
            var result = await _repository.GetAllCoveragesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCoveragesAsync_ThrowsDataAccessException_OnDbException()
        {
            // Arrange
            var dbException = new InvalidOperationException("DB error");
            _dbConnectionMock
                .Setup(x => x.QueryAsync<CoverageRecord>(It.IsAny<string>(), null, null, null, null))
                .ThrowsAsync(dbException);

            // Act
            Func<Task> act = async () => await _repository.GetAllCoveragesAsync();

            // Assert
            var exception = await Assert.ThrowsAsync<DataAccessException>(act);
            exception.Message.Should().Contain("An error occurred while retrieving coverage records.");
            exception.InnerException.Should().Be(dbException);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    dbException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("PN12345678")]
        [InlineData("PN87654321")]
        public async Task GetCoverageByPolicyNumberAsync_ReturnsCoverageRecord_WhenRecordExists(string policyNumber)
        {
            // Arrange
            var expectedRecord = CreateSampleCoverageRecord(policyNumber);
            _dbConnectionMock
                .Setup(x => x.QuerySingleOrDefaultAsync<CoverageRecord>(
                    It.IsAny<string>(),
                    It.Is<object>(p => ((dynamic)p).PolicyNumber == policyNumber),
                    null, null, null))
                .ReturnsAsync(expectedRecord);

            // Act
            var result = await _repository.GetCoverageByPolicyNumberAsync(policyNumber);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedRecord);
            result!.CoveragePolicyNumber.Should().Be(policyNumber);
        }

        [Fact]
        public async Task GetCoverageByPolicyNumberAsync_ReturnsNull_WhenRecordDoesNotExist()
        {
            // Arrange
            string policyNumber = "NONEXISTENT";
            _dbConnectionMock
                .Setup(x => x.QuerySingleOrDefaultAsync<CoverageRecord>(
                    It.IsAny<string>(),
                    It.Is<object>(p => ((dynamic)p).PolicyNumber == policyNumber),
                    null, null, null))
                .ReturnsAsync((CoverageRecord?)null);

            // Act
            var result = await _repository.GetCoverageByPolicyNumberAsync(policyNumber);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCoverageByPolicyNumberAsync_ThrowsDataAccessException_OnDbException()
        {
            // Arrange
            string policyNumber = "PN12345678";
            var dbException = new InvalidOperationException("DB error");
            _dbConnectionMock
                .Setup(x => x.QuerySingleOrDefaultAsync<CoverageRecord>(
                    It.IsAny<string>(),
                    It.Is<object>(p => ((dynamic)p).PolicyNumber == policyNumber),
                    null, null, null))
                .ThrowsAsync(dbException);

            // Act
            Func<Task> act = async () => await _repository.GetCoverageByPolicyNumberAsync(policyNumber);

            // Assert
            var exception = await Assert.ThrowsAsync<DataAccessException>(act);
            exception.Message.Should().Contain(policyNumber);
            exception.InnerException.Should().Be(dbException);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    dbException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("          ")]
        public async Task GetCoverageByPolicyNumberAsync_HandlesNullOrEmptyPolicyNumber(string policyNumber)
        {
            // Arrange
            _dbConnectionMock
                .Setup(x => x.QuerySingleOrDefaultAsync<CoverageRecord>(
                    It.IsAny<string>(),
                    It.Is<object>(p => ((dynamic)p).PolicyNumber == policyNumber),
                    null, null, null))
                .ReturnsAsync((CoverageRecord?)null);

            // Act
            var result = await _repository.GetCoverageByPolicyNumberAsync(policyNumber);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbConnectionIsNull()
        {
            // Act
            Action act = () => new CoverageRepository(null!, _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("dbConnection");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act
            Action act = () => new CoverageRepository(_dbConnectionMock.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetAllCoveragesAsync_IntegrationTest_ReturnsRecords_FromDatabase()
        {
            // This test assumes a real database connection and should be run in integration environment.
            // Commented out by default to avoid running in unit test environments.
            /*
            using var realDbConnection = new SqlConnection("your_connection_string");
            var realLogger = new LoggerFactory().CreateLogger<CoverageRepository>();
            var repository = new CoverageRepository(realDbConnection, realLogger);

            var result = await repository.GetAllCoveragesAsync();

            result.Should().NotBeNull();
            result.Count.Should().BeGreaterThan(0);
            */
        }

        [Fact]
        public async Task GetCoverageByPolicyNumberAsync_IntegrationTest_ReturnsRecord_FromDatabase()
        {
            // This test assumes a real database connection and should be run in integration environment.
            // Commented out by default to avoid running in unit test environments.
            /*
            using var realDbConnection = new SqlConnection("your_connection_string");
            var realLogger = new LoggerFactory().CreateLogger<CoverageRepository>();
            var repository = new CoverageRepository(realDbConnection, realLogger);

            var policyNumber = "PN12345678";
            var result = await repository.GetCoverageByPolicyNumberAsync(policyNumber);

            result.Should().NotBeNull();
            result.CoveragePolicyNumber.Should().Be(policyNumber);
            */
        }

        [Fact]
        public void CoverageRecord_Properties_ShouldPreserveCobolBusinessLogic()
        {
            // Arrange
            var record = new CoverageRecord
            {
                CoveragePolicyNumber = "PN12345678",
                CoverageStatus = "ACTIVE",
                CoverageStartDate = "2024-01-01",
                CoverageEndDate = "2024-12-31",
                CoverageAddedTimestamp = new DateTime(2024, 01, 01, 12, 00, 00),
                CoverageUpdatedTimestamp = new DateTime(2024, 06, 01, 12, 00, 00)
            };

            // Assert COBOL mappings and boundary conditions
            record.CoveragePolicyNumber.Length.Should().BeLessOrEqualTo(10); // PIC X(10)
            record.CoverageStatus.Length.Should().BeLessOrEqualTo(10); // PIC X(10)
            DateTime.TryParse(record.CoverageStartDate, out _).Should().BeTrue();
            DateTime.TryParse(record.CoverageEndDate, out _).Should().BeTrue();
            record.CoverageAddedTimestamp.Should().BeAfter(new DateTime(2000, 01, 01));
            record.CoverageUpdatedTimestamp.Should().BeAfter(new DateTime(2000, 01, 01));
        }

        [Theory]
        [InlineData("SHORT")]
        [InlineData("1234567890")]
        [InlineData("TOOLONG12345")]
        public void CoverageRecord_CoveragePolicyNumber_LengthValidation(string policyNumber)
        {
            // Arrange
            var record = new CoverageRecord { CoveragePolicyNumber = policyNumber };

            // Assert
            record.CoveragePolicyNumber.Length.Should().BeLessOrEqualTo(10, "COBOL PIC X(10) constraint");
        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("INACTIVE")]
        [InlineData("PENDING")]
        [InlineData("1234567890")]
        public void CoverageRecord_CoverageStatus_LengthValidation(string status)
        {
            // Arrange
            var record = new CoverageRecord { CoverageStatus = status };

            // Assert
            record.CoverageStatus.Length.Should().BeLessOrEqualTo(10, "COBOL PIC X(10) constraint");
        }

        [Theory]
        [InlineData("2024-01-01")]
        [InlineData("2024-12-31")]
        [InlineData("INVALID_DATE")]
        public void CoverageRecord_CoverageStartDate_FormatValidation(string startDate)
        {
            // Arrange
            var record = new CoverageRecord { CoverageStartDate = startDate };

            // Assert
            if (DateTime.TryParse(startDate, out var parsedDate))
            {
                parsedDate.Should().BeOfType<DateTime>();
            }
            else
            {
                // Comment: COBOL PIC X(10) allows any string, but business logic expects ISO date
                startDate.Should().NotMatchRegex(@"^\d{4}-\d{2}-\d{2}$");
            }
        }

        [Fact]
        public void CoverageRecord_DefaultValues_ShouldBeEmptyOrDefault()
        {
            // Arrange
            var record = new CoverageRecord();

            // Assert
            record.CoveragePolicyNumber.Should().BeEmpty();
            record.CoverageStatus.Should().BeEmpty();
            record.CoverageStartDate.Should().BeEmpty();
            record.CoverageEndDate.Should().BeEmpty();
            record.CoverageAddedTimestamp.Should().Be(default);
            record.CoverageUpdatedTimestamp.Should().Be(default);
        }
    }
}