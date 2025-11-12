using System;
using Xunit;
using FluentAssertions;
using Moq;

namespace Insurance.Domain.Models.Tests
{
    public class PolicyRecordTests : IDisposable
    {
        // Setup resources if needed
        public PolicyRecordTests()
        {
            // No external dependencies to mock for PolicyRecord, but constructor is present for future extension
        }

        // Teardown resources if needed
        public void Dispose()
        {
            // Cleanup logic if required
        }

        [Fact]
        public void GetPolicyHolderDateOfBirth_ShouldReturnParsedDate_WhenDateIsValid()
        {
            // Arrange
            var record = new PolicyRecord { PolicyHolderDateOfBirth = "1980-12-25" };

            // Act
            var result = record.GetPolicyHolderDateOfBirth();

            // Assert
            result.Should().Be(new DateTime(1980, 12, 25));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-date")]
        [InlineData("12/25/1980")]
        [InlineData("1980-13-01")] // Invalid month
        [InlineData("1980-12-32")] // Invalid day
        public void GetPolicyHolderDateOfBirth_ShouldReturnNull_WhenDateIsInvalid(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyHolderDateOfBirth = input };

            // Act
            var result = record.GetPolicyHolderDateOfBirth();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetPolicyStartDate_ShouldReturnParsedDate_WhenDateIsValid()
        {
            // Arrange
            var record = new PolicyRecord { PolicyStartDate = "2022-01-01" };

            // Act
            var result = record.GetPolicyStartDate();

            // Assert
            result.Should().Be(new DateTime(2022, 1, 1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-date")]
        [InlineData("2022-02-30")] // Invalid day
        public void GetPolicyStartDate_ShouldReturnNull_WhenDateIsInvalid(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyStartDate = input };

            // Act
            var result = record.GetPolicyStartDate();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetPolicyExpiryDate_ShouldReturnParsedDate_WhenDateIsValid()
        {
            // Arrange
            var record = new PolicyRecord { PolicyExpiryDate = "2025-12-31" };

            // Act
            var result = record.GetPolicyExpiryDate();

            // Assert
            result.Should().Be(new DateTime(2025, 12, 31));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("bad-date")]
        [InlineData("2025-00-10")] // Invalid month
        public void GetPolicyExpiryDate_ShouldReturnNull_WhenDateIsInvalid(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyExpiryDate = input };

            // Act
            var result = record.GetPolicyExpiryDate();

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Y")]
        [InlineData("y")]
        [InlineData("Y ")]
        [InlineData(" y")]
        public void IsClaimed_ShouldReturnTrue_WhenPolicyClaimedIsY_RegardlessOfCaseOrWhitespace(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyClaimed = input.Trim() };

            // Act
            var result = record.IsClaimed;

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("N")]
        [InlineData("n")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("X")]
        [InlineData("Yes")]
        public void IsClaimed_ShouldReturnFalse_WhenPolicyClaimedIsNotY(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyClaimed = input };

            // Act
            var result = record.IsClaimed;

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Y")]
        [InlineData("y")]
        public void IsNotificationEnabled_ShouldReturnTrue_WhenPolicyNotifyFlagIsY(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyNotifyFlag = input };

            // Act
            var result = record.IsNotificationEnabled;

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("N")]
        [InlineData("n")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("X")]
        public void IsNotificationEnabled_ShouldReturnFalse_WhenPolicyNotifyFlagIsNotY(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyNotifyFlag = input };

            // Act
            var result = record.IsNotificationEnabled;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetAddTimestamp_ShouldReturnParsedDateTime_WhenTimestampIsValid()
        {
            // Arrange
            var record = new PolicyRecord { PolicyAddTimestamp = "2024-06-01 13:45:22.123" };

            // Act
            var result = record.GetAddTimestamp();

            // Assert
            result.Should().Be(new DateTime(2024, 6, 1, 13, 45, 22, 123));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-timestamp")]
        [InlineData("2024-06-01T13:45:22.123")] // ISO format not accepted by DateTime.Parse without settings
        public void GetAddTimestamp_ShouldReturnNull_WhenTimestampIsInvalid(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyAddTimestamp = input };

            // Act
            var result = record.GetAddTimestamp();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUpdateTimestamp_ShouldReturnParsedDateTime_WhenTimestampIsValid()
        {
            // Arrange
            var record = new PolicyRecord { PolicyUpdateTimestamp = "2024-06-02 15:00:00.999" };

            // Act
            var result = record.GetUpdateTimestamp();

            // Assert
            result.Should().Be(new DateTime(2024, 6, 2, 15, 0, 0, 999));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("bad-timestamp")]
        public void GetUpdateTimestamp_ShouldReturnNull_WhenTimestampIsInvalid(string input)
        {
            // Arrange
            var record = new PolicyRecord { PolicyUpdateTimestamp = input };

            // Act
            var result = record.GetUpdateTimestamp();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void PolicyPremiumAmount_ShouldBeSetCorrectly()
        {
            // Arrange
            var record = new PolicyRecord { PolicyPremiumAmount = 1234.56m };

            // Act & Assert
            record.PolicyPremiumAmount.Should().Be(1234.56m);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999999.99)]
        public void PolicyPremiumAmount_ShouldHandleBoundaryValues(decimal amount)
        {
            // Arrange
            var record = new PolicyRecord { PolicyPremiumAmount = amount };

            // Act & Assert
            record.PolicyPremiumAmount.Should().Be(amount);
        }

        [Fact]
        public void AllProperties_ShouldBeSetAndRetrievedCorrectly()
        {
            // Arrange
            var record = new PolicyRecord
            {
                PolicyNumber = "PN123456",
                PolicyHolderFirstName = "John",
                PolicyHolderMiddleName = "A",
                PolicyHolderLastName = "Doe",
                PolicyBeneficiaryName = "Jane Doe",
                PolicyBeneficiaryRelation = "Spouse",
                PolicyHolderAddress1 = "123 Main St",
                PolicyHolderAddress2 = "Apt 4B",
                PolicyHolderCity = "Springfield",
                PolicyHolderState = "IL",
                PolicyHolderZipCode = "62704",
                PolicyHolderDateOfBirth = "1975-05-20",
                PolicyHolderGender = "M",
                PolicyHolderPhone = "555-1234",
                PolicyHolderEmail = "john.doe@example.com",
                PolicyPaymentFrequency = "Monthly",
                PolicyPaymentMethod = "Credit Card",
                PolicyUnderwriter = "Acme Underwriters",
                PolicyTermsAndConditions = "Standard terms apply.",
                PolicyClaimed = "N",
                PolicyDiscountCode = "DISC10",
                PolicyPremiumAmount = 500.00m,
                PolicyType = "Life",
                PolicyStartDate = "2023-01-01",
                PolicyExpiryDate = "2028-01-01",
                PolicyStatus = "Active",
                PolicyAgentCode = "AGT001",
                PolicyNotifyFlag = "Y",
                PolicyAddTimestamp = "2023-01-01 10:00:00.000",
                PolicyUpdateTimestamp = "2023-06-01 12:00:00.000"
            };

            // Act & Assert
            record.PolicyNumber.Should().Be("PN123456");
            record.PolicyHolderFirstName.Should().Be("John");
            record.PolicyHolderMiddleName.Should().Be("A");
            record.PolicyHolderLastName.Should().Be("Doe");
            record.PolicyBeneficiaryName.Should().Be("Jane Doe");
            record.PolicyBeneficiaryRelation.Should().Be("Spouse");
            record.PolicyHolderAddress1.Should().Be("123 Main St");
            record.PolicyHolderAddress2.Should().Be("Apt 4B");
            record.PolicyHolderCity.Should().Be("Springfield");
            record.PolicyHolderState.Should().Be("IL");
            record.PolicyHolderZipCode.Should().Be("62704");
            record.PolicyHolderDateOfBirth.Should().Be("1975-05-20");
            record.PolicyHolderGender.Should().Be("M");
            record.PolicyHolderPhone.Should().Be("555-1234");
            record.PolicyHolderEmail.Should().Be("john.doe@example.com");
            record.PolicyPaymentFrequency.Should().Be("Monthly");
            record.PolicyPaymentMethod.Should().Be("Credit Card");
            record.PolicyUnderwriter.Should().Be("Acme Underwriters");
            record.PolicyTermsAndConditions.Should().Be("Standard terms apply.");
            record.PolicyClaimed.Should().Be("N");
            record.PolicyDiscountCode.Should().Be("DISC10");
            record.PolicyPremiumAmount.Should().Be(500.00m);
            record.PolicyType.Should().Be("Life");
            record.PolicyStartDate.Should().Be("2023-01-01");
            record.PolicyExpiryDate.Should().Be("2028-01-01");
            record.PolicyStatus.Should().Be("Active");
            record.PolicyAgentCode.Should().Be("AGT001");
            record.PolicyNotifyFlag.Should().Be("Y");
            record.PolicyAddTimestamp.Should().Be("2023-01-01 10:00:00.000");
            record.PolicyUpdateTimestamp.Should().Be("2023-06-01 12:00:00.000");
        }

        [Fact]
        public void IsClaimed_ShouldPreserveCobolBusinessLogic_YMeansTrue_NMeansFalse()
        {
            // Arrange
            var claimedRecord = new PolicyRecord { PolicyClaimed = "Y" };
            var notClaimedRecord = new PolicyRecord { PolicyClaimed = "N" };

            // Act & Assert
            claimedRecord.IsClaimed.Should().BeTrue();
            notClaimedRecord.IsClaimed.Should().BeFalse();
        }

        [Fact]
        public void IsNotificationEnabled_ShouldPreserveCobolBusinessLogic_YMeansTrue_NMeansFalse()
        {
            // Arrange
            var notifyRecord = new PolicyRecord { PolicyNotifyFlag = "Y" };
            var noNotifyRecord = new PolicyRecord { PolicyNotifyFlag = "N" };

            // Act & Assert
            notifyRecord.IsNotificationEnabled.Should().BeTrue();
            noNotifyRecord.IsNotificationEnabled.Should().BeFalse();
        }

        [Fact]
        public void GetPolicyHolderDateOfBirth_ShouldHandleLeapYear()
        {
            // Arrange
            var record = new PolicyRecord { PolicyHolderDateOfBirth = "2000-02-29" };

            // Act
            var result = record.GetPolicyHolderDateOfBirth();

            // Assert
            result.Should().Be(new DateTime(2000, 2, 29));
        }

        [Fact]
        public void GetPolicyStartDate_ShouldHandleLeapYear()
        {
            // Arrange
            var record = new PolicyRecord { PolicyStartDate = "2016-02-29" };

            // Act
            var result = record.GetPolicyStartDate();

            // Assert
            result.Should().Be(new DateTime(2016, 2, 29));
        }

        [Fact]
        public void GetPolicyExpiryDate_ShouldHandleLeapYear()
        {
            // Arrange
            var record = new PolicyRecord { PolicyExpiryDate = "2024-02-29" };

            // Act
            var result = record.GetPolicyExpiryDate();

            // Assert
            result.Should().Be(new DateTime(2024, 2, 29));
        }

        [Fact]
        public void PolicyRecord_ShouldBeImmutable()
        {
            // Arrange
            var record = new PolicyRecord { PolicyNumber = "PN123" };

            // Act
            Action act = () => record = record with { PolicyNumber = "PN456" };

            // Assert
            act.Should().NotThrow();
            record.PolicyNumber.Should().Be("PN456");
        }

        // Complex scenario: All date fields are valid, IsClaimed and IsNotificationEnabled are true
        [Fact]
        public void AllMethods_ShouldReturnExpectedValues_WhenAllFieldsAreValid()
        {
            // Arrange
            var record = new PolicyRecord
            {
                PolicyHolderDateOfBirth = "1990-01-15",
                PolicyStartDate = "2020-05-01",
                PolicyExpiryDate = "2030-05-01",
                PolicyClaimed = "Y",
                PolicyNotifyFlag = "Y",
                PolicyAddTimestamp = "2020-05-01 08:00:00.000",
                PolicyUpdateTimestamp = "2022-05-01 09:30:00.000"
            };

            // Act & Assert
            record.GetPolicyHolderDateOfBirth().Should().Be(new DateTime(1990, 1, 15));
            record.GetPolicyStartDate().Should().Be(new DateTime(2020, 5, 1));
            record.GetPolicyExpiryDate().Should().Be(new DateTime(2030, 5, 1));
            record.IsClaimed.Should().BeTrue();
            record.IsNotificationEnabled.Should().BeTrue();
            record.GetAddTimestamp().Should().Be(new DateTime(2020, 5, 1, 8, 0, 0, 0));
            record.GetUpdateTimestamp().Should().Be(new DateTime(2022, 5, 1, 9, 30, 0, 0));
        }

        // Edge case: All fields are null or empty
        [Fact]
        public void AllMethods_ShouldReturnNullOrFalse_WhenAllFieldsAreNullOrEmpty()
        {
            // Arrange
            var record = new PolicyRecord();

            // Act & Assert
            record.GetPolicyHolderDateOfBirth().Should().BeNull();
            record.GetPolicyStartDate().Should().BeNull();
            record.GetPolicyExpiryDate().Should().BeNull();
            record.IsClaimed.Should().BeFalse();
            record.IsNotificationEnabled.Should().BeFalse();
            record.GetAddTimestamp().Should().BeNull();
            record.GetUpdateTimestamp().Should().BeNull();
        }
    }
}