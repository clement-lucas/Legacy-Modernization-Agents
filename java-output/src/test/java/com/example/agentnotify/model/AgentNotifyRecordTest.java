package com.example.agentnotify.model;

import io.quarkus.test.junit.QuarkusTest;
import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validation;
import jakarta.validation.Validator;
import jakarta.validation.ValidatorFactory;
import org.junit.jupiter.api.*;
import org.mockito.Mockito;

import java.util.Set;

import static org.junit.jupiter.api.Assertions.*;

@QuarkusTest
class AgentNotifyRecordTest {

    private Validator validator;

    @BeforeEach
    void setUp() {
        ValidatorFactory factory = Validation.buildDefaultValidatorFactory();
        validator = factory.getValidator();
    }

    @AfterEach
    void tearDown() {
        validator = null;
    }

    /**
     * Helper method to create a valid AgentNotifyRecord instance.
     */
    private AgentNotifyRecord createValidRecord() {
        return new AgentNotifyRecord(
                "AGT1234567", // agentCode (max 10)
                "John Doe", // agentName (max 45)
                "123 Main St", // agentAddress1 (max 50)
                "Suite 100", // agentAddress2 (max 50)
                "Metropolis", // agentCity (max 20)
                "NY", // agentState (max 2)
                "POL1234567", // policyNumber (max 10)
                "Jane", // policyHolderFName (max 35)
                "A", // policyHolderMName (max 1)
                "Smith", // policyHolderLName (max 35)
                "2024-06-01", // policyStartDate (max 10)
                "2025-06-01", // policyExpiryDate (max 10)
                "2024-06-15", // notifyDate (max 10)
                "Policy renewal notification" // notifyMessages (max 100)
        );
    }

    @Test
    void testNoArgsConstructorCreatesEmptyRecord() {
        AgentNotifyRecord record = new AgentNotifyRecord();
        assertNull(record.getAgentCode());
        assertNull(record.getAgentName());
        assertNull(record.getAgentAddress1());
        assertNull(record.getAgentAddress2());
        assertNull(record.getAgentCity());
        assertNull(record.getAgentState());
        assertNull(record.getPolicyNumber());
        assertNull(record.getPolicyHolderFName());
        assertNull(record.getPolicyHolderMName());
        assertNull(record.getPolicyHolderLName());
        assertNull(record.getPolicyStartDate());
        assertNull(record.getPolicyExpiryDate());
        assertNull(record.getNotifyDate());
        assertNull(record.getNotifyMessages());
    }

    @Test
    void testAllArgsConstructorSetsAllFieldsCorrectly() {
        AgentNotifyRecord record = createValidRecord();
        assertEquals("AGT1234567", record.getAgentCode());
        assertEquals("John Doe", record.getAgentName());
        assertEquals("123 Main St", record.getAgentAddress1());
        assertEquals("Suite 100", record.getAgentAddress2());
        assertEquals("Metropolis", record.getAgentCity());
        assertEquals("NY", record.getAgentState());
        assertEquals("POL1234567", record.getPolicyNumber());
        assertEquals("Jane", record.getPolicyHolderFName());
        assertEquals("A", record.getPolicyHolderMName());
        assertEquals("Smith", record.getPolicyHolderLName());
        assertEquals("2024-06-01", record.getPolicyStartDate());
        assertEquals("2025-06-01", record.getPolicyExpiryDate());
        assertEquals("2024-06-15", record.getNotifyDate());
        assertEquals("Policy renewal notification", record.getNotifyMessages());
    }

    @Test
    void testSettersAndGettersWorkCorrectly() {
        AgentNotifyRecord record = new AgentNotifyRecord();
        record.setAgentCode("AGT1234567");
        record.setAgentName("John Doe");
        record.setAgentAddress1("123 Main St");
        record.setAgentAddress2("Suite 100");
        record.setAgentCity("Metropolis");
        record.setAgentState("NY");
        record.setPolicyNumber("POL1234567");
        record.setPolicyHolderFName("Jane");
        record.setPolicyHolderMName("A");
        record.setPolicyHolderLName("Smith");
        record.setPolicyStartDate("2024-06-01");
        record.setPolicyExpiryDate("2025-06-01");
        record.setNotifyDate("2024-06-15");
        record.setNotifyMessages("Policy renewal notification");

        assertEquals("AGT1234567", record.getAgentCode());
        assertEquals("John Doe", record.getAgentName());
        assertEquals("123 Main St", record.getAgentAddress1());
        assertEquals("Suite 100", record.getAgentAddress2());
        assertEquals("Metropolis", record.getAgentCity());
        assertEquals("NY", record.getAgentState());
        assertEquals("POL1234567", record.getPolicyNumber());
        assertEquals("Jane", record.getPolicyHolderFName());
        assertEquals("A", record.getPolicyHolderMName());
        assertEquals("Smith", record.getPolicyHolderLName());
        assertEquals("2024-06-01", record.getPolicyStartDate());
        assertEquals("2025-06-01", record.getPolicyExpiryDate());
        assertEquals("2024-06-15", record.getNotifyDate());
        assertEquals("Policy renewal notification", record.getNotifyMessages());
    }

    @Test
    void testEqualsAndHashCodeForIdenticalObjects() {
        AgentNotifyRecord record1 = createValidRecord();
        AgentNotifyRecord record2 = createValidRecord();
        assertEquals(record1, record2);
        assertEquals(record1.hashCode(), record2.hashCode());
    }

    @Test
    void testEqualsAndHashCodeForDifferentObjects() {
        AgentNotifyRecord record1 = createValidRecord();
        AgentNotifyRecord record2 = createValidRecord();
        record2.setAgentCode("DIFFERENT");
        assertNotEquals(record1, record2);
        assertNotEquals(record1.hashCode(), record2.hashCode());
    }

    @Test
    void testToStringContainsFieldValues() {
        AgentNotifyRecord record = createValidRecord();
        String str = record.toString();
        assertTrue(str.contains("AGT1234567"));
        assertTrue(str.contains("John Doe"));
        assertTrue(str.contains("POL1234567"));
        assertTrue(str.contains("Policy renewal notification"));
    }

    @Test
    void testValidationSucceedsForValidRecord() {
        AgentNotifyRecord record = createValidRecord();
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    @Test
    void testValidationFailsForBlankAgentCode() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentCode("");
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testValidationFailsForNullAgentCode() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentCode(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testValidationFailsForAgentCodeTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentCode("ABCDEFGHIJK"); // 11 chars, max is 10
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testValidationFailsForBlankAgentName() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentName("   ");
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testValidationFailsForNullAgentName() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentName(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testValidationFailsForAgentNameTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentName("A".repeat(46)); // 46 chars, max is 45
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testValidationFailsForAgentAddress1TooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentAddress1("A".repeat(51)); // 51 chars, max is 50
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentAddress1")));
    }

    @Test
    void testValidationFailsForAgentAddress2TooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentAddress2("B".repeat(51)); // 51 chars, max is 50
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentAddress2")));
    }

    @Test
    void testValidationFailsForAgentCityTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentCity("C".repeat(21)); // 21 chars, max is 20
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCity")));
    }

    @Test
    void testValidationFailsForAgentStateTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentState("XYZ"); // 3 chars, max is 2
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentState")));
    }

    @Test
    void testValidationFailsForBlankPolicyNumber() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyNumber("");
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyNumber")));
    }

    @Test
    void testValidationFailsForNullPolicyNumber() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyNumber(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyNumber")));
    }

    @Test
    void testValidationFailsForPolicyNumberTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyNumber("12345678901"); // 11 chars, max is 10
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyNumber")));
    }

    @Test
    void testValidationFailsForPolicyHolderFNameTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyHolderFName("D".repeat(36)); // 36 chars, max is 35
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyHolderFName")));
    }

    @Test
    void testValidationFailsForPolicyHolderMNameTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyHolderMName("AB"); // 2 chars, max is 1
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyHolderMName")));
    }

    @Test
    void testValidationFailsForPolicyHolderLNameTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyHolderLName("E".repeat(36)); // 36 chars, max is 35
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyHolderLName")));
    }

    @Test
    void testValidationFailsForPolicyStartDateTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyStartDate("2024-06-011"); // 11 chars, max is 10
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyStartDate")));
    }

    @Test
    void testValidationFailsForPolicyExpiryDateTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyExpiryDate("2025-06-011"); // 11 chars, max is 10
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyExpiryDate")));
    }

    @Test
    void testValidationFailsForNotifyDateTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setNotifyDate("2024-06-151"); // 11 chars, max is 10
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("notifyDate")));
    }

    @Test
    void testValidationFailsForNotifyMessagesTooLong() {
        AgentNotifyRecord record = createValidRecord();
        record.setNotifyMessages("F".repeat(101)); // 101 chars, max is 100
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("notifyMessages")));
    }

    @Test
    void testNullOptionalFieldsAreAllowed() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentAddress1(null);
        record.setAgentAddress2(null);
        record.setAgentCity(null);
        record.setAgentState(null);
        record.setPolicyHolderFName(null);
        record.setPolicyHolderMName(null);
        record.setPolicyHolderLName(null);
        record.setPolicyStartDate(null);
        record.setPolicyExpiryDate(null);
        record.setNotifyDate(null);
        record.setNotifyMessages(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        // Only required fields should be checked
        assertTrue(violations.isEmpty());
    }

    @Test
    void testBlankOptionalFieldsAreAllowed() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentAddress1("");
        record.setAgentAddress2("");
        record.setAgentCity("");
        record.setAgentState("");
        record.setPolicyHolderFName("");
        record.setPolicyHolderMName("");
        record.setPolicyHolderLName("");
        record.setPolicyStartDate("");
        record.setPolicyExpiryDate("");
        record.setNotifyDate("");
        record.setNotifyMessages("");
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    @Test
    void testBoundaryConditionsForMaxLengthFields() {
        AgentNotifyRecord record = new AgentNotifyRecord(
                "A".repeat(10), // agentCode
                "B".repeat(45), // agentName
                "C".repeat(50), // agentAddress1
                "D".repeat(50), // agentAddress2
                "E".repeat(20), // agentCity
                "F".repeat(2),  // agentState
                "G".repeat(10), // policyNumber
                "H".repeat(35), // policyHolderFName
                "I",            // policyHolderMName
                "J".repeat(35), // policyHolderLName
                "2024-06-01",   // policyStartDate
                "2025-06-01",   // policyExpiryDate
                "2024-06-15",   // notifyDate
                "K".repeat(100) // notifyMessages
        );
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    @Test
    void testBoundaryConditionsForMinLengthFields() {
        AgentNotifyRecord record = new AgentNotifyRecord(
                "A", // agentCode
                "B", // agentName
                null, // agentAddress1
                null, // agentAddress2
                null, // agentCity
                null, // agentState
                "C", // policyNumber
                null, // policyHolderFName
                null, // policyHolderMName
                null, // policyHolderLName
                null, // policyStartDate
                null, // policyExpiryDate
                null, // notifyDate
                null  // notifyMessages
        );
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    // Integration test for Quarkus reflection registration
    @Test
    void testRegisterForReflectionAnnotationPresent() {
        boolean isPresent = AgentNotifyRecord.class.isAnnotationPresent(io.quarkus.runtime.annotations.RegisterForReflection.class);
        assertTrue(isPresent);
    }

    // Test that verifies COBOL business logic mapping: all fields are Strings and lengths are preserved
    @Test
    void testCobolFieldMappingAndLengthPreservation() {
        AgentNotifyRecord record = createValidRecord();
        assertTrue(record.getAgentCode() instanceof String);
        assertTrue(record.getAgentName() instanceof String);
        assertTrue(record.getAgentAddress1() instanceof String);
        assertTrue(record.getAgentAddress2() instanceof String);
        assertTrue(record.getAgentCity() instanceof String);
        assertTrue(record.getAgentState() instanceof String);
        assertTrue(record.getPolicyNumber() instanceof String);
        assertTrue(record.getPolicyHolderFName() instanceof String);
        assertTrue(record.getPolicyHolderMName() instanceof String);
        assertTrue(record.getPolicyHolderLName() instanceof String);
        assertTrue(record.getPolicyStartDate() instanceof String);
        assertTrue(record.getPolicyExpiryDate() instanceof String);
        assertTrue(record.getNotifyDate() instanceof String);
        assertTrue(record.getNotifyMessages() instanceof String);

        assertTrue(record.getAgentCode().length() <= 10);
        assertTrue(record.getAgentName().length() <= 45);
        assertTrue(record.getAgentAddress1().length() <= 50);
        assertTrue(record.getAgentAddress2().length() <= 50);
        assertTrue(record.getAgentCity().length() <= 20);
        assertTrue(record.getAgentState().length() <= 2);
        assertTrue(record.getPolicyNumber().length() <= 10);
        assertTrue(record.getPolicyHolderFName().length() <= 35);
        assertTrue(record.getPolicyHolderMName().length() <= 1);
        assertTrue(record.getPolicyHolderLName().length() <= 35);
        assertTrue(record.getPolicyStartDate().length() <= 10);
        assertTrue(record.getPolicyExpiryDate().length() <= 10);
        assertTrue(record.getNotifyDate().length() <= 10);
        assertTrue(record.getNotifyMessages().length() <= 100);
    }

    // Test for null safety and Lombok-generated methods
    @Test
    void testLombokGeneratedMethodsNullSafety() {
        AgentNotifyRecord record1 = new AgentNotifyRecord();
        AgentNotifyRecord record2 = new AgentNotifyRecord();
        assertEquals(record1, record2);
        assertEquals(record1.hashCode(), record2.hashCode());
        assertNotNull(record1.toString());
        assertNotNull(record2.toString());
    }

    // Test that verifies that changing one field changes equality
    @Test
    void testEqualsChangesWhenFieldChanges() {
        AgentNotifyRecord record1 = createValidRecord();
        AgentNotifyRecord record2 = createValidRecord();
        record2.setNotifyMessages("Different message");
        assertNotEquals(record1, record2);
    }

    // Test edge case: all fields are null except required ones
    @Test
    void testEdgeCaseAllOptionalFieldsNull() {
        AgentNotifyRecord record = new AgentNotifyRecord(
                "AGT1234567",
                "John Doe",
                null,
                null,
                null,
                null,
                "POL1234567",
                null,
                null,
                null,
                null,
                null,
                null,
                null
        );
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    // Test edge case: required fields are blank or null
    @Test
    void testEdgeCaseRequiredFieldsBlankOrNull() {
        AgentNotifyRecord record = new AgentNotifyRecord(
                "", // agentCode
                "", // agentName
                null,
                null,
                null,
                null,
                "", // policyNumber
                null,
                null,
                null,
                null,
                null,
                null,
                null
        );
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertFalse(violations.isEmpty());
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("policyNumber")));
    }

    // Test edge case: date fields with invalid format (should pass as only @Size is enforced)
    @Test
    void testDateFieldsWithInvalidFormat() {
        AgentNotifyRecord record = createValidRecord();
        record.setPolicyStartDate("not-a-date");
        record.setPolicyExpiryDate("1234567890");
        record.setNotifyDate("abc");
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        // Only size is enforced, so these should pass
        assertTrue(violations.isEmpty());
    }

    // Test edge case: notifyMessages is null and blank
    @Test
    void testNotifyMessagesNullAndBlank() {
        AgentNotifyRecord record = createValidRecord();
        record.setNotifyMessages(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
        record.setNotifyMessages("");
        violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }

    // Test edge case: agentState is null and blank
    @Test
    void testAgentStateNullAndBlank() {
        AgentNotifyRecord record = createValidRecord();
        record.setAgentState(null);
        Set<ConstraintViolation<AgentNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty());
        record.setAgentState("");
        violations = validator.validate(record);
        assertTrue(violations.isEmpty());
    }
}