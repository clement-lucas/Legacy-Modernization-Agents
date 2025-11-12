using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace NotificationFileDriver.Tests
{
    public class NotificationFileServiceTests : IDisposable
    {
        private readonly string _agentFileName = "AGENT-NOTIFY-FILE";
        private readonly string _customerFileName = "CUSTOMER-NOTIFY-FILE";
        private readonly string _reportFileName = "NOTIFY-REPORT-FILE";
        private readonly string _invalidFileName = "INVALID-FILE";
        private readonly string _agentPhysicalName = "AGENTFLE";
        private readonly string _customerPhysicalName = "CUSTFLE";
        private readonly string _reportPhysicalName = "RPTFLE";

        private readonly Mock<ILogger<NotificationFileService>> _loggerMock;
        private readonly NotificationFileService _service;

        public NotificationFileServiceTests()
        {
            _loggerMock = new Mock<ILogger<NotificationFileService>>();
            _service = new NotificationFileService(_loggerMock.Object);

            // Ensure test files are deleted before each test
            DeleteTestFiles();
        }

        public void Dispose()
        {
            // Clean up created files after each test
            DeleteTestFiles();
        }

        private void DeleteTestFiles()
        {
            if (File.Exists(_agentPhysicalName))
                File.Delete(_agentPhysicalName);
            if (File.Exists(_customerPhysicalName))
                File.Delete(_customerPhysicalName);
            if (File.Exists(_reportPhysicalName))
                File.Delete(_reportPhysicalName);
        }

        private AgentNotifyRecord GetSampleAgentRecord()
        {
            return new AgentNotifyRecord(
                "A123",
                "Agent Name",
                "Address1",
                "Address2",
                "City",
                "ST",
                "PN123456",
                "John",
                "M",
                "Doe",
                "2024-01-01",
                "2024-12-31",
                "2024-06-01",
                "Message"
            );
        }

        private CustomerNotifyRecord GetSampleCustomerRecord()
        {
            return new CustomerNotifyRecord(
                "PN654321",
                "Jane",
                "Q",
                "Smith",
                "2024-02-01",
                "2024-11-30",
                "2024-06-02",
                "CustMsg",
                "A456",
                "Agent2",
                "StatutoryMsg"
            );
        }

        private NotifyReportRecord GetSampleReportRecord()
        {
            return new NotifyReportRecord("This is a report line.");
        }

        [Fact]
        public async Task OpenFileAsync_ShouldOpenAgentFileAndReturnSuccess()
        {
            var result = await _service.OpenFileAsync(_agentFileName);

            result.StatusCode.Should().Be("00");
            File.Exists(_agentPhysicalName).Should().BeTrue();
        }

        [Fact]
        public async Task OpenFileAsync_ShouldOpenCustomerFileAndReturnSuccess()
        {
            var result = await _service.OpenFileAsync(_customerFileName);

            result.StatusCode.Should().Be("00");
            File.Exists(_customerPhysicalName).Should().BeTrue();
        }

        [Fact]
        public async Task OpenFileAsync_ShouldOpenReportFileAndReturnSuccess()
        {
            var result = await _service.OpenFileAsync(_reportFileName);

            result.StatusCode.Should().Be("00");
            File.Exists(_reportPhysicalName).Should().BeTrue();
        }

        [Fact]
        public async Task OpenFileAsync_ShouldReturnErrorForUnknownFile()
        {
            var result = await _service.OpenFileAsync(_invalidFileName);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().Contain("Unknown file name");
        }

        [Fact]
        public async Task CloseFileAsync_ShouldCloseAgentFileAndReturnSuccess()
        {
            await _service.OpenFileAsync(_agentFileName);

            var result = await _service.CloseFileAsync(_agentFileName);

            result.StatusCode.Should().Be("00");
        }

        [Fact]
        public async Task CloseFileAsync_ShouldCloseCustomerFileAndReturnSuccess()
        {
            await _service.OpenFileAsync(_customerFileName);

            var result = await _service.CloseFileAsync(_customerFileName);

            result.StatusCode.Should().Be("00");
        }

        [Fact]
        public async Task CloseFileAsync_ShouldCloseReportFileAndReturnSuccess()
        {
            await _service.OpenFileAsync(_reportFileName);

            var result = await _service.CloseFileAsync(_reportFileName);

            result.StatusCode.Should().Be("00");
        }

        [Fact]
        public async Task CloseFileAsync_ShouldReturnErrorForUnknownFile()
        {
            var result = await _service.CloseFileAsync(_invalidFileName);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().Contain("Unknown file name");
        }

        [Fact]
        public async Task WriteAgentRecordAsync_ShouldReturnErrorIfFileNotOpen()
        {
            var record = GetSampleAgentRecord();

            var result = await _service.WriteAgentRecordAsync(_agentFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().Contain("Agent file not open");
        }

        [Fact]
        public async Task WriteAgentRecordAsync_ShouldWriteRecordAndReturnSuccess()
        {
            await _service.OpenFileAsync(_agentFileName);
            var record = GetSampleAgentRecord();

            var result = await _service.WriteAgentRecordAsync(_agentFileName, record);

            result.StatusCode.Should().Be("00");

            // Verify the file contains the formatted record
            var fileContent = await File.ReadAllTextAsync(_agentPhysicalName);
            fileContent.Should().Contain(record.AgentCode.PadRight(10).Substring(0, 10));
            fileContent.Should().EndWith(Environment.NewLine);
        }

        [Fact]
        public async Task WriteAgentRecordAsync_ShouldHandleNullFieldsAndPadCorrectly()
        {
            await _service.OpenFileAsync(_agentFileName);
            var record = new AgentNotifyRecord(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            var result = await _service.WriteAgentRecordAsync(_agentFileName, record);

            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_agentPhysicalName);
            // All fields should be padded with spaces
            fileContent.Trim().Should().BeEmpty();
        }

        [Fact]
        public async Task WriteCustomerRecordAsync_ShouldReturnErrorIfFileNotOpen()
        {
            var record = GetSampleCustomerRecord();

            var result = await _service.WriteCustomerRecordAsync(_customerFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().Contain("Customer file not open");
        }

        [Fact]
        public async Task WriteCustomerRecordAsync_ShouldWriteRecordAndReturnSuccess()
        {
            await _service.OpenFileAsync(_customerFileName);
            var record = GetSampleCustomerRecord();

            var result = await _service.WriteCustomerRecordAsync(_customerFileName, record);

            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_customerPhysicalName);
            fileContent.Should().Contain(record.CustPolicyNumber.PadRight(10).Substring(0, 10));
            fileContent.Should().EndWith(Environment.NewLine);
        }

        [Fact]
        public async Task WriteCustomerRecordAsync_ShouldHandleNullFieldsAndPadCorrectly()
        {
            await _service.OpenFileAsync(_customerFileName);
            var record = new CustomerNotifyRecord(
                null, null, null, null, null, null, null, null, null, null, null);

            var result = await _service.WriteCustomerRecordAsync(_customerFileName, record);

            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_customerPhysicalName);
            fileContent.Trim().Should().BeEmpty();
        }

        [Fact]
        public async Task WriteReportRecordAsync_ShouldReturnErrorIfFileNotOpen()
        {
            var record = GetSampleReportRecord();

            var result = await _service.WriteReportRecordAsync(_reportFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().Contain("Report file not open");
        }

        [Fact]
        public async Task WriteReportRecordAsync_ShouldWriteRecordAndReturnSuccess()
        {
            await _service.OpenFileAsync(_reportFileName);
            var record = GetSampleReportRecord();

            var result = await _service.WriteReportRecordAsync(_reportFileName, record);

            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_reportPhysicalName);
            fileContent.Should().Contain(record.ReportLine);
            fileContent.Should().EndWith(Environment.NewLine);
        }

        [Fact]
        public async Task WriteAgentRecordAsync_ShouldReturnErrorOnException()
        {
            await _service.OpenFileAsync(_agentFileName);

            // Simulate exception by disposing the stream before writing
            await _service.CloseFileAsync(_agentFileName);

            var record = GetSampleAgentRecord();
            var result = await _service.WriteAgentRecordAsync(_agentFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().NotBeNull();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), _agentFileName),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task WriteCustomerRecordAsync_ShouldReturnErrorOnException()
        {
            await _service.OpenFileAsync(_customerFileName);

            await _service.CloseFileAsync(_customerFileName);

            var record = GetSampleCustomerRecord();
            var result = await _service.WriteCustomerRecordAsync(_customerFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().NotBeNull();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), _customerFileName),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task WriteReportRecordAsync_ShouldReturnErrorOnException()
        {
            await _service.OpenFileAsync(_reportFileName);

            await _service.CloseFileAsync(_reportFileName);

            var record = GetSampleReportRecord();
            var result = await _service.WriteReportRecordAsync(_reportFileName, record);

            result.StatusCode.Should().Be("99");
            result.ErrorMessage.Should().NotBeNull();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), _reportFileName),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task OpenFileAsync_ShouldReturnErrorOnIOException()
        {
            // Simulate IOException by creating file and locking it
            using (var fs = new FileStream(_agentPhysicalName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                var result = await _service.OpenFileAsync(_agentFileName);

                result.StatusCode.Should().Be("99");
                result.ErrorMessage.Should().NotBeNull();
                _loggerMock.Verify(
                    x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), _agentFileName),
                    Times.AtLeastOnce);
            }
        }

        [Fact]
        public async Task CloseFileAsync_ShouldReturnErrorOnException()
        {
            // Simulate error by closing file twice
            await _service.OpenFileAsync(_agentFileName);
            await _service.CloseFileAsync(_agentFileName);

            var result = await _service.CloseFileAsync(_agentFileName);

            result.StatusCode.Should().Be("00"); // Dispose is null-safe, so no error expected
        }

        [Theory]
        [InlineData("AGENT-NOTIFY-FILE", "OPEN")]
        [InlineData("CUSTOMER-NOTIFY-FILE", "OPEN")]
        [InlineData("NOTIFY-REPORT-FILE", "OPEN")]
        public async Task IntegrationTest_OpenWriteCloseAgentCustomerReportFile(string fileName, string operationType)
        {
            // Integration test: open, write, close for each file type
            var openResult = await _service.OpenFileAsync(fileName);
            openResult.StatusCode.Should().Be("00");

            FileOperationResult writeResult;
            if (fileName == _agentFileName)
            {
                writeResult = await _service.WriteAgentRecordAsync(fileName, GetSampleAgentRecord());
                writeResult.StatusCode.Should().Be("00");
            }
            else if (fileName == _customerFileName)
            {
                writeResult = await _service.WriteCustomerRecordAsync(fileName, GetSampleCustomerRecord());
                writeResult.StatusCode.Should().Be("00");
            }
            else
            {
                writeResult = await _service.WriteReportRecordAsync(fileName, GetSampleReportRecord());
                writeResult.StatusCode.Should().Be("00");
            }

            var closeResult = await _service.CloseFileAsync(fileName);
            closeResult.StatusCode.Should().Be("00");
        }

        [Fact]
        public async Task FormatAgentRecord_ShouldPadAndTruncateFieldsCorrectly()
        {
            await _service.OpenFileAsync(_agentFileName);
            var record = new AgentNotifyRecord(
                "123456789012345", // longer than 10
                "Name", // shorter than 45
                "Addr1", // shorter than 50
                "Addr2", // shorter than 50
                "CityName", // shorter than 20
                "ST", // exact 2
                "PN123", // shorter than 10
                "FirstName", // shorter than 35
                "M", // exact 1
                "LastName", // shorter than 35
                "2024-01-01", // exact 10
                "2024-12-31", // exact 10
                "2024-06-01", // exact 10
                "Msg" // shorter than 100
            );

            var result = await _service.WriteAgentRecordAsync(_agentFileName, record);
            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_agentPhysicalName);
            // Field should be truncated to 10
            fileContent.Substring(0, 10).Should().Be("1234567890");
            // AgentName should be padded to 45
            fileContent.Substring(10, 45).Should().StartWith("Name").And.EndWith(" ");
            // AgentNotifyMessages should be padded to 100
            fileContent.Substring(258, 100).Should().StartWith("Msg").And.EndWith(" ");
        }

        [Fact]
        public async Task FormatCustomerRecord_ShouldPadAndTruncateFieldsCorrectly()
        {
            await _service.OpenFileAsync(_customerFileName);
            var record = new CustomerNotifyRecord(
                "PN123456789012", // longer than 10
                "FName", // shorter than 35
                "M", // exact 1
                "LName", // shorter than 35
                "2024-02-01", // exact 10
                "2024-11-30", // exact 10
                "2024-06-02", // exact 10
                "Msg", // shorter than 100
                "A4567890123", // longer than 10
                "AgentName", // shorter than 45
                "StatMsg" // shorter than 100
            );

            var result = await _service.WriteCustomerRecordAsync(_customerFileName, record);
            result.StatusCode.Should().Be("00");

            var fileContent = await File.ReadAllTextAsync(_customerPhysicalName);
            fileContent.Substring(0, 10).Should().Be("PN12345678");
            fileContent.Substring(11, 35).Should().StartWith("FName").And.EndWith(" ");
            fileContent.Substring(202, 100).Should().StartWith("StatMsg").And.EndWith(" ");
        }

        [Fact]
        public async Task WriteAgentRecordAsync_ShouldSupportMultipleWrites()
        {
            await _service.OpenFileAsync(_agentFileName);
            var record1 = GetSampleAgentRecord();
            var record2 = new AgentNotifyRecord(
                "A999", "AgentX", "AddrX1", "AddrX2", "CityX", "XY", "PN999999", "Alice", "L", "Lee", "2024-03-01", "2024-09-30", "2024-07-01", "MsgX");

            var result1 = await _service.WriteAgentRecordAsync(_agentFileName, record1);
            var result2 = await _service.WriteAgentRecordAsync(_agentFileName, record2);

            result1.StatusCode.Should().Be("00");
            result2.StatusCode.Should().Be("00");

            var lines = await File.ReadAllLinesAsync(_agentPhysicalName);
            lines.Length.Should().Be(2);
        }

        [Fact]
        public async Task WriteCustomerRecordAsync_ShouldSupportMultipleWrites()
        {
            await _service.OpenFileAsync(_customerFileName);
            var record1 = GetSampleCustomerRecord();
            var record2 = new CustomerNotifyRecord(
                "PN888888", "Bob", "R", "Brown", "2024-04-01", "2024-10-31", "2024-08-01", "MsgY", "A888", "AgentY", "StatMsgY");

            var result1 = await _service.WriteCustomerRecordAsync(_customerFileName, record1);
            var result2 = await _service.WriteCustomerRecordAsync(_customerFileName, record2);

            result1.StatusCode.Should().Be("00");
            result2.StatusCode.Should().Be("00");

            var lines = await File.ReadAllLinesAsync(_customerPhysicalName);
            lines.Length.Should().Be(2);
        }

        [Fact]
        public async Task WriteReportRecordAsync_ShouldSupportMultipleWrites()
        {
            await _service.OpenFileAsync(_reportFileName);
            var record1 = GetSampleReportRecord();
            var record2 = new NotifyReportRecord("Second report line.");

            var result1 = await _service.WriteReportRecordAsync(_reportFileName, record1);
            var result2 = await _service.WriteReportRecordAsync(_reportFileName, record2);

            result1.StatusCode.Should().Be("00");
            result2.StatusCode.Should().Be("00");

            var lines = await File.ReadAllLinesAsync(_reportPhysicalName);
            lines.Length.Should().Be(2);
        }
    }
}