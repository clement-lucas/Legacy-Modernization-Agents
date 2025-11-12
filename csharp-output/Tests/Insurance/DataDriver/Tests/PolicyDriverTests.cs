using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Insurance.DataDriver;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Insurance.DataDriver.Tests
{
    public class PolicyDriverTests : IDisposable
    {
        private readonly Mock<DbConnection> _dbConnectionMock;
        private readonly Mock<ILogger<PolicyDriver>> _loggerMock;
        private readonly Mock<DbCommand> _dbCommandMock;
        private readonly Mock<DbDataReader> _dbDataReaderMock;
        private readonly PolicyDriver _policyDriver;

        public PolicyDriverTests()
        {
            _dbConnectionMock = new Mock<DbConnection>();
            _loggerMock = new Mock<ILogger<PolicyDriver>>();
            _dbCommandMock = new Mock<DbCommand>();
            _dbDataReaderMock = new Mock<DbDataReader>();

            _dbConnectionMock.Setup(c => c.CreateCommand()).Returns(_dbCommandMock.Object);

            _policyDriver = new PolicyDriver(_dbConnectionMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            PolicyDriverRequest request = null;

            // Act
            Func<Task> act = async () => await _policyDriver.ExecuteAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("INVALID")]
        public async Task ExecuteAsync_ShouldReturnMinusOneSqlCode_WhenOperationTypeIsInvalid(string operationType)
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = operationType, ProcessDate = "2024-06-01" };

            // Act
            var response = await _policyDriver.ExecuteAsync(request);

            // Assert
            response.SqlCode.Should().Be(-1);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid operation type")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnMinus3SqlCode_WhenOpenWithInvalidProcessDate()
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "not-a-date" };

            // Act
            var response = await _policyDriver.ExecuteAsync(request);

            // Assert
            response.SqlCode.Should().Be(-3);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid process date format")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZeroSqlCode_WhenOpenWithValidProcessDate()
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _policyDriver.ExecuteAsync(request);

            // Assert
            response.SqlCode.Should().Be(0);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZeroSqlCode_WhenOpenIsCalledTwice()
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response1 = await _policyDriver.ExecuteAsync(request);
            var response2 = await _policyDriver.ExecuteAsync(request);

            // Assert
            response1.SqlCode.Should().Be(0);
            response2.SqlCode.Should().Be(0);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Policy cursor is already open")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnMinus5SqlCode_WhenFetchWithoutOpen()
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = "FETCH", ProcessDate = "2024-06-01" };

            // Act
            var response = await _policyDriver.ExecuteAsync(request);

            // Assert
            response.SqlCode.Should().Be(-5);
            response.PolicyData.Should().BeNull();
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Policy cursor is not open")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZeroSqlCodeAndPolicyData_WhenFetchReturnsRow()
        {
            // Arrange
            var openRequest = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };
            var fetchRequest = new PolicyDriverRequest { OperationType = "FETCH", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup reader to return one row
            _dbDataReaderMock.SetupSequence(r => r.ReadAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Setup reader for MapPolicyRecord
            _dbDataReaderMock.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns((string name) =>
            {
                return name switch
                {
                    "POLICY_NUMBER" => 0,
                    "POLICY_HOLDER_FNAME" => 1,
                    "POLICY_HOLDER_MNAME" => 2,
                    "POLICY_HOLDER_LNAME" => 3,
                    "POLICY_BENEF_NAME" => 4,
                    "POLICY_BENEF_RELATION" => 5,
                    "POLICY_HOLDER_ADDR_1" => 6,
                    "POLICY_HOLDER_ADDR_2" => 7,
                    "POLICY_HOLDER_CITY" => 8,
                    "POLICY_HOLDER_STATE" => 9,
                    "POLICY_HOLDER_ZIP_CD" => 10,
                    "POLICY_HOLDER_DOB" => 11,
                    "POLICY_HOLDER_GENDER" => 12,
                    "POLICY_HOLDER_PHONE" => 13,
                    "POLICY_HOLDER_EMAIL" => 14,
                    "POLICY_PAYMENT_FREQUENCY" => 15,
                    "POLICY_PAYMENT_METHOD" => 16,
                    "POLICY_UNDERWRITER" => 17,
                    "POLICY_TERMS_CONDITIONS" => 18,
                    "POLICY_CLAIMED" => 19,
                    "POLICY_DISCOUNT_CODE" => 20,
                    "POLICY_PREMIUM_AMOUNT" => 21,
                    "POLICY_COVERAGE_AMOUNT" => 22,
                    "POLICY_TYPE" => 23,
                    "POLICY_START_DATE" => 24,
                    "POLICY_EXPIRY_DATE" => 25,
                    "POLICY_STATUS" => 26,
                    "POLICY_AGENT_CODE" => 27,
                    "POLICY_NOTIFY_FLAG" => 28,
                    "POLICY_ADD_TIMESTAMP" => 29,
                    "POLICY_UPDATE_TIMESTAMP" => 30,
                    _ => -1
                };
            });

            _dbDataReaderMock.Setup(r => r.GetString(0)).Returns("PN123");
            _dbDataReaderMock.Setup(r => r.GetString(1)).Returns("John");
            _dbDataReaderMock.Setup(r => r.GetString(2)).Returns("M");
            _dbDataReaderMock.Setup(r => r.GetString(3)).Returns("Doe");
            _dbDataReaderMock.Setup(r => r.GetString(4)).Returns("Jane Doe");
            _dbDataReaderMock.Setup(r => r.GetString(5)).Returns("Spouse");
            _dbDataReaderMock.Setup(r => r.GetString(6)).Returns("123 Main St");
            _dbDataReaderMock.Setup(r => r.GetString(7)).Returns("Apt 4");
            _dbDataReaderMock.Setup(r => r.GetString(8)).Returns("Los Angeles");
            _dbDataReaderMock.Setup(r => r.GetString(9)).Returns("CA");
            _dbDataReaderMock.Setup(r => r.GetString(10)).Returns("90001");
            _dbDataReaderMock.Setup(r => r.GetDateTime(11)).Returns(new DateTime(1980, 1, 1));
            _dbDataReaderMock.Setup(r => r.GetString(12)).Returns("M");
            _dbDataReaderMock.Setup(r => r.GetString(13)).Returns("555-1234");
            _dbDataReaderMock.Setup(r => r.GetString(14)).Returns("john.doe@email.com");
            _dbDataReaderMock.Setup(r => r.GetString(15)).Returns("Monthly");
            _dbDataReaderMock.Setup(r => r.GetString(16)).Returns("CreditCard");
            _dbDataReaderMock.Setup(r => r.GetString(17)).Returns("BestInsure");
            _dbDataReaderMock.Setup(r => r.GetString(18)).Returns("Standard Terms");
            _dbDataReaderMock.Setup(r => r.GetBoolean(19)).Returns(false);
            _dbDataReaderMock.Setup(r => r.GetString(20)).Returns("DISC10");
            _dbDataReaderMock.Setup(r => r.GetDecimal(21)).Returns(100.50m);
            _dbDataReaderMock.Setup(r => r.GetDecimal(22)).Returns(10000.00m);
            _dbDataReaderMock.Setup(r => r.GetString(23)).Returns("HEALTH");
            _dbDataReaderMock.Setup(r => r.GetDateTime(24)).Returns(new DateTime(2024, 6, 1));
            _dbDataReaderMock.Setup(r => r.GetDateTime(25)).Returns(new DateTime(2025, 6, 1));
            _dbDataReaderMock.Setup(r => r.GetString(26)).Returns("A");
            _dbDataReaderMock.Setup(r => r.GetString(27)).Returns("AGT123");
            _dbDataReaderMock.Setup(r => r.GetBoolean(28)).Returns(true);
            _dbDataReaderMock.Setup(r => r.GetDateTime(29)).Returns(new DateTime(2024, 5, 1));
            _dbDataReaderMock.Setup(r => r.GetDateTime(30)).Returns(new DateTime(2024, 6, 1));

            // Act
            await _policyDriver.ExecuteAsync(openRequest);
            var response = await _policyDriver.ExecuteAsync(fetchRequest);

            // Assert
            response.SqlCode.Should().Be(0);
            response.PolicyData.Should().NotBeNull();
            response.PolicyData.Should().Contain("PN123");
            response.PolicyData.Should().Contain("John");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturn100SqlCode_WhenFetchReturnsNoRows()
        {
            // Arrange
            var openRequest = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };
            var fetchRequest = new PolicyDriverRequest { OperationType = "FETCH", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup reader to return no rows
            _dbDataReaderMock.Setup(r => r.ReadAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await _policyDriver.ExecuteAsync(openRequest);
            var response = await _policyDriver.ExecuteAsync(fetchRequest);

            // Assert
            response.SqlCode.Should().Be(100);
            response.PolicyData.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnMinus6SqlCode_WhenFetchThrowsDbException()
        {
            // Arrange
            var openRequest = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };
            var fetchRequest = new PolicyDriverRequest { OperationType = "FETCH", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dbDataReaderMock.Setup(r => r.ReadAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(CreateDbException(-999));

            // Act
            await _policyDriver.ExecuteAsync(openRequest);
            var response = await _policyDriver.ExecuteAsync(fetchRequest);

            // Assert
            response.SqlCode.Should().Be(-999);
            response.PolicyData.Should().BeNull();
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error fetching from policy cursor")),
                It.IsAny<DbException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZeroSqlCode_WhenCloseIsCalledWithOpenCursor()
        {
            // Arrange
            var openRequest = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };
            var closeRequest = new PolicyDriverRequest { OperationType = "CLOSE", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dbDataReaderMock.Setup(r => r.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _dbCommandMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            // Act
            await _policyDriver.ExecuteAsync(openRequest);
            var response = await _policyDriver.ExecuteAsync(closeRequest);

            // Assert
            response.SqlCode.Should().Be(0);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnZeroSqlCode_WhenCloseIsCalledWithoutOpenCursor()
        {
            // Arrange
            var closeRequest = new PolicyDriverRequest { OperationType = "CLOSE", ProcessDate = "2024-06-01" };

            // Act
            var response = await _policyDriver.ExecuteAsync(closeRequest);

            // Assert
            response.SqlCode.Should().Be(0);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Policy cursor is not open")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnMinus7SqlCode_WhenCloseThrowsDbException()
        {
            // Arrange
            var openRequest = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };
            var closeRequest = new PolicyDriverRequest { OperationType = "CLOSE", ProcessDate = "2024-06-01" };

            var paramMock = new Mock<DbParameter>();
            var paramCollectionMock = new Mock<DbParameterCollection>();
            paramCollectionMock.Setup(p => p.Add(It.IsAny<DbParameter>())).Returns(0);

            _dbCommandMock.Setup(c => c.CreateParameter()).Returns(paramMock.Object);
            _dbCommandMock.SetupGet(c => c.Parameters).Returns(paramCollectionMock.Object);
            _dbCommandMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(_dbDataReaderMock.Object);

            _dbConnectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
            _dbConnectionMock.Setup(c => c.OpenAsync(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dbDataReaderMock.Setup(r => r.DisposeAsync()).Throws(CreateDbException(-888));
            _dbCommandMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

            // Act
            await _policyDriver.ExecuteAsync(openRequest);
            var response = await _policyDriver.ExecuteAsync(closeRequest);

            // Assert
            response.SqlCode.Should().Be(-888);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error closing policy cursor")),
                It.IsAny<DbException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenDbConnectionIsNull()
        {
            // Arrange
            DbConnection dbConnection = null;

            // Act
            Action act = () => new PolicyDriver(dbConnection, _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange
            ILogger<PolicyDriver> logger = null;

            // Act
            Action act = () => new PolicyDriver(_dbConnectionMock.Object, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnMinus2SqlCode_WhenExceptionIsThrownInOperation()
        {
            // Arrange
            var request = new PolicyDriverRequest { OperationType = "OPEN", ProcessDate = "2024-06-01" };

            _dbConnectionMock.Setup(c => c.State).Throws(new InvalidOperationException("Connection error"));

            // Act
            var response = await _policyDriver.ExecuteAsync(request);

            // Assert
            response.SqlCode.Should().Be(-2);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception during PolicyDriver operation")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper method to create a DbException with a specific error code
        private static DbException CreateDbException(int errorCode)
        {
            var dbExceptionType = typeof(DbException);
            var ctor = dbExceptionType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new Type[] { typeof(string), typeof(Exception) },
                null);

            var dbException = ctor.Invoke(new object[] { "Mock DB Exception", null }) as DbException;
            var errorCodeField = dbExceptionType.GetField("_errorCode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (errorCodeField != null)
            {
                errorCodeField.SetValue(dbException, errorCode);
            }
            return dbException;
        }
    }
}