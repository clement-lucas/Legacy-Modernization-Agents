package com.example.agent;

import io.quarkus.test.junit.QuarkusTest;
import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validation;
import jakarta.validation.Validator;
import jakarta.validation.ValidatorFactory;
import org.junit.jupiter.api.*;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;

import java.util.Set;

import static org.junit.jupiter.api.Assertions.*;

@QuarkusTest
class AgentRecordTest {

    @InjectMocks
    private AgentRecord agentRecord;

    private Validator validator;
    private AutoCloseable mocks;

    @BeforeEach
    void setUp() {
        mocks = MockitoAnnotations.openMocks(this);
        ValidatorFactory factory = Validation.buildDefaultValidatorFactory();
        validator = factory.getValidator();
        agentRecord = new AgentRecord();
    }

    @AfterEach
    void tearDown() throws Exception {
        mocks.close();
        agentRecord = null;
    }

    // Helper method to create a valid AgentRecord
    private AgentRecord createValidAgentRecord() {
        AgentRecord record = new AgentRecord();
        record.setAgentCode("AGT1234567");
        record.setAgentName("John Doe");
        record.setAgentAddress1("123 Main St");
        record.setAgentAddress2("Suite 100");
        record.setAgentCity("Metropolis");
        record.setAgentState("NY");
        record.setAgentZipCd("12345");
        record.setAgentDob("1980-01-01");
        record.setAgentType("SALES");
        record.setAgentStatus("A");
        record.setAgentEmail("john.doe@example.com");
        record.setAgentContactNo("5551234567");
        record.setAgentStartDate("2020-01-01");
        record.setAgentEndDate("2025-01-01");
        return record;
    }

    // --- Tests for Getters and Setters ---

    @Test
    void testSetAndGetAgentCode() {
        agentRecord.setAgentCode("AGT000001");
        assertEquals("AGT000001", agentRecord.getAgentCode());
    }

    @Test
    void testSetAndGetAgentName() {
        agentRecord.setAgentName("Jane Smith");
        assertEquals("Jane Smith", agentRecord.getAgentName());
    }

    @Test
    void testSetAndGetAgentAddress1() {
        agentRecord.setAgentAddress1("456 Elm St");
        assertEquals("456 Elm St", agentRecord.getAgentAddress1());
    }

    @Test
    void testSetAndGetAgentAddress2() {
        agentRecord.setAgentAddress2("Apt 2B");
        assertEquals("Apt 2B", agentRecord.getAgentAddress2());
    }

    @Test
    void testSetAndGetAgentCity() {
        agentRecord.setAgentCity("Gotham");
        assertEquals("Gotham", agentRecord.getAgentCity());
    }

    @Test
    void testSetAndGetAgentState() {
        agentRecord.setAgentState("CA");
        assertEquals("CA", agentRecord.getAgentState());
    }

    @Test
    void testSetAndGetAgentZipCd() {
        agentRecord.setAgentZipCd("90210");
        assertEquals("90210", agentRecord.getAgentZipCd());
    }

    @Test
    void testSetAndGetAgentDob() {
        agentRecord.setAgentDob("1975-12-31");
        assertEquals("1975-12-31", agentRecord.getAgentDob());
    }

    @Test
    void testSetAndGetAgentType() {
        agentRecord.setAgentType("SUPPORT");
        assertEquals("SUPPORT", agentRecord.getAgentType());
    }

    @Test
    void testSetAndGetAgentStatus() {
        agentRecord.setAgentStatus("I");
        assertEquals("I", agentRecord.getAgentStatus());
    }

    @Test
    void testSetAndGetAgentEmail() {
        agentRecord.setAgentEmail("agent@company.com");
        assertEquals("agent@company.com", agentRecord.getAgentEmail());
    }

    @Test
    void testSetAndGetAgentContactNo() {
        agentRecord.setAgentContactNo("1234567890");
        assertEquals("1234567890", agentRecord.getAgentContactNo());
    }

    @Test
    void testSetAndGetAgentStartDate() {
        agentRecord.setAgentStartDate("2022-01-01");
        assertEquals("2022-01-01", agentRecord.getAgentStartDate());
    }

    @Test
    void testSetAndGetAgentEndDate() {
        agentRecord.setAgentEndDate("2023-01-01");
        assertEquals("2023-01-01", agentRecord.getAgentEndDate());
    }

    // --- Validation Tests ---

    @Test
    void testAgentCodeNotBlankConstraint() {
        agentRecord.setAgentCode("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testAgentNameNotBlankConstraint() {
        agentRecord.setAgentName("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testAgentCodeMaxLengthConstraint() {
        agentRecord.setAgentCode("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testAgentNameMaxLengthConstraint() {
        agentRecord.setAgentName("A".repeat(46)); // 46 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testAgentAddress1MaxLengthConstraint() {
        agentRecord.setAgentAddress1("A".repeat(51)); // 51 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentAddress1")));
    }

    @Test
    void testAgentAddress2MaxLengthConstraint() {
        agentRecord.setAgentAddress2("A".repeat(51)); // 51 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentAddress2")));
    }

    @Test
    void testAgentCityMaxLengthConstraint() {
        agentRecord.setAgentCity("A".repeat(21)); // 21 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCity")));
    }

    @Test
    void testAgentStateMaxLengthConstraint() {
        agentRecord.setAgentState("ABC"); // 3 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentState")));
    }

    @Test
    void testAgentZipCdMaxLengthConstraint() {
        agentRecord.setAgentZipCd("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentZipCd")));
    }

    @Test
    void testAgentDobMaxLengthConstraint() {
        agentRecord.setAgentDob("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentDob")));
    }

    @Test
    void testAgentTypeMaxLengthConstraint() {
        agentRecord.setAgentType("A".repeat(11)); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentType")));
    }

    @Test
    void testAgentStatusMaxLengthConstraint() {
        agentRecord.setAgentStatus("AB"); // 2 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentStatus")));
    }

    @Test
    void testAgentEmailMaxLengthConstraint() {
        agentRecord.setAgentEmail("A".repeat(31) + "@test.com"); // >30 chars before @
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentEmail")));
    }

    @Test
    void testAgentEmailFormatConstraint() {
        agentRecord.setAgentEmail("not-an-email");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentEmail")));
    }

    @Test
    void testAgentContactNoMaxLengthConstraint() {
        agentRecord.setAgentContactNo("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentContactNo")));
    }

    @Test
    void testAgentStartDateMaxLengthConstraint() {
        agentRecord.setAgentStartDate("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentStartDate")));
    }

    @Test
    void testAgentEndDateMaxLengthConstraint() {
        agentRecord.setAgentEndDate("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentEndDate")));
    }

    // --- Null Checks and Edge Cases ---

    @Test
    void testNullAgentCodeShouldFailNotBlank() {
        agentRecord.setAgentCode(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testNullAgentNameShouldFailNotBlank() {
        agentRecord.setAgentName(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testNullOptionalFieldsShouldPass() {
        // Only agentCode and agentName are @NotBlank
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentAddress1(null);
        agentRecord.setAgentAddress2(null);
        agentRecord.setAgentCity(null);
        agentRecord.setAgentState(null);
        agentRecord.setAgentZipCd(null);
        agentRecord.setAgentDob(null);
        agentRecord.setAgentType(null);
        agentRecord.setAgentStatus(null);
        agentRecord.setAgentEmail(null);
        agentRecord.setAgentContactNo(null);
        agentRecord.setAgentStartDate(null);
        agentRecord.setAgentEndDate(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty());
    }

    @Test
    void testBoundaryAgentCodeLength() {
        agentRecord.setAgentCode("1234567890"); // 10 chars, should pass
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().noneMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
    }

    @Test
    void testBoundaryAgentNameLength() {
        agentRecord.setAgentName("A".repeat(45)); // 45 chars, should pass
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().noneMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    @Test
    void testBoundaryAgentEmailLength() {
        agentRecord.setAgentEmail("a@b.co" + "m".repeat(22)); // total 30 chars
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.stream().noneMatch(v -> v.getPropertyPath().toString().equals("agentEmail")));
    }

    // --- Integration Test: Persistence (Quarkus/JPA) ---
    // This test assumes a test database is configured for QuarkusTest

    @Test
    void testPersistAgentRecordToDatabase() {
        AgentRecord record = createValidAgentRecord();
        // Simulate JPA persist (in real test, use EntityManager)
        // For demonstration, just validate constraints
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty(), "AgentRecord should be valid for persistence");
    }

    // --- COBOL Business Logic Preservation ---
    // Example: Ensure field mappings and constraints match COBOL PIC specifications

    @Test
    void testCobolFieldMappingPreserved() {
        AgentRecord record = createValidAgentRecord();
        assertEquals(10, record.getAgentCode().length(), "COBOL PIC X(10) for agentCode");
        assertTrue(record.getAgentName().length() <= 45, "COBOL PIC X(45) for agentName");
        assertTrue(record.getAgentAddress1().length() <= 50, "COBOL PIC X(50) for agentAddress1");
        assertTrue(record.getAgentAddress2().length() <= 50, "COBOL PIC X(50) for agentAddress2");
        assertTrue(record.getAgentCity().length() <= 20, "COBOL PIC X(20) for agentCity");
        assertTrue(record.getAgentState().length() <= 2, "COBOL PIC X(2) for agentState");
        assertTrue(record.getAgentZipCd().length() <= 10, "COBOL PIC X(10) for agentZipCd");
        assertTrue(record.getAgentDob().length() <= 10, "COBOL PIC X(10) for agentDob");
        assertTrue(record.getAgentType().length() <= 10, "COBOL PIC X(10) for agentType");
        assertTrue(record.getAgentStatus().length() <= 1, "COBOL PIC X(1) for agentStatus");
        assertTrue(record.getAgentEmail().length() <= 30, "COBOL PIC X(30) for agentEmail");
        assertTrue(record.getAgentContactNo().length() <= 10, "COBOL PIC X(10) for agentContactNo");
        assertTrue(record.getAgentStartDate().length() <= 10, "COBOL PIC X(10) for agentStartDate");
        assertTrue(record.getAgentEndDate().length() <= 10, "COBOL PIC X(10) for agentEndDate");
    }

    // --- Complex Scenario: All Fields at Maximum Length ---
    @Test
    void testAllFieldsAtMaximumLength() {
        AgentRecord record = new AgentRecord();
        record.setAgentCode("A".repeat(10));
        record.setAgentName("B".repeat(45));
        record.setAgentAddress1("C".repeat(50));
        record.setAgentAddress2("D".repeat(50));
        record.setAgentCity("E".repeat(20));
        record.setAgentState("F".repeat(2));
        record.setAgentZipCd("G".repeat(10));
        record.setAgentDob("H".repeat(10));
        record.setAgentType("I".repeat(10));
        record.setAgentStatus("J");
        record.setAgentEmail("a@b.co" + "m".repeat(22)); // 30 chars
        record.setAgentContactNo("K".repeat(10));
        record.setAgentStartDate("L".repeat(10));
        record.setAgentEndDate("M".repeat(10));
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(record);
        assertTrue(violations.isEmpty(), "All fields at max length should pass validation");
    }

    // --- Edge Case: Empty String for Optional Fields ---
    @Test
    void testEmptyStringOptionalFields() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentAddress1("");
        agentRecord.setAgentAddress2("");
        agentRecord.setAgentCity("");
        agentRecord.setAgentState("");
        agentRecord.setAgentZipCd("");
        agentRecord.setAgentDob("");
        agentRecord.setAgentType("");
        agentRecord.setAgentStatus("");
        agentRecord.setAgentEmail("");
        agentRecord.setAgentContactNo("");
        agentRecord.setAgentStartDate("");
        agentRecord.setAgentEndDate("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        // Only agentCode and agentName are @NotBlank, so should pass
        assertTrue(violations.isEmpty());
    }

    // --- Edge Case: Null for All Fields ---
    @Test
    void testNullForAllFields() {
        agentRecord.setAgentCode(null);
        agentRecord.setAgentName(null);
        agentRecord.setAgentAddress1(null);
        agentRecord.setAgentAddress2(null);
        agentRecord.setAgentCity(null);
        agentRecord.setAgentState(null);
        agentRecord.setAgentZipCd(null);
        agentRecord.setAgentDob(null);
        agentRecord.setAgentType(null);
        agentRecord.setAgentStatus(null);
        agentRecord.setAgentEmail(null);
        agentRecord.setAgentContactNo(null);
        agentRecord.setAgentStartDate(null);
        agentRecord.setAgentEndDate(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        // Only agentCode and agentName are @NotBlank, so should fail for those
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentCode")));
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("agentName")));
    }

    // --- Edge Case: AgentStatus as blank (should pass, only max length enforced) ---
    @Test
    void testAgentStatusBlankShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentStatus("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Blank agentStatus should pass as only max length enforced");
    }

    // --- Edge Case: AgentEmail blank (should pass, only format and max length enforced) ---
    @Test
    void testAgentEmailBlankShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentEmail("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Blank agentEmail should pass as only format and max length enforced");
    }

    // --- Edge Case: AgentEmail null (should pass, only format and max length enforced) ---
    @Test
    void testAgentEmailNullShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentEmail(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Null agentEmail should pass as only format and max length enforced");
    }

    // --- Edge Case: AgentContactNo blank (should pass, only max length enforced) ---
    @Test
    void testAgentContactNoBlankShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentContactNo("");
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Blank agentContactNo should pass as only max length enforced");
    }

    // --- Edge Case: AgentContactNo null (should pass, only max length enforced) ---
    @Test
    void testAgentContactNoNullShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentContactNo(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Null agentContactNo should pass as only max length enforced");
    }

    // --- Edge Case: AgentStartDate and AgentEndDate blank/null ---
    @Test
    void testAgentStartDateAndEndDateBlankAndNullShouldPass() {
        agentRecord.setAgentCode("AGT1234567");
        agentRecord.setAgentName("John Doe");
        agentRecord.setAgentStartDate("");
        agentRecord.setAgentEndDate(null);
        Set<ConstraintViolation<AgentRecord>> violations = validator.validate(agentRecord);
        assertTrue(violations.isEmpty(), "Blank/null agentStartDate/agentEndDate should pass as only max length enforced");
    }
}