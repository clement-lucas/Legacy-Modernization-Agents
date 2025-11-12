using System;
using FluentAssertions;
using Xunit;
using Insurance.Notifications.Models;
using Moq;

namespace Insurance.Notifications.Models.Tests
{
    /// <summary>
    /// Comprehensive unit tests for AgentNotifyRecord.
    /// </summary>
    public class forTests : IDisposable
    {
        // Setup resources if needed
        public forTests()
        {
            // No dependencies to mock for record type, but constructor provided for future extensibility
        }

        // Teardown resources if needed
        public void Dispose()
        {
            // Cleanup if necessary
        }

        [Fact]
        public void Constructor_ShouldInitializePropertiesWithDefaultValues()
        {
            // Arrange & Act
            var record = new AgentNotifyRecord();

            // Assert
            record.AgentCode.Should().BeEmpty();
            record.AgentName.Should().BeEmpty();
            record.AgentAddress1.Should().BeEmpty();
            record.AgentAddress2.Should().BeEmpty();
            record.AgentCity.Should().BeEmpty();
            record.AgentState.Should().BeEmpty();
            record.PolicyNumber.Should().BeEmpty();
            record.PolicyHolderFirstName.Should().BeEmpty();
            record.PolicyHolderMiddleInitial.Should().BeEmpty();
            record.PolicyHolderLastName.Should().BeEmpty();
            record.PolicyStartDate.Should().BeEmpty();
            record.PolicyExpiryDate.Should().BeEmpty();
            record.NotifyDate.Should().BeEmpty();
            record.NotifyMessages.Should().BeEmpty();
        }

        [Fact]
        public void Properties_ShouldBeSettableViaInit()
        {
            // Arrange & Act
            var record = new AgentNotifyRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentAddress1 = "123 Main St",
                AgentAddress2 = "Suite 100",
                AgentCity = "Springfield",
                AgentState = "IL",
                PolicyNumber = "PN456",
                PolicyHolderFirstName = "Jane",
                PolicyHolderMiddleInitial = "Q",
                PolicyHolderLastName = "Smith",
                PolicyStartDate = "2024-01-01",
                PolicyExpiryDate = "2024-12-31",
                NotifyDate = "2024-06-01",
                NotifyMessages = "Policy expiring soon"
            };

            // Assert
            record.AgentCode.Should().Be("A123");
            record.AgentName.Should().Be("John Doe");
            record.AgentAddress1.Should().Be("123 Main St");
            record.AgentAddress2.Should().Be("Suite 100");
            record.AgentCity.Should().Be("Springfield");
            record.AgentState.Should().Be("IL");
            record.PolicyNumber.Should().Be("PN456");
            record.PolicyHolderFirstName.Should().Be("Jane");
            record.PolicyHolderMiddleInitial.Should().Be("Q");
            record.PolicyHolderLastName.Should().Be("Smith");
            record.PolicyStartDate.Should().Be("2024-01-01");
            record.PolicyExpiryDate.Should().Be("2024-12-31");
            record.NotifyDate.Should().Be("2024-06-01");
            record.NotifyMessages.Should().Be("Policy expiring soon");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Properties_ShouldAllowNullOrEmptyStrings(string testValue)
        {
            // Arrange & Act
            var record = new AgentNotifyRecord
            {
                AgentCode = testValue,
                AgentName = testValue,
                AgentAddress1 = testValue,
                AgentAddress2 = testValue,
                AgentCity = testValue,
                AgentState = testValue,
                PolicyNumber = testValue,
                PolicyHolderFirstName = testValue,
                PolicyHolderMiddleInitial = testValue,
                PolicyHolderLastName = testValue,
                PolicyStartDate = testValue,
                PolicyExpiryDate = testValue,
                NotifyDate = testValue,
                NotifyMessages = testValue
            };

            // Assert
            record.AgentCode.Should().Be(testValue);
            record.AgentName.Should().Be(testValue);
            record.AgentAddress1.Should().Be(testValue);
            record.AgentAddress2.Should().Be(testValue);
            record.AgentCity.Should().Be(testValue);
            record.AgentState.Should().Be(testValue);
            record.PolicyNumber.Should().Be(testValue);
            record.PolicyHolderFirstName.Should().Be(testValue);
            record.PolicyHolderMiddleInitial.Should().Be(testValue);
            record.PolicyHolderLastName.Should().Be(testValue);
            record.PolicyStartDate.Should().Be(testValue);
            record.PolicyExpiryDate.Should().Be(testValue);
            record.NotifyDate.Should().Be(testValue);
            record.NotifyMessages.Should().Be(testValue);
        }

        [Theory]
        [InlineData("2024-01-01")]
        [InlineData("2024-12-31")]
        [InlineData("0001-01-01")]
        [InlineData("9999-12-31")]
        public void PolicyDates_ShouldAcceptValidDateStrings(string dateValue)
        {
            // Arrange & Act
            var record = new AgentNotifyRecord
            {
                PolicyStartDate = dateValue,
                PolicyExpiryDate = dateValue,
                NotifyDate = dateValue
            };

            // Assert
            record.PolicyStartDate.Should().Be(dateValue);
            record.PolicyExpiryDate.Should().Be(dateValue);
            record.NotifyDate.Should().Be(dateValue);
        }

        [Theory]
        [InlineData("not-a-date")]
        [InlineData("2024/01/01")]
        [InlineData("01-01-2024")]
        [InlineData("2024-13-01")]
        public void PolicyDates_ShouldAcceptAnyStringFormat(string invalidDate)
        {
            // Arrange & Act
            var record = new AgentNotifyRecord
            {
                PolicyStartDate = invalidDate,
                PolicyExpiryDate = invalidDate,
                NotifyDate = invalidDate
            };

            // Assert
            record.PolicyStartDate.Should().Be(invalidDate);
            record.PolicyExpiryDate.Should().Be(invalidDate);
            record.NotifyDate.Should().Be(invalidDate);
        }

        [Fact]
        public void RecordEquality_ShouldBeBasedOnAllProperties()
        {
            // Arrange
            var record1 = new AgentNotifyRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentAddress1 = "123 Main St",
                AgentAddress2 = "Suite 100",
                AgentCity = "Springfield",
                AgentState = "IL",
                PolicyNumber = "PN456",
                PolicyHolderFirstName = "Jane",
                PolicyHolderMiddleInitial = "Q",
                PolicyHolderLastName = "Smith",
                PolicyStartDate = "2024-01-01",
                PolicyExpiryDate = "2024-12-31",
                NotifyDate = "2024-06-01",
                NotifyMessages = "Policy expiring soon"
            };

            var record2 = new AgentNotifyRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentAddress1 = "123 Main St",
                AgentAddress2 = "Suite 100",
                AgentCity = "Springfield",
                AgentState = "IL",
                PolicyNumber = "PN456",
                PolicyHolderFirstName = "Jane",
                PolicyHolderMiddleInitial = "Q",
                PolicyHolderLastName = "Smith",
                PolicyStartDate = "2024-01-01",
                PolicyExpiryDate = "2024-12-31",
                NotifyDate = "2024-06-01",
                NotifyMessages = "Policy expiring soon"
            };

            // Assert
            record1.Should().Be(record2);
            record1.GetHashCode().Should().Be(record2.GetHashCode());
        }

        [Fact]
        public void RecordEquality_ShouldDetectDifferentRecords()
        {
            // Arrange
            var record1 = new AgentNotifyRecord
            {
                AgentCode = "A123"
            };
            var record2 = new AgentNotifyRecord
            {
                AgentCode = "B456"
            };

            // Assert
            record1.Should().NotBe(record2);
        }

        [Fact]
        public void Record_ShouldBeImmutableAfterCreation()
        {
            // Arrange
            var record = new AgentNotifyRecord
            {
                AgentCode = "A123"
            };

            // Act & Assert
            Action act = () => record.AgentCode = "B456";
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void NotifyMessages_ShouldAllowLongStrings()
        {
            // Arrange
            var longMessage = new string('A', 10000);
            var record = new AgentNotifyRecord
            {
                NotifyMessages = longMessage
            };

            // Assert
            record.NotifyMessages.Should().Be(longMessage);
        }

        [Fact]
        public void PolicyHolderMiddleInitial_ShouldAllowSingleCharacter()
        {
            // Arrange
            var record = new AgentNotifyRecord
            {
                PolicyHolderMiddleInitial = "X"
            };

            // Assert
            record.PolicyHolderMiddleInitial.Should().Be("X");
        }

        [Fact]
        public void PolicyHolderMiddleInitial_ShouldAllowEmptyOrNull()
        {
            // Arrange
            var recordEmpty = new AgentNotifyRecord
            {
                PolicyHolderMiddleInitial = ""
            };
            var recordNull = new AgentNotifyRecord
            {
                PolicyHolderMiddleInitial = null
            };

            // Assert
            recordEmpty.PolicyHolderMiddleInitial.Should().BeEmpty();
            recordNull.PolicyHolderMiddleInitial.Should().BeNull();
        }

        [Fact]
        public void AgentState_ShouldAllowTwoLetterAbbreviation()
        {
            // Arrange
            var record = new AgentNotifyRecord
            {
                AgentState = "NY"
            };

            // Assert
            record.AgentState.Should().Be("NY");
        }

        [Theory]
        [InlineData("N")]
        [InlineData("NEWYORK")]
        [InlineData("")]
        public void AgentState_ShouldAllowAnyStringLength(string state)
        {
            // Arrange
            var record = new AgentNotifyRecord
            {
                AgentState = state
            };

            // Assert
            record.AgentState.Should().Be(state);
        }

        // Integration test example: Simulate storing and retrieving record from a database
        // This test assumes a repository interface exists, which would be mocked.
        [Fact]
        public void Integration_SaveAndRetrieveAgentNotifyRecord_ShouldPreserveAllFields()
        {
            // Arrange
            var mockRepository = new Mock<IAgentNotifyRecordRepository>();
            var record = new AgentNotifyRecord
            {
                AgentCode = "A123",
                AgentName = "John Doe",
                AgentAddress1 = "123 Main St",
                AgentAddress2 = "Suite 100",
                AgentCity = "Springfield",
                AgentState = "IL",
                PolicyNumber = "PN456",
                PolicyHolderFirstName = "Jane",
                PolicyHolderMiddleInitial = "Q",
                PolicyHolderLastName = "Smith",
                PolicyStartDate = "2024-01-01",
                PolicyExpiryDate = "2024-12-31",
                NotifyDate = "2024-06-01",
                NotifyMessages = "Policy expiring soon"
            };

            mockRepository.Setup(r => r.Save(record)).Verifiable();
            mockRepository.Setup(r => r.GetByPolicyNumber("PN456")).Returns(record);

            // Act
            mockRepository.Object.Save(record);
            var retrieved = mockRepository.Object.GetByPolicyNumber("PN456");

            // Assert
            mockRepository.Verify(r => r.Save(record), Times.Once);
            retrieved.Should().BeEquivalentTo(record);
        }

        // Edge case: All properties set to null
        [Fact]
        public void AllPropertiesSetToNull_ShouldNotThrowAndShouldPreserveNulls()
        {
            // Arrange & Act
            var record = new AgentNotifyRecord
            {
                AgentCode = null,
                AgentName = null,
                AgentAddress1 = null,
                AgentAddress2 = null,
                AgentCity = null,
                AgentState = null,
                PolicyNumber = null,
                PolicyHolderFirstName = null,
                PolicyHolderMiddleInitial = null,
                PolicyHolderLastName = null,
                PolicyStartDate = null,
                PolicyExpiryDate = null,
                NotifyDate = null,
                NotifyMessages = null
            };

            // Assert
            record.AgentCode.Should().BeNull();
            record.AgentName.Should().BeNull();
            record.AgentAddress1.Should().BeNull();
            record.AgentAddress2.Should().BeNull();
            record.AgentCity.Should().BeNull();
            record.AgentState.Should().BeNull();
            record.PolicyNumber.Should().BeNull();
            record.PolicyHolderFirstName.Should().BeNull();
            record.PolicyHolderMiddleInitial.Should().BeNull();
            record.PolicyHolderLastName.Should().BeNull();
            record.PolicyStartDate.Should().BeNull();
            record.PolicyExpiryDate.Should().BeNull();
            record.NotifyDate.Should().BeNull();
            record.NotifyMessages.Should().BeNull();
        }

        // Edge case: Boundary conditions for string length
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        [InlineData(1024)]
        public void Properties_ShouldAcceptVariousStringLengths(int length)
        {
            // Arrange
            var value = new string('X', length);
            var record = new AgentNotifyRecord
            {
                AgentCode = value,
                AgentName = value,
                AgentAddress1 = value,
                AgentAddress2 = value,
                AgentCity = value,
                AgentState = value,
                PolicyNumber = value,
                PolicyHolderFirstName = value,
                PolicyHolderMiddleInitial = value,
                PolicyHolderLastName = value,
                PolicyStartDate = value,
                PolicyExpiryDate = value,
                NotifyDate = value,
                NotifyMessages = value
            };

            // Assert
            record.AgentCode.Should().Be(value);
            record.AgentName.Should().Be(value);
            record.AgentAddress1.Should().Be(value);
            record.AgentAddress2.Should().Be(value);
            record.AgentCity.Should().Be(value);
            record.AgentState.Should().Be(value);
            record.PolicyNumber.Should().Be(value);
            record.PolicyHolderFirstName.Should().Be(value);
            record.PolicyHolderMiddleInitial.Should().Be(value);
            record.PolicyHolderLastName.Should().Be(value);
            record.PolicyStartDate.Should().Be(value);
            record.PolicyExpiryDate.Should().Be(value);
            record.NotifyDate.Should().Be(value);
            record.NotifyMessages.Should().Be(value);
        }

        // Comment: This test ensures that the record can be used in a collection and compared for uniqueness.
        [Fact]
        public void Record_ShouldBeUsableInHashSetAndDictionary()
        {
            // Arrange
            var record1 = new AgentNotifyRecord { AgentCode = "A123" };
            var record2 = new AgentNotifyRecord { AgentCode = "A123" };
            var record3 = new AgentNotifyRecord { AgentCode = "B456" };

            var set = new System.Collections.Generic.HashSet<AgentNotifyRecord>();
            set.Add(record1);
            set.Add(record2); // Should not add duplicate
            set.Add(record3);

            // Assert
            set.Count.Should().Be(2);

            var dict = new System.Collections.Generic.Dictionary<AgentNotifyRecord, string>();
            dict[record1] = "First";
            dict[record2] = "Second"; // Should overwrite
            dict[record3] = "Third";

            dict.Count.Should().Be(2);
            dict[record1].Should().Be("Second");
            dict[record3].Should().Be("Third");
        }
    }

    // Mock repository interface for integration test
    public interface IAgentNotifyRecordRepository
    {
        void Save(AgentNotifyRecord record);
        AgentNotifyRecord GetByPolicyNumber(string policyNumber);
    }
}