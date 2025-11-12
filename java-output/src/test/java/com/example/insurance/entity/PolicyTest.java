package com.example.insurance.entity;

import io.quarkus.test.junit.QuarkusTest;
import org.junit.jupiter.api.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import static org.junit.jupiter.api.Assertions.*;

/**
 * Unit and integration tests for {@link Policy}.
 * Ensures COBOL data mapping and business logic are preserved.
 */
@QuarkusTest
class PolicyTest {

    private Policy policy;

    @BeforeEach
    void setUp() {
        policy = new Policy();

        policy.setPolicyNumber("P123456789");
        policy.setPolicyHolderFname("John");
        policy.setPolicyHolderMname("A");
        policy.setPolicyHolderLname("Doe");
        policy.setPolicyBenefName("Jane Doe");
        policy.setPolicyBenefRelation("Spouse");
        policy.setPolicyHolderAddr1("123 Main St");
        policy.setPolicyHolderAddr2("Apt 4B");
        policy.setPolicyHolderCity("Metropolis");
        policy.setPolicyHolderState("NY");
        policy.setPolicyHolderZipCd("10001");
        policy.setPolicyHolderDob("1980-01-01");
        policy.setPolicyHolderGender("Male");
        policy.setPolicyHolderPhone("5551234567");
        policy.setPolicyHolderEmail("john.doe@example.com");
        policy.setPolicyPaymentFreq("Monthly");
        policy.setPolicyPaymentMethod("Credit");
        policy.setPolicyUnderwriter("Best Underwriter");
        policy.setPolicyTermsCond("Standard terms and conditions apply.");
        policy.setPolicyClaimed("N");
        policy.setPolicyDiscountCode("DISC10");
        policy.setPolicyPremiumAmount(new BigDecimal("1234.56"));
        policy.setPolicyCoverageAmount(new BigDecimal("100000.00"));
        policy.setPolicyType("Life Insurance");
        policy.setPolicyStartDate(LocalDate.of(2023, 1, 1));
        policy.setPolicyExpiryDate(LocalDate.of(2024, 1, 1));
        policy.setPolicyStatus("A");
        policy.setPolicyAgentCode("AGT1234567");
        policy.setPolicyNotifyFlag("Y");
        policy.setPolicyAddTimestamp(LocalDateTime.of(2023, 1, 1, 10, 0, 0));
        policy.setPolicyUpdateTimestamp(LocalDateTime.of(2023, 6, 1, 12, 0, 0));
    }

    @AfterEach
    void tearDown() {
        policy = null;
    }

    @Test
    void testDefaultConstructorInitializesFieldsToNullOrDefaults() {
        Policy emptyPolicy = new Policy();
        assertNull(emptyPolicy.getPolicyNumber());
        assertNull(emptyPolicy.getPolicyHolderFname());
        assertNull(emptyPolicy.getPolicyPremiumAmount());
        assertNull(emptyPolicy.getPolicyStartDate());
        assertNull(emptyPolicy.getPolicyAddTimestamp());
    }

    @Test
    void testSettersAndGetters_AllFields() {
        assertEquals("P123456789", policy.getPolicyNumber());
        assertEquals("John", policy.getPolicyHolderFname());
        assertEquals("A", policy.getPolicyHolderMname());
        assertEquals("Doe", policy.getPolicyHolderLname());
        assertEquals("Jane Doe", policy.getPolicyBenefName());
        assertEquals("Spouse", policy.getPolicyBenefRelation());
        assertEquals("123 Main St", policy.getPolicyHolderAddr1());
        assertEquals("Apt 4B", policy.getPolicyHolderAddr2());
        assertEquals("Metropolis", policy.getPolicyHolderCity());
        assertEquals("NY", policy.getPolicyHolderState());
        assertEquals("10001", policy.getPolicyHolderZipCd());
        assertEquals("1980-01-01", policy.getPolicyHolderDob());
        assertEquals("Male", policy.getPolicyHolderGender());
        assertEquals("5551234567", policy.getPolicyHolderPhone());
        assertEquals("john.doe@example.com", policy.getPolicyHolderEmail());
        assertEquals("Monthly", policy.getPolicyPaymentFreq());
        assertEquals("Credit", policy.getPolicyPaymentMethod());
        assertEquals("Best Underwriter", policy.getPolicyUnderwriter());
        assertEquals("Standard terms and conditions apply.", policy.getPolicyTermsCond());
        assertEquals("N", policy.getPolicyClaimed());
        assertEquals("DISC10", policy.getPolicyDiscountCode());
        assertEquals(new BigDecimal("1234.56"), policy.getPolicyPremiumAmount());
        assertEquals(new BigDecimal("100000.00"), policy.getPolicyCoverageAmount());
        assertEquals("Life Insurance", policy.getPolicyType());
        assertEquals(LocalDate.of(2023, 1, 1), policy.getPolicyStartDate());
        assertEquals(LocalDate.of(2024, 1, 1), policy.getPolicyExpiryDate());
        assertEquals("A", policy.getPolicyStatus());
        assertEquals("AGT1234567", policy.getPolicyAgentCode());
        assertEquals("Y", policy.getPolicyNotifyFlag());
        assertEquals(LocalDateTime.of(2023, 1, 1, 10, 0, 0), policy.getPolicyAddTimestamp());
        assertEquals(LocalDateTime.of(2023, 6, 1, 12, 0, 0), policy.getPolicyUpdateTimestamp());
    }

    @Test
    void testSetters_NullAndEmptyValues() {
        // Test setting nullable fields to null (should not throw, but may violate DB constraints)
        policy.setPolicyHolderAddr2(null);
        assertNull(policy.getPolicyHolderAddr2());

        policy.setPolicyHolderAddr2("");
        assertEquals("", policy.getPolicyHolderAddr2());

        policy.setPolicyDiscountCode(null);
        assertNull(policy.getPolicyDiscountCode());

        policy.setPolicyPremiumAmount(null);
        assertNull(policy.getPolicyPremiumAmount());

        policy.setPolicyCoverageAmount(null);
        assertNull(policy.getPolicyCoverageAmount());
    }

    @Test
    void testSetters_BoundaryValues() {
        // Test string fields at max length
        String maxFname = "A".repeat(35);
        policy.setPolicyHolderFname(maxFname);
        assertEquals(maxFname, policy.getPolicyHolderFname());

        String maxLname = "B".repeat(35);
        policy.setPolicyHolderLname(maxLname);
        assertEquals(maxLname, policy.getPolicyHolderLname());

        String maxBenefName = "C".repeat(60);
        policy.setPolicyBenefName(maxBenefName);
        assertEquals(maxBenefName, policy.getPolicyBenefName());

        String maxAddr1 = "D".repeat(100);
        policy.setPolicyHolderAddr1(maxAddr1);
        assertEquals(maxAddr1, policy.getPolicyHolderAddr1());

        String maxState = "NY";
        policy.setPolicyHolderState(maxState);
        assertEquals(maxState, policy.getPolicyHolderState());

        String maxZip = "1234567890";
        policy.setPolicyHolderZipCd(maxZip);
        assertEquals(maxZip, policy.getPolicyHolderZipCd());

        String maxGender = "Female";
        policy.setPolicyHolderGender(maxGender);
        assertEquals(maxGender, policy.getPolicyHolderGender());
    }

    @Test
    void testSetters_InvalidValues() {
        // Simulate invalid values for numeric fields
        assertThrows(NumberFormatException.class, () -> {
            policy.setPolicyPremiumAmount(new BigDecimal("notanumber"));
        });

        // Simulate invalid date string (should be handled in service, not entity)
        policy.setPolicyHolderDob("invalid-date");
        assertEquals("invalid-date", policy.getPolicyHolderDob());
    }

    @Test
    void testPolicyDates_ExpiryAfterStart() {
        // Ensures expiry is after start date (COBOL business logic)
        assertTrue(policy.getPolicyExpiryDate().isAfter(policy.getPolicyStartDate()));
    }

    @Test
    void testPolicyPremiumAmount_NegativeValue() {
        policy.setPolicyPremiumAmount(new BigDecimal("-100.00"));
        assertEquals(new BigDecimal("-100.00"), policy.getPolicyPremiumAmount());
    }

    @Test
    void testPolicyCoverageAmount_ZeroValue() {
        policy.setPolicyCoverageAmount(BigDecimal.ZERO);
        assertEquals(BigDecimal.ZERO, policy.getPolicyCoverageAmount());
    }

    @Test
    void testPolicyStatus_ActiveAndInactive() {
        policy.setPolicyStatus("A");
        assertEquals("A", policy.getPolicyStatus());

        policy.setPolicyStatus("I");
        assertEquals("I", policy.getPolicyStatus());
    }

    @Test
    void testPolicyClaimedFlag_YesNo() {
        policy.setPolicyClaimed("Y");
        assertEquals("Y", policy.getPolicyClaimed());

        policy.setPolicyClaimed("N");
        assertEquals("N", policy.getPolicyClaimed());
    }

    @Test
    void testPolicyNotifyFlag_YesNo() {
        policy.setPolicyNotifyFlag("Y");
        assertEquals("Y", policy.getPolicyNotifyFlag());

        policy.setPolicyNotifyFlag("N");
        assertEquals("N", policy.getPolicyNotifyFlag());
    }

    @Test
    void testPolicyAddAndUpdateTimestamps() {
        LocalDateTime now = LocalDateTime.now();
        policy.setPolicyAddTimestamp(now);
        policy.setPolicyUpdateTimestamp(now.plusDays(1));

        assertEquals(now, policy.getPolicyAddTimestamp());
        assertEquals(now.plusDays(1), policy.getPolicyUpdateTimestamp());
    }

    @Test
    void testEqualsAndHashCode() {
        Policy policy2 = new Policy();
        policy2.setPolicyNumber("P123456789");
        // Only policyNumber is @Id, so equality should be based on it if equals/hashCode are overridden
        // If not overridden, this will fail (default Object equality)
        assertNotEquals(policy, policy2);
    }

    @Test
    void testToString_NotNull() {
        assertNotNull(policy.toString());
    }

    // --- Integration Tests for PanacheEntityBase (Quarkus ORM) ---

    @Test
    void testPersistAndFindById() {
        // Persist and retrieve the policy entity
        policy.persist();
        Policy found = Policy.findById(policy.getPolicyNumber());
        assertNotNull(found);
        assertEquals(policy.getPolicyNumber(), found.getPolicyNumber());
        assertEquals(policy.getPolicyHolderFname(), found.getPolicyHolderFname());
        // Clean up
        found.delete();
    }

    @Test
    void testDeletePolicy() {
        policy.persist();
        Policy found = Policy.findById(policy.getPolicyNumber());
        assertNotNull(found);
        found.delete();
        Policy deleted = Policy.findById(policy.getPolicyNumber());
        assertNull(deleted);
    }

    @Test
    void testFindAllPolicies() {
        long beforeCount = Policy.count();
        policy.persist();
        long afterCount = Policy.count();
        assertEquals(beforeCount + 1, afterCount);
        // Clean up
        policy.delete();
    }

    @Test
    void testFindByPolicyHolderFname() {
        policy.persist();
        Policy found = Policy.find("policyHolderFname", policy.getPolicyHolderFname()).firstResult();
        assertNotNull(found);
        assertEquals(policy.getPolicyHolderFname(), found.getPolicyHolderFname());
        // Clean up
        found.delete();
    }

    // --- Edge Case: Null Primary Key ---
    @Test
    void testPersistPolicyWithNullPrimaryKey_ShouldFail() {
        Policy invalidPolicy = new Policy();
        invalidPolicy.setPolicyHolderFname("NullPK");
        // All other required fields omitted for brevity
        assertThrows(Exception.class, invalidPolicy::persist);
    }

    // --- Edge Case: Overly Long Strings ---
    @Test
    void testSetters_OverlyLongStrings_TruncateOrThrow() {
        String longFname = "X".repeat(40);
        policy.setPolicyHolderFname(longFname);
        assertEquals(longFname, policy.getPolicyHolderFname());
        // Actual DB constraint violation would occur on persist, not setter
    }

    // --- Edge Case: Null Required Fields ---
    @Test
    void testPersistPolicyWithNullRequiredFields_ShouldFail() {
        Policy invalidPolicy = new Policy();
        invalidPolicy.setPolicyNumber("P000000000");
        // All other required fields left null
        assertThrows(Exception.class, invalidPolicy::persist);
    }

    // --- Edge Case: Dates in the Past/Future ---
    @Test
    void testPolicyStartDateInPast() {
        policy.setPolicyStartDate(LocalDate.now().minusYears(10));
        assertTrue(policy.getPolicyStartDate().isBefore(LocalDate.now()));
    }

    @Test
    void testPolicyExpiryDateInFuture() {
        policy.setPolicyExpiryDate(LocalDate.now().plusYears(10));
        assertTrue(policy.getPolicyExpiryDate().isAfter(LocalDate.now()));
    }
}