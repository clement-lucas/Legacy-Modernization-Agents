using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Insurance.PolicyExpiryBatch;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Insurance.PolicyExpiryBatch.Tests
{
    public class PolicyExpiryBatchProcessorTests : IDisposable
    {
        private readonly Mock<IDbDriver1> _dbDriver1Mock;
        private readonly Mock<IDbDriver2> _dbDriver2Mock;
        private readonly Mock<IFileDriver1> _fileDriver1Mock;
        private readonly Mock<IFileDriver2> _fileDriver2Mock;
        private readonly Mock<ILogger<PolicyExpiryBatchProcessor>> _loggerMock;
        private readonly PolicyExpiryBatchProcessor _processor;

        public PolicyExpiryBatchProcessorTests()
        {
            _dbDriver1Mock = new Mock<IDbDriver1>(MockBehavior.Strict);
            _dbDriver2Mock = new Mock<IDbDriver2>(MockBehavior.Strict);
            _fileDriver1Mock = new Mock<IFileDriver1>(MockBehavior.Strict);
            _fileDriver2Mock = new Mock<IFileDriver2>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<PolicyExpiryBatchProcessor>>(MockBehavior.Loose);

            _processor = new PolicyExpiryBatchProcessor(
                _dbDriver1Mock.Object,
                _dbDriver2Mock.Object,
                _fileDriver1Mock.Object,
                _fileDriver2Mock.Object,
                _loggerMock.Object
            );
        }

        public void Dispose()
        {
            // Cleanup if necessary
        }

        [Fact]
        public async Task RunAsync_ShouldProcessSuccessfully_WhenAllDependenciesSucceed()
        {
            // Arrange
            var processDate = DateTime.Now;
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = GetSampleAgentRecord() });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenOpenCursorFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = false, SqlCode = -100 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error opening policy cursor: -100");
            _loggerMock.VerifyLog(LogLevel.Error, "Error opening policy cursor: {SqlCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenOpenAgentFileFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = false, StatusCode = 99 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error opening agent file: 99");
            _loggerMock.VerifyLog(LogLevel.Error, "Error opening agent file: {StatusCode}", Times.Once());
        }

        [Theory]
        [InlineData("CUSTOMER-NOTIFY-FILE")]
        [InlineData("NOTIFY-REPORT-FILE")]
        [InlineData("AGENT-NOTIFY-FILE")]
        public async Task RunAsync_ShouldLogAndThrow_WhenOpenNotifyFileFails(string fileName)
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(fileName))
                .ReturnsAsync(new FileOpenResult { IsSuccess = false, StatusCode = 77 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.Is<string>(f => f != fileName)))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage($"Error opening notify file {fileName}: 77");
            _loggerMock.VerifyLog(LogLevel.Error, $"Error opening notify file {fileName}: {{StatusCode}}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenFetchNextPolicyReturnsError()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.Setup(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Error,
                    SqlCode = -200
                });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error fetching policy record: -200");
            _loggerMock.VerifyLog(LogLevel.Error, "Error fetching policy record: {SqlCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenSearchAgentFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = false, StatusCode = 88 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error fetching agent record: 88");
            _loggerMock.VerifyLog(LogLevel.Error, "Error fetching agent record: {StatusCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogWarning_WhenWriteCustomerNotifyFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = GetSampleAgentRecord() });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = false, StatusCode = 55 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
            _loggerMock.VerifyLog(LogLevel.Warning, "Error writing to customer notify file: {StatusCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogWarning_WhenWriteAgentNotifyFails_ForCorporateAgent()
        {
            // Arrange
            var agent = GetSampleAgentRecord();
            agent.AgentType = "CORPORATE";
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = agent });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = false, StatusCode = 66 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
            _loggerMock.VerifyLog(LogLevel.Warning, "Error writing to agent notify file: {StatusCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenInsertTrackingFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = GetSampleAgentRecord() });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = false, SqlCode = -300 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error inserting into tracking table: -300");
            _loggerMock.VerifyLog(LogLevel.Error, "Error inserting into tracking table: {SqlCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenCloseCursorFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = false, SqlCode = -400 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error closing policy cursor: -400");
            _loggerMock.VerifyLog(LogLevel.Error, "Error closing policy cursor: {SqlCode}", Times.Once());
        }

        [Fact]
        public async Task RunAsync_ShouldLogAndThrow_WhenCloseAgentFileFails()
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = false, StatusCode = 99 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error closing agent file: 99");
            _loggerMock.VerifyLog(LogLevel.Error, "Error closing agent file: {StatusCode}", Times.Once());
        }

        [Theory]
        [InlineData("CUSTOMER-NOTIFY-FILE")]
        [InlineData("NOTIFY-REPORT-FILE")]
        [InlineData("AGENT-NOTIFY-FILE")]
        public async Task RunAsync_ShouldLogAndThrow_WhenCloseNotifyFileFails(string fileName)
        {
            // Arrange
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(fileName))
                .ReturnsAsync(new FileCloseResult { IsSuccess = false, StatusCode = 77 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.Is<string>(f => f != fileName)))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage($"Error closing notify file {fileName}: 77");
            _loggerMock.VerifyLog(LogLevel.Error, $"Error closing notify file {fileName}: {{StatusCode}}", Times.Once());
        }

        [Fact]
        public async Task PopulateCustomerDetail_ShouldThrow_WhenCurrentPolicyIsNull()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateCustomerDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => method.Invoke(_processor, null);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("Current policy or agent is null.");
        }

        [Fact]
        public async Task PopulateCustomerDetail_ShouldThrow_WhenCurrentAgentIsNull()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateCustomerDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processorType.GetField("_currentPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSamplePolicyRecord());

            // Act
            Action act = () => method.Invoke(_processor, null);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("Current policy or agent is null.");
        }

        [Fact]
        public async Task PopulateCustomerDetail_ShouldPopulateFieldsCorrectly()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateCustomerDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processorType.GetField("_currentPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSamplePolicyRecord());
            processorType.GetField("_currentAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSampleAgentRecord());
            processorType.GetField("_processDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, new DateTime(2024, 6, 1));

            // Act
            method.Invoke(_processor, null);
            var customerNotifyRecord = (CustomerNotifyRecord)processorType.GetField("_customerNotifyRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_processor);

            // Assert
            customerNotifyRecord.PolicyNumber.Should().Be("P12345");
            customerNotifyRecord.AgentName.Should().Be("John Agent");
            customerNotifyRecord.NotifyDate.Should().Be("06/01/2024");
            customerNotifyRecord.NotifyMessage.Should().Contain("EXPIRING SOON");
            customerNotifyRecord.StatutoryMessage.Should().Contain("INSURANCE COVERAGE WILL END");
        }

        [Fact]
        public async Task PopulateAgentDetail_ShouldThrow_WhenCurrentPolicyIsNull()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateAgentDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Action act = () => method.Invoke(_processor, null);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("Current policy or agent is null.");
        }

        [Fact]
        public async Task PopulateAgentDetail_ShouldThrow_WhenCurrentAgentIsNull()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateAgentDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processorType.GetField("_currentPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSamplePolicyRecord());

            // Act
            Action act = () => method.Invoke(_processor, null);

            // Assert
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithMessage("Current policy or agent is null.");
        }

        [Fact]
        public async Task PopulateAgentDetail_ShouldPopulateFieldsCorrectly()
        {
            // Arrange
            var processorType = typeof(PolicyExpiryBatchProcessor);
            var method = processorType.GetMethod("PopulateAgentDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processorType.GetField("_currentPolicy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSamplePolicyRecord());
            processorType.GetField("_currentAgent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, GetSampleAgentRecord());
            processorType.GetField("_processDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_processor, new DateTime(2024, 6, 1));

            // Act
            method.Invoke(_processor, null);
            var agentNotifyRecord = (AgentNotifyRecord)processorType.GetField("_agentNotifyRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_processor);

            // Assert
            agentNotifyRecord.AgentCode.Should().Be("A987");
            agentNotifyRecord.AgentName.Should().Be("John Agent");
            agentNotifyRecord.PolicyNumber.Should().Be("P12345");
            agentNotifyRecord.NotifyDate.Should().Be("06/01/2024");
            agentNotifyRecord.NotifyMessage.Should().Contain("EXPIRING SOON");
        }

        // Edge case: Policy with zero premium
        [Fact]
        public async Task RunAsync_ShouldProcessPolicyWithZeroPremium()
        {
            // Arrange
            var policy = GetSamplePolicyRecord();
            policy.PremiumAmount = 0m;
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = policy
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = GetSampleAgentRecord() });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        // Edge case: Policy with null beneficiary name
        [Fact]
        public async Task RunAsync_ShouldProcessPolicyWithNullBeneficiaryName()
        {
            // Arrange
            var policy = GetSamplePolicyRecord();
            policy.BeneficiaryName = null;
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = policy
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = GetSampleAgentRecord() });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        // Edge case: Agent is not corporate, agent notify should not be written
        [Fact]
        public async Task RunAsync_ShouldNotWriteAgentNotify_ForNonCorporateAgent()
        {
            // Arrange
            var agent = GetSampleAgentRecord();
            agent.AgentType = "INDIVIDUAL";
            _dbDriver1Mock.Setup(x => x.OpenCursorAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new DbOpenResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.OpenAsync())
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.OpenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileOpenResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver1Mock.SetupSequence(x => x.FetchNextPolicyAsync())
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.Success,
                    Policy = GetSamplePolicyRecord()
                })
                .ReturnsAsync(new DbFetchResult
                {
                    Status = DbFetchStatus.EndOfData
                });
            _fileDriver1Mock.Setup(x => x.SearchAgentAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileAgentResult { IsSuccess = true, StatusCode = 0, Agent = agent });
            _fileDriver2Mock.Setup(x => x.WriteCustomerNotifyAsync(It.IsAny<string>(), It.IsAny<CustomerNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()))
                .ReturnsAsync(new FileWriteResult { IsSuccess = true, StatusCode = 0 });
            _dbDriver2Mock.Setup(x => x.InsertTrackingAsync(It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(new DbInsertResult { IsSuccess = true, SqlCode = 0 });
            _dbDriver1Mock.Setup(x => x.CloseCursorAsync())
                .ReturnsAsync(new DbCloseResult { IsSuccess = true, SqlCode = 0 });
            _fileDriver1Mock.Setup(x => x.CloseAsync())
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });
            _fileDriver2Mock.Setup(x => x.CloseAsync(It.IsAny<string>()))
                .ReturnsAsync(new FileCloseResult { IsSuccess = true, StatusCode = 0 });

            // Act
            Func<Task> act = async () => await _processor.RunAsync();

            // Assert
            await act.Should().NotThrowAsync();
            _fileDriver2Mock.Verify(x => x.WriteAgentNotifyAsync(It.IsAny<string>(), It.IsAny<AgentNotifyRecord>()), Times.Never());
        }

        // Helper methods for sample records and logger verification
        private PolicyRecord GetSamplePolicyRecord()
        {
            return new PolicyRecord
            {
                PolicyNumber = "P12345",
                HolderFirstName = "Jane",
                HolderMiddleName = "Q",
                HolderLastName = "Doe",
                HolderAddress1 = "123 Main St",
                HolderAddress2 = "Apt 4",
                HolderCity = "Metropolis",
                HolderState = "NY",
                HolderZipCode = "10001",
                StartDate = new DateTime(2023, 6, 1),
                ExpiryDate = new DateTime(2024, 6, 1),
                BeneficiaryName = "John Doe",
                AgentCode = "A987",
                HolderEmail = "jane.doe@email.com",
                PremiumAmount = 500.00m
            };
        }

        private AgentRecord GetSampleAgentRecord()
        {
            return new AgentRecord
            {
                AgentCode = "A987",
                AgentName = "John Agent",
                AgentType = "CORPORATE",
                Address1 = "456 Agency Rd",
                Address2 = "Suite 200",
                City = "Metropolis",
                State = "NY",
                ZipCode = "10001",
                Email = "john.agent@agency.com"
            };
        }
    }

    // Extension for verifying logger calls
    public static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, Times times)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                times
            );
        }
    }
}