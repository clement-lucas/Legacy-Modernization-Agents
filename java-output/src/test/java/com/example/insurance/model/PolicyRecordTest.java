package com.example.insurance.model;

import org.junit.jupiter.api.*;
import static org.junit.jupiter.api.Assertions.*;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.Objects;

/**
 * Comprehensive unit tests for PolicyRecord.
 * Covers constructor, getters/setters, equals/hashCode, toString, edge cases, nulls, and boundary conditions.
 * Ensures COBOL business logic and field constraints are preserved.
 */
class PolicyRecordTest {

    private PolicyRecord policyRecord;
    private PolicyRecord fullPolicyRecord;

    @BeforeEach
    void setUp() {
        policyRecord = new PolicyRecord();
        fullPolicyRecord = new PolicyRecord(
                "PN12345678", // policyNumber
                "John",       // policyHolderFname
                "A",          // policyHolderMname
                "Doe",        // policyHolderLname
                "Jane Doe",   // policyBenefName
                "Spouse",     // policyBenefRelation
                "123 Main St",// policyHolderAddr1
                "Apt 4B",     // policyHolderAddr2
                "Metropolis", // policyHolderCity
                "NY",         // policyHolderState
                "10001",      // policyHolderZipCd
                "1980-01-01", // policyHolderDob
                "Male",       // policyHolderGender
                "5551234567", // policyHolderPhone
                "john.doe@email.com", // policyHolderEmail
                "Monthly",    // policyPaymentFreq
                "ACH",        // policyPaymentMethod
                "Acme Underwriter", // policyUnderwriter
                "Standard terms and conditions apply.", // policyTermsCond
                "N",          // policyClaimed
                "DISC10",     // policyDiscountCode
                new BigDecimal("12345.67"), // policyPremiumAmount
                "Life",       // policyType
                "2024-01-01", // policyStartDate
                "2025-01-01", // policyExpiryDate
                "A",          // policyStatus
                "AGT001",     // policyAgentCode
                "Y",          // policyNotifyFlag
                "2024-01-01 12:00:00.000000", // policyAddTimestamp
                "2024-06-01 15:30:00.123456"  // policyUpdateTimestamp
        );
    }

    @AfterEach
    void tearDown() {
        policyRecord = null;
        fullPolicyRecord = null;
    }

    @Test
    void testNoArgsConstructorCreatesEmptyObject() {
        PolicyRecord empty = new PolicyRecord();
        assertNotNull(empty);
        // All fields should be null except BigDecimal, which is also null
        assertNull(empty.getPolicyNumber());
        assertNull(empty.getPolicyPremiumAmount());
    }

    @Test
    void testAllArgsConstructorSetsAllFieldsCorrectly() {
        assertEquals("PN12345678", fullPolicyRecord.getPolicyNumber());
        assertEquals("John", fullPolicyRecord.getPolicyHolderFname());
        assertEquals("A", fullPolicyRecord.getPolicyHolderMname());
        assertEquals("Doe", fullPolicyRecord.getPolicyHolderLname());
        assertEquals("Jane Doe", fullPolicyRecord.getPolicyBenefName());
        assertEquals("Spouse", fullPolicyRecord.getPolicyBenefRelation());
        assertEquals("123 Main St", fullPolicyRecord.getPolicyHolderAddr1());
        assertEquals("Apt 4B", fullPolicyRecord.getPolicyHolderAddr2());
        assertEquals("Metropolis", fullPolicyRecord.getPolicyHolderCity());
        assertEquals("NY", fullPolicyRecord.getPolicyHolderState());
        assertEquals("10001", fullPolicyRecord.getPolicyHolderZipCd());
        assertEquals("1980-01-01", fullPolicyRecord.getPolicyHolderDob());
        assertEquals("Male", fullPolicyRecord.getPolicyHolderGender());
        assertEquals("5551234567", fullPolicyRecord.getPolicyHolderPhone());
        assertEquals("john.doe@email.com", fullPolicyRecord.getPolicyHolderEmail());
        assertEquals("Monthly", fullPolicyRecord.getPolicyPaymentFreq());
        assertEquals("ACH", fullPolicyRecord.getPolicyPaymentMethod());
        assertEquals("Acme Underwriter", fullPolicyRecord.getPolicyUnderwriter());
        assertEquals("Standard terms and conditions apply.", fullPolicyRecord.getPolicyTermsCond());
        assertEquals("N", fullPolicyRecord.getPolicyClaimed());
        assertEquals("DISC10", fullPolicyRecord.getPolicyDiscountCode());
        assertEquals(new BigDecimal("12345.67"), fullPolicyRecord.getPolicyPremiumAmount());
        assertEquals("Life", fullPolicyRecord.getPolicyType());
        assertEquals("2024-01-01", fullPolicyRecord.getPolicyStartDate());
        assertEquals("2025-01-01", fullPolicyRecord.getPolicyExpiryDate());
        assertEquals("A", fullPolicyRecord.getPolicyStatus());
        assertEquals("AGT001", fullPolicyRecord.getPolicyAgentCode());
        assertEquals("Y", fullPolicyRecord.getPolicyNotifyFlag());
        assertEquals("2024-01-01 12:00:00.000000", fullPolicyRecord.getPolicyAddTimestamp());
        assertEquals("2024-06-01 15:30:00.123456", fullPolicyRecord.getPolicyUpdateTimestamp());
    }

    @Test
    void testSettersAndGettersWorkForAllFields() {
        policyRecord.setPolicyNumber("PN87654321");
        assertEquals("PN87654321", policyRecord.getPolicyNumber());

        policyRecord.setPolicyHolderFname("Alice");
        assertEquals("Alice", policyRecord.getPolicyHolderFname());

        policyRecord.setPolicyHolderMname("B");
        assertEquals("B", policyRecord.getPolicyHolderMname());

        policyRecord.setPolicyHolderLname("Smith");
        assertEquals("Smith", policyRecord.getPolicyHolderLname());

        policyRecord.setPolicyBenefName("Bob Smith");
        assertEquals("Bob Smith", policyRecord.getPolicyBenefName());

        policyRecord.setPolicyBenefRelation("Child");
        assertEquals("Child", policyRecord.getPolicyBenefRelation());

        policyRecord.setPolicyHolderAddr1("456 Elm St");
        assertEquals("456 Elm St", policyRecord.getPolicyHolderAddr1());

        policyRecord.setPolicyHolderAddr2("Suite 5C");
        assertEquals("Suite 5C", policyRecord.getPolicyHolderAddr2());

        policyRecord.setPolicyHolderCity("Gotham");
        assertEquals("Gotham", policyRecord.getPolicyHolderCity());

        policyRecord.setPolicyHolderState("CA");
        assertEquals("CA", policyRecord.getPolicyHolderState());

        policyRecord.setPolicyHolderZipCd("90210");
        assertEquals("90210", policyRecord.getPolicyHolderZipCd());

        policyRecord.setPolicyHolderDob("1990-12-31");
        assertEquals("1990-12-31", policyRecord.getPolicyHolderDob());

        policyRecord.setPolicyHolderGender("Female");
        assertEquals("Female", policyRecord.getPolicyHolderGender());

        policyRecord.setPolicyHolderPhone("5559876543");
        assertEquals("5559876543", policyRecord.getPolicyHolderPhone());

        policyRecord.setPolicyHolderEmail("alice.smith@email.com");
        assertEquals("alice.smith@email.com", policyRecord.getPolicyHolderEmail());

        policyRecord.setPolicyPaymentFreq("Annual");
        assertEquals("Annual", policyRecord.getPolicyPaymentFreq());

        policyRecord.setPolicyPaymentMethod("CreditCard");
        assertEquals("CreditCard", policyRecord.getPolicyPaymentMethod());

        policyRecord.setPolicyUnderwriter("Best Underwriter");
        assertEquals("Best Underwriter", policyRecord.getPolicyUnderwriter());

        policyRecord.setPolicyTermsCond("Special terms apply.");
        assertEquals("Special terms apply.", policyRecord.getPolicyTermsCond());

        policyRecord.setPolicyClaimed("Y");
        assertEquals("Y", policyRecord.getPolicyClaimed());

        policyRecord.setPolicyDiscountCode("DISC20");
        assertEquals("DISC20", policyRecord.getPolicyDiscountCode());

        policyRecord.setPolicyPremiumAmount(new BigDecimal("99999.99"));
        assertEquals(new BigDecimal("99999.99"), policyRecord.getPolicyPremiumAmount());

        policyRecord.setPolicyType("Health");
        assertEquals("Health", policyRecord.getPolicyType());

        policyRecord.setPolicyStartDate("2023-07-01");
        assertEquals("2023-07-01", policyRecord.getPolicyStartDate());

        policyRecord.setPolicyExpiryDate("2024-07-01");
        assertEquals("2024-07-01", policyRecord.getPolicyExpiryDate());

        policyRecord.setPolicyStatus("I");
        assertEquals("I", policyRecord.getPolicyStatus());

        policyRecord.setPolicyAgentCode("AGT002");
        assertEquals("AGT002", policyRecord.getPolicyAgentCode());

        policyRecord.setPolicyNotifyFlag("N");
        assertEquals("N", policyRecord.getPolicyNotifyFlag());

        policyRecord.setPolicyAddTimestamp("2023-07-01 08:00:00.000000");
        assertEquals("2023-07-01 08:00:00.000000", policyRecord.getPolicyAddTimestamp());

        policyRecord.setPolicyUpdateTimestamp("2023-12-31 23:59:59.999999");
        assertEquals("2023-12-31 23:59:59.999999", policyRecord.getPolicyUpdateTimestamp());
    }

    @Test
    void testEqualsAndHashCodeWithIdenticalObjects() {
        PolicyRecord another = new PolicyRecord(
                "PN12345678", "John", "A", "Doe", "Jane Doe", "Spouse",
                "123 Main St", "Apt 4B", "Metropolis", "NY", "10001", "1980-01-01", "Male",
                "5551234567", "john.doe@email.com", "Monthly", "ACH", "Acme Underwriter",
                "Standard terms and conditions apply.", "N", "DISC10", new BigDecimal("12345.67"),
                "Life", "2024-01-01", "2025-01-01", "A", "AGT001", "Y",
                "2024-01-01 12:00:00.000000", "2024-06-01 15:30:00.123456"
        );
        assertEquals(fullPolicyRecord, another);
        assertEquals(fullPolicyRecord.hashCode(), another.hashCode());
    }

    @Test
    void testEqualsAndHashCodeWithDifferentObjects() {
        PolicyRecord different = new PolicyRecord(
                "PN00000000", "Jane", "B", "Smith", "John Smith", "Parent",
                "789 Oak St", "Unit 12", "Star City", "TX", "75001", "1975-05-05", "Female",
                "5550000000", "jane.smith@email.com", "Quarterly", "Wire", "Global Underwriter",
                "Different terms.", "Y", "DISC30", new BigDecimal("54321.00"),
                "Auto", "2022-01-01", "2023-01-01", "I", "AGT003", "N",
                "2022-01-01 09:00:00.000000", "2023-01-01 10:00:00.000000"
        );
        assertNotEquals(fullPolicyRecord, different);
        assertNotEquals(fullPolicyRecord.hashCode(), different.hashCode());
    }

    @Test
    void testToStringContainsAllFields() {
        String str = fullPolicyRecord.toString();
        assertTrue(str.contains("PN12345678"));
        assertTrue(s