using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insurance.Data.Models;
using Insurance.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Insurance.Data.Models.Tests
{
    public class PolicyRepositoryTests : IDisposable
    {
        private readonly Mock<InsuranceDbContext> _dbContextMock;
        private readonly Mock<DbSet<Policy>> _dbSetMock;
        private readonly Mock<ILogger<PolicyRepository>> _loggerMock;
        private readonly PolicyRepository _repository;

        private readonly List<Policy> _policyStore;

        public PolicyRepositoryTests()
        {
            _policyStore = new List<Policy>();
            _dbSetMock = new Mock<DbSet<Policy>>();
            _dbContextMock = new Mock<InsuranceDbContext>();
            _loggerMock = new Mock<ILogger<PolicyRepository>>();

            SetupDbSetMock();

            _dbContextMock.Setup(c => c.Policies).Returns(_dbSetMock.Object);

            _repository = new PolicyRepository(_dbContextMock.Object, _loggerMock.Object);
        }

        private void SetupDbSetMock()
        {
            var queryable = _policyStore.AsQueryable();

            _dbSetMock.As<IQueryable<Policy>>().Setup(m => m.Provider).Returns(queryable.Provider);
            _dbSetMock.As<IQueryable<Policy>>().Setup(m => m.Expression).Returns(queryable.Expression);
            _dbSetMock.As<IQueryable<Policy>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            _dbSetMock.As<IQueryable<Policy>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            _dbSetMock.Setup(d => d.AsNoTracking()).Returns(_dbSetMock.Object);

            _dbSetMock.Setup(d => d.ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _policyStore.ToList());

            _dbSetMock.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keys) =>
                {
                    var policyNumber = keys[0] as string;
                    return _policyStore.FirstOrDefault(p => p.PolicyNumber == policyNumber);
                });

            _dbSetMock.Setup(d => d.AddAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Policy policy, CancellationToken _) =>
                {
                    _policyStore.Add(policy);
                    var entryMock = new Mock<EntityEntry<Policy>>();
                    entryMock.Setup(e => e.Entity).Returns(policy);
                    return entryMock.Object;
                });

            _dbSetMock.Setup(d => d.Update(It.IsAny<Policy>()))
                .Callback((Policy policy) =>
                {
                    var idx = _policyStore.FindIndex(p => p.PolicyNumber == policy.PolicyNumber);
                    if (idx >= 0)
                        _policyStore[idx] = policy;
                });

            _dbSetMock.Setup(d => d.Remove(It.IsAny<Policy>()))
                .Callback((Policy policy) =>
                {
                    _policyStore.RemoveAll(p => p.PolicyNumber == policy.PolicyNumber);
                });
        }

        public void Dispose()
        {
            _policyStore.Clear();
        }

        private Policy CreateTestPolicy(string policyNumber = "P123456789")
        {
            return new Policy
            {
                PolicyNumber = policyNumber,
                PolicyHolderFirstName = "John",
                PolicyHolderMiddleName = "A",
                PolicyHolderLastName = "Doe",
                PolicyBeneficiaryName = "Jane Doe",
                PolicyBeneficiaryRelation = "Spouse",
                PolicyHolderAddress1 = "123 Main St",
                PolicyHolderAddress2 = "Apt 4B",
                PolicyHolderCity = "Metropolis",
                PolicyHolderState = "NY",
                PolicyHolderZipCode = "10001",
                PolicyHolderDateOfBirth = "1980-01-01",
                PolicyHolderGender = "Male",
                PolicyHolderPhone = "5551234567",
                PolicyHolderEmail = "john.doe@example.com",
                PolicyPaymentFrequency = "Monthly",
                PolicyPaymentMethod = "Credit",
                PolicyUnderwriter = "Acme Insurance",
                PolicyTermsAndConditions = "Standard terms apply.",
                PolicyClaimed = "N",
                PolicyDiscountCode = "DISC10",
                PolicyPremiumAmount = 1234.56m,
                PolicyCoverageAmount = 100000m,
                PolicyType = "Life",
                PolicyStartDate = DateTime.Today.AddYears(-1),
                PolicyExpiryDate = DateTime.Today.AddYears(1),
                PolicyStatus = "A",
                PolicyAgentCode = "AGT001",
                PolicyNotifyFlag = "Y",
                PolicyAddTimestamp = DateTime.Now.AddMonths(-1),
                PolicyUpdateTimestamp = DateTime.Now
            };
        }

        [Fact]
        public async Task GetPolicyByNumberAsync_ShouldReturnPolicy_WhenPolicyExists()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _policyStore.Add(policy);

            // Act
            var result = await _repository.GetPolicyByNumberAsync(policy.PolicyNumber);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(policy);
        }

        [Fact]
        public async Task GetPolicyByNumberAsync_ShouldReturnNull_WhenPolicyDoesNotExist()
        {
            // Arrange
            var policyNumber = "NONEXISTENT";

            // Act
            var result = await _repository.GetPolicyByNumberAsync(policyNumber);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPolicyByNumberAsync_ShouldThrowDataAccessException_OnDbException()
        {
            // Arrange
            _dbSetMock.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            Func<Task> act = async () => await _repository.GetPolicyByNumberAsync("P123456789");

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage("Failed to retrieve policy P123456789*");
            _loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPoliciesAsync_ShouldReturnAllPolicies()
        {
            // Arrange
            var policy1 = CreateTestPolicy("P1");
            var policy2 = CreateTestPolicy("P2");
            _policyStore.Add(policy1);
            _policyStore.Add(policy2);

            // Act
            var result = await _repository.GetAllPoliciesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(policy1);
            result.Should().ContainEquivalentOf(policy2);
        }

        [Fact]
        public async Task GetAllPoliciesAsync_ShouldReturnEmptyList_WhenNoPoliciesExist()
        {
            // Act
            var result = await _repository.GetAllPoliciesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPoliciesAsync_ShouldThrowDataAccessException_OnDbException()
        {
            // Arrange
            _dbSetMock.Setup(d => d.ToListAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _repository.GetAllPoliciesAsync();

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage("Failed to retrieve policies*");
            _loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddPolicyAsync_ShouldAddPolicyAndReturnEntity()
        {
            // Arrange
            var policy = CreateTestPolicy();

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.AddPolicyAsync(policy);

            // Assert
            result.Should().BeEquivalentTo(policy);
            _policyStore.Should().ContainEquivalentOf(policy);
        }

        [Fact]
        public async Task AddPolicyAsync_ShouldThrowDataAccessException_OnDbException()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _dbSetMock.Setup(d => d.AddAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _repository.AddPolicyAsync(policy);

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage($"Failed to add policy {policy.PolicyNumber}*");
            _loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), policy.PolicyNumber), Times.Once);
        }

        [Fact]
        public async Task UpdatePolicyAsync_ShouldUpdatePolicyAndReturnEntity()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _policyStore.Add(policy);

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var updatedPolicy = policy with { PolicyHolderFirstName = "Jane" };

            // Act
            var result = await _repository.UpdatePolicyAsync(updatedPolicy);

            // Assert
            result.Should().BeEquivalentTo(updatedPolicy);
            _policyStore.Should().ContainEquivalentOf(updatedPolicy);
        }

        [Fact]
        public async Task UpdatePolicyAsync_ShouldThrowDataAccessException_OnDbException()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _policyStore.Add(policy);

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _repository.UpdatePolicyAsync(policy);

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage($"Failed to update policy {policy.PolicyNumber}*");
            _loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), policy.PolicyNumber), Times.Once);
        }

        [Fact]
        public async Task DeletePolicyAsync_ShouldDeletePolicyAndReturnTrue_WhenPolicyExists()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _policyStore.Add(policy);

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.DeletePolicyAsync(policy.PolicyNumber);

            // Assert
            result.Should().BeTrue();
            _policyStore.Should().NotContain(policy);
        }

        [Fact]
        public async Task DeletePolicyAsync_ShouldReturnFalse_WhenPolicyDoesNotExist()
        {
            // Arrange
            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.DeletePolicyAsync("NONEXISTENT");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeletePolicyAsync_ShouldThrowDataAccessException_OnDbException()
        {
            // Arrange
            var policy = CreateTestPolicy();
            _policyStore.Add(policy);

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _repository.DeletePolicyAsync(policy.PolicyNumber);

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage($"Failed to delete policy {policy.PolicyNumber}*");
            _loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), policy.PolicyNumber), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
        {
            // Act
            Action act = () => new PolicyRepository(null!, _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("dbContext");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act
            Action act = () => new PolicyRepository(_dbContextMock.Object, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("          ")]
        public async Task GetPolicyByNumberAsync_ShouldHandleNullOrEmptyPolicyNumber(string? policyNumber)
        {
            // Arrange
            // PolicyNumber is required in COBOL; test that repository returns null for invalid input.
            // (Assuming DB will not throw for empty, just return null.)

            // Act
            var result = await _repository.GetPolicyByNumberAsync(policyNumber!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddPolicyAsync_ShouldThrowDataAccessException_WhenPolicyIsNull()
        {
            // Arrange
            // Simulate AddAsync throwing ArgumentNullException
            _dbSetMock.Setup(d => d.AddAsync(null!, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException());

            // Act
            Func<Task> act = async () => await _repository.AddPolicyAsync(null!);

            // Assert
            await act.Should().ThrowAsync<DataAccessException>();
        }

        [Fact]
        public async Task UpdatePolicyAsync_ShouldThrowDataAccessException_WhenPolicyIsNull()
        {
            // Arrange
            // Simulate Update throwing ArgumentNullException
            _dbSetMock.Setup(d => d.Update(null!)).Throws(new ArgumentNullException());

            // Act
            Func<Task> act = async () => await _repository.UpdatePolicyAsync(null!);

            // Assert
            await act.Should().ThrowAsync<DataAccessException>();
        }

        [Fact]
        public async Task DeletePolicyAsync_ShouldThrowDataAccessException_WhenExceptionOccursDuringGet()
        {
            // Arrange
            _dbSetMock.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _repository.DeletePolicyAsync("P123456789");

            // Assert
            await act.Should().ThrowAsync<DataAccessException>()
                .WithMessage("Failed to delete policy P123456789*");
        }

        // Integration test using in-memory EF Core database
        [Fact]
        public async Task Integration_AddAndRetrievePolicy_ShouldPersistAndReturnPolicy()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new InsuranceDbContext(options);
            var logger = new Mock<ILogger<PolicyRepository>>();
            var repo = new PolicyRepository(dbContext, logger.Object);

            var policy = CreateTestPolicy("INTEGRATION1");

            // Act
            var added = await repo.AddPolicyAsync(policy);
            var retrieved = await repo.GetPolicyByNumberAsync(policy.PolicyNumber);

            // Assert
            added.Should().BeEquivalentTo(policy);
            retrieved.Should().BeEquivalentTo(policy);
        }

        [Fact]
        public async Task Integration_DeletePolicy_ShouldRemovePolicy()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new InsuranceDbContext(options);
            var logger = new Mock<ILogger<PolicyRepository>>();
            var repo = new PolicyRepository(dbContext, logger.Object);

            var policy = CreateTestPolicy("INTEGRATION2");
            await repo.AddPolicyAsync(policy);

            // Act
            var deleted = await repo.DeletePolicyAsync(policy.PolicyNumber);
            var retrieved = await repo.GetPolicyByNumberAsync(policy.PolicyNumber);

            // Assert
            deleted.Should().BeTrue();
            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task Integration_UpdatePolicy_ShouldPersistChanges()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new InsuranceDbContext(options);
            var logger = new Mock<ILogger<PolicyRepository>>();
            var repo = new PolicyRepository(dbContext, logger.Object);

            var policy = CreateTestPolicy("INTEGRATION3");
            await repo.AddPolicyAsync(policy);

            var updatedPolicy = policy with { PolicyHolderFirstName = "UpdatedName" };

            // Act
            var result = await repo.UpdatePolicyAsync(updatedPolicy);
            var retrieved = await repo.GetPolicyByNumberAsync(policy.PolicyNumber);

            // Assert
            result.PolicyHolderFirstName.Should().Be("UpdatedName");
            retrieved.PolicyHolderFirstName.Should().Be("UpdatedName");
        }

        [Fact]
        public async Task Integration_GetAllPolicies_ShouldReturnAll()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new InsuranceDbContext(options);
            var logger = new Mock<ILogger<PolicyRepository>>();
            var repo = new PolicyRepository(dbContext, logger.Object);

            var p1 = CreateTestPolicy("INTEGRATION4A");
            var p2 = CreateTestPolicy("INTEGRATION4B");
            await repo.AddPolicyAsync(p1);
            await repo.AddPolicyAsync(p2);

            // Act
            var all = await repo.GetAllPoliciesAsync();

            // Assert
            all.Should().HaveCount(2);
            all.Should().ContainEquivalentOf(p1);
            all.Should().ContainEquivalentOf(p2);
        }

        // Edge case: Policy with boundary values for string lengths
        [Fact]
        public async Task AddPolicyAsync_ShouldAcceptBoundaryStringLengths()
        {
            // Arrange
            var policy = new Policy
            {
                PolicyNumber = new string('A', 10),
                PolicyHolderFirstName = new string('B', 35),
                PolicyHolderMiddleName = "C",
                PolicyHolderLastName = new string('D', 35),
                PolicyBeneficiaryName = new string('E', 60),
                PolicyBeneficiaryRelation = new string('F', 15),
                PolicyHolderAddress1 = new string('G', 100),
                PolicyHolderAddress2 = new string('H', 100),
                PolicyHolderCity = new string('I', 30),
                PolicyHolderState = new string('J', 2),
                PolicyHolderZipCode = new string('K', 10),
                PolicyHolderDateOfBirth = new string('L', 10),
                PolicyHolderGender = new string('M', 8),
                PolicyHolderPhone = new string('N', 10),
                PolicyHolderEmail = new string('O', 30),
                PolicyPaymentFrequency = new string('P', 10),
                PolicyPaymentMethod = new string('Q', 8),
                PolicyUnderwriter = new string('R', 50),
                PolicyTermsAndConditions = new string('S', 200),
                PolicyClaimed = "Y",
                PolicyDiscountCode = new string('T', 10),
                PolicyPremiumAmount = 9999999.99m,
                PolicyCoverageAmount = 9999999999.99m,
                PolicyType = new string('U', 50),
                PolicyStartDate = DateTime.Today,
                PolicyExpiryDate = DateTime.Today.AddYears(1),
                PolicyStatus = "A",
                PolicyAgentCode = new string('V', 10),
                PolicyNotifyFlag = "N",
                PolicyAddTimestamp = DateTime.Now,
                PolicyUpdateTimestamp = DateTime.Now
            };

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.AddPolicyAsync(policy);

            // Assert
            result.Should().BeEquivalentTo(policy);
        }

        // Edge case: Policy with minimum values
        [Fact]
        public async Task AddPolicyAsync_ShouldAcceptMinimumValues()
        {
            // Arrange
            var policy = new Policy
            {
                PolicyNumber = "1",
                PolicyHolderFirstName = "A",
                PolicyHolderMiddleName = "B",
                PolicyHolderLastName = "C",
                PolicyBeneficiaryName = "D",
                PolicyBeneficiaryRelation = "E",
                PolicyHolderAddress1 = "F",
                PolicyHolderAddress2 = "G",
                PolicyHolderCity = "H",
                PolicyHolderState = "I",
                PolicyHolderZipCode = "J",
                PolicyHolderDateOfBirth = "K",
                PolicyHolderGender = "L",
                PolicyHolderPhone = "M",
                PolicyHolderEmail = "N",
                PolicyPaymentFrequency = "O",
                PolicyPaymentMethod = "P",
                PolicyUnderwriter = "Q",
                PolicyTermsAndConditions = "R",
                PolicyClaimed = "S",
                PolicyDiscountCode = "T",
                PolicyPremiumAmount = 0.01m,
                PolicyCoverageAmount = 0.01m,
                PolicyType = "U",
                PolicyStartDate = DateTime.MinValue,
                PolicyExpiryDate = DateTime.MinValue,
                PolicyStatus = "V",
                PolicyAgentCode = "W",
                PolicyNotifyFlag = "X",
                PolicyAddTimestamp = DateTime.MinValue,
                PolicyUpdateTimestamp = DateTime.MinValue
            };

            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.AddPolicyAsync(policy);

            // Assert
            result.Should().BeEquivalentTo(policy);
        }
    }
}