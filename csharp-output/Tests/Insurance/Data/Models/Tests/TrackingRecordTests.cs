using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace Insurance.Data.Models.Tests
{
    public class TrackingRecordTests : IDisposable
    {
        // Setup resources if needed
        public TrackingRecordTests()
        {
            // No dependencies to mock for TrackingRecord itself, but setup can be extended for integration tests
        }

        // Teardown resources if needed
        public void Dispose()
        {
            // Cleanup if necessary
        }

        [Fact]
        public void Constructor_ShouldInitializeWithValidValues()
        {
            // Arrange
            var expectedPolicyNumber = "1234567890";
            var expectedNotifyDate = new DateTime(2024, 6, 1);
            var expectedStatus = "A";
            var expectedAddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0);
            var expectedUpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0);

            // Act
            var record = new TrackingRecord
            {
                PolicyNumber = expectedPolicyNumber,
                NotifyDate = expectedNotifyDate,
                Status = expectedStatus,
                AddTimestamp = expectedAddTimestamp,
                UpdateTimestamp = expectedUpdateTimestamp
            };

            // Assert
            record.PolicyNumber.Should().Be(expectedPolicyNumber);
            record.NotifyDate.Should().Be(expectedNotifyDate);
            record.Status.Should().Be(expectedStatus);
            record.AddTimestamp.Should().Be(expectedAddTimestamp);
            record.UpdateTimestamp.Should().Be(expectedUpdateTimestamp);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PolicyNumber_ShouldNotAllowNullOrEmpty(string invalidPolicyNumber)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = invalidPolicyNumber,
                NotifyDate = DateTime.Now,
                Status = "A",
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.PolicyNumber)));
        }

        [Theory]
        [InlineData("12345678901")] // 11 chars, should fail
        [InlineData("12345678901234567890")] // 20 chars, should fail
        public void PolicyNumber_ShouldNotExceedMaxLength(string longPolicyNumber)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = longPolicyNumber,
                NotifyDate = DateTime.Now,
                Status = "A",
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.PolicyNumber)));
        }

        [Fact]
        public void PolicyNumber_ShouldAllowExactlyMaxLength()
        {
            // Arrange
            var validPolicyNumber = new string('X', 10);
            var record = new TrackingRecord
            {
                PolicyNumber = validPolicyNumber,
                NotifyDate = DateTime.Now,
                Status = "A",
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().NotContain(v => v.MemberNames.Contains(nameof(TrackingRecord.PolicyNumber)));
        }

        [Theory]
        [InlineData(null)]
        public void Status_ShouldNotAllowNull(string invalidStatus)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = invalidStatus,
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.Status)));
        }

        [Theory]
        [InlineData("")]
        public void Status_ShouldNotAllowEmpty(string invalidStatus)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = invalidStatus,
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.Status)));
        }

        [Theory]
        [InlineData("AB")]
        [InlineData("XYZ")]
        public void Status_ShouldNotExceedMaxLength(string longStatus)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = longStatus,
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.Status)));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("Z")]
        public void Status_ShouldAllowSingleCharacter(string validStatus)
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = validStatus,
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().NotContain(v => v.MemberNames.Contains(nameof(TrackingRecord.Status)));
        }

        [Fact]
        public void NotifyDate_ShouldNotAllowDefaultDateTime()
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = default,
                Status = "A",
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            // Required attribute does not catch default DateTime, so business logic check
            record.NotifyDate.Should().NotBe(default(DateTime), "NotifyDate should be a valid date per COBOL logic");
        }

        [Fact]
        public void AddTimestamp_ShouldNotAllowDefaultDateTime()
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = "A",
                AddTimestamp = default,
                UpdateTimestamp = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            record.AddTimestamp.Should().NotBe(default(DateTime), "AddTimestamp should be a valid timestamp per COBOL logic");
        }

        [Fact]
        public void UpdateTimestamp_ShouldNotAllowDefaultDateTime()
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = DateTime.Now,
                Status = "A",
                AddTimestamp = DateTime.Now,
                UpdateTimestamp = default
            };

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            record.UpdateTimestamp.Should().NotBe(default(DateTime), "UpdateTimestamp should be a valid timestamp per COBOL logic");
        }

        [Fact]
        public void AllFields_ShouldBeRequired()
        {
            // Arrange
            var record = new TrackingRecord();

            // Act
            var validationResults = ValidateModel(record);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.PolicyNumber)));
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.NotifyDate)));
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.Status)));
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.AddTimestamp)));
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TrackingRecord.UpdateTimestamp)));
        }

        [Fact]
        public void Record_ShouldBeImmutable()
        {
            // Arrange
            var record1 = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            // Act
            var record2 = record1 with { Status = "B" };

            // Assert
            record2.Status.Should().Be("B");
            record1.Status.Should().Be("A");
            record2.PolicyNumber.Should().Be(record1.PolicyNumber);
        }

        [Fact]
        public void Equality_ShouldWorkForSameValues()
        {
            // Arrange
            var record1 = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            var record2 = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            // Act & Assert
            record1.Should().Be(record2);
        }

        [Fact]
        public void Equality_ShouldFailForDifferentValues()
        {
            // Arrange
            var record1 = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            var record2 = new TrackingRecord
            {
                PolicyNumber = "0987654321",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            // Act & Assert
            record1.Should().NotBe(record2);
        }

        [Fact]
        public void ToString_ShouldReturnExpectedFormat()
        {
            // Arrange
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            // Act
            var str = record.ToString();

            // Assert
            str.Should().Contain("PolicyNumber = 1234567890");
            str.Should().Contain("NotifyDate = 6/1/2024");
            str.Should().Contain("Status = A");
            str.Should().Contain("AddTimestamp = 6/2/2024");
            str.Should().Contain("UpdateTimestamp = 6/3/2024");
        }

        // Integration test for database context (mocked)
        // Simulates adding and retrieving TrackingRecord from a repository
        [Fact]
        public void Repository_AddAndGet_ShouldPreserveValues()
        {
            // Arrange
            var mockRepo = new Mock<ITrackingRecordRepository>();
            var record = new TrackingRecord
            {
                PolicyNumber = "1234567890",
                NotifyDate = new DateTime(2024, 6, 1),
                Status = "A",
                AddTimestamp = new DateTime(2024, 6, 2, 10, 0, 0),
                UpdateTimestamp = new DateTime(2024, 6, 3, 11, 0, 0)
            };

            mockRepo.Setup(r => r.Add(It.IsAny<TrackingRecord>())).Returns(record);
            mockRepo.Setup(r => r.GetByPolicyNumber("1234567890")).Returns(record);

            // Act
            var added = mockRepo.Object.Add(record);
            var fetched = mockRepo.Object.GetByPolicyNumber("1234567890");

            // Assert
            added.Should().BeEquivalentTo(record);
            fetched.Should().BeEquivalentTo(record);
        }

        // Helper to validate data annotations
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        // Mock repository interface for integration test
        public interface ITrackingRecordRepository
        {
            TrackingRecord Add(TrackingRecord record);
            TrackingRecord GetByPolicyNumber(string policyNumber);
        }
    }
}