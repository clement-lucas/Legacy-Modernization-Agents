package com.example.customer;

import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validation;
import jakarta.validation.Validator;
import jakarta.validation.ValidatorFactory;
import org.junit.jupiter.api.*;
import org.mockito.MockedStatic;
import org.mockito.Mockito;

import java.util.Set;

import static org.junit.jupiter.api.Assertions.*;

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
class CustomerNotifyRecordTest {

    private CustomerNotifyRecord record;
    private Validator validator;

    @BeforeAll
    void setUpValidator() {
        ValidatorFactory factory = Validation.buildDefaultValidatorFactory();
        validator = factory.getValidator();
    }

    @BeforeEach
    void setUp() {
        record = new CustomerNotifyRecord(
                "POL1234567",
                "John",
                "A",
                "Doe",
                "2024-01-01",
                "2025-01-01",
                "2024-06-01",
                "Renewal notification",
                "AGT1234567",
                "Agent Smith",
                "Statutory message text"
        );
    }

    @AfterEach
    void tearDown() {
        record = null;
    }

    @Test
    void testAllArgsConstructorSetsFieldsCorrectly() {
        assertEquals("POL1234567", record.getCustPolicyNumber());
        assertEquals("John", record.getCustFirstName());
        assertEquals("A", record.getCustMiddleName());
        assertEquals("Doe", record.getCustLastName());
        assertEquals("2024-01-01", record.getCustStartDate());
        assertEquals("2025-01-01", record.getCustExpiryDate());
        assertEquals("2024-06-01", record.getCustNotifyDate());
        assertEquals("Renewal notification", record.getCustNotifyMessages());
        assertEquals("AGT1234567", record.getCustAgentCode());
        assertEquals("Agent Smith", record.getCustAgentName());
        assertEquals("Statutory message text", record.getStatutoryMessage());
    }

    @Test
    void testNoArgsConstructorCreatesEmptyObject() {
        CustomerNotifyRecord emptyRecord = new CustomerNotifyRecord();
        assertNull(emptyRecord.getCustPolicyNumber());
        assertNull(emptyRecord.getCustFirstName());
        assertNull(emptyRecord.getCustMiddleName());
        assertNull(emptyRecord.getCustLastName());
        assertNull(emptyRecord.getCustStartDate());
        assertNull(emptyRecord.getCustExpiryDate());
        assertNull(emptyRecord.getCustNotifyDate());
        assertNull(emptyRecord.getCustNotifyMessages());
        assertNull(emptyRecord.getCustAgentCode());
        assertNull(emptyRecord.getCustAgentName());
        assertNull(emptyRecord.getStatutoryMessage());
    }

    @Test
    void testSettersAndGettersWorkForAllFields() {
        CustomerNotifyRecord r = new CustomerNotifyRecord();
        r.setCustPolicyNumber("POL0000001");
        r.setCustFirstName("Jane");
        r.setCustMiddleName("B");
        r.setCustLastName("Smith");
        r.setCustStartDate("2023-12-31");
        r.setCustExpiryDate("2024-12-31");
        r.setCustNotifyDate("2024-06-15");
        r.setCustNotifyMessages("Policy expires soon");
        r.setCustAgentCode("AGT0000001");
        r.setCustAgentName("Agent Doe");
        r.setStatutoryMessage("Legal notice");

        assertEquals("POL0000001", r.getCustPolicyNumber());
        assertEquals("Jane", r.getCustFirstName());
        assertEquals("B", r.getCustMiddleName());
        assertEquals("Smith", r.getCustLastName());
        assertEquals("2023-12-31", r.getCustStartDate());
        assertEquals("2024-12-31", r.getCustExpiryDate());
        assertEquals("2024-06-15", r.getCustNotifyDate());
        assertEquals("Policy expires soon", r.getCustNotifyMessages());
        assertEquals("AGT0000001", r.getCustAgentCode());
        assertEquals("Agent Doe", r.getCustAgentName());
        assertEquals("Legal notice", r.getStatutoryMessage());
    }

    @Test
    void testCustPolicyNumberCannotBeBlank() {
        record.setCustPolicyNumber("");
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custPolicyNumber")));
    }

    @Test
    void testCustPolicyNumberCannotBeNull() {
        record.setCustPolicyNumber(null);
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custPolicyNumber")));
    }

    @Test
    void testCustPolicyNumberMaxLength() {
        record.setCustPolicyNumber("12345678901"); // 11 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custPolicyNumber")));
    }

    @Test
    void testCustFirstNameMaxLength() {
        record.setCustFirstName("A".repeat(36)); // 36 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custFirstName")));
    }

    @Test
    void testCustMiddleNameMaxLength() {
        record.setCustMiddleName("AB"); // 2 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custMiddleName")));
    }

    @Test
    void testCustLastNameMaxLength() {
        record.setCustLastName("B".repeat(36)); // 36 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custLastName")));
    }

    @Test
    void testCustStartDateMaxLength() {
        record.setCustStartDate("2024-01-011"); // 11 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custStartDate")));
    }

    @Test
    void testCustExpiryDateMaxLength() {
        record.setCustExpiryDate("2025-01-011"); // 11 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custExpiryDate")));
    }

    @Test
    void testCustNotifyDateMaxLength() {
        record.setCustNotifyDate("2024-06-011"); // 11 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custNotifyDate")));
    }

    @Test
    void testCustNotifyMessagesMaxLength() {
        record.setCustNotifyMessages("M".repeat(101)); // 101 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custNotifyMessages")));
    }

    @Test
    void testCustAgentCodeMaxLength() {
        record.setCustAgentCode("C".repeat(11)); // 11 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custAgentCode")));
    }

    @Test
    void testCustAgentNameMaxLength() {
        record.setCustAgentName("N".repeat(46)); // 46 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custAgentName")));
    }

    @Test
    void testStatutoryMessageMaxLength() {
        record.setStatutoryMessage("S".repeat(101)); // 101 chars, should fail
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("statutoryMessage")));
    }

    @Test
    void testNullValuesForOptionalFields() {
        record.setCustFirstName(null);
        record.setCustMiddleName(null);
        record.setCustLastName(null);
        record.setCustStartDate(null);
        record.setCustExpiryDate(null);
        record.setCustNotifyDate(null);
        record.setCustNotifyMessages(null);
        record.setCustAgentCode(null);
        record.setCustAgentName(null);
        record.setStatutoryMessage(null);

        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        // Only custPolicyNumber is @NotBlank, others allow null
        assertTrue(violations.stream().noneMatch(v -> !v.getPropertyPath().toString().equals("custPolicyNumber")));
    }

    @Test
    void testBoundaryValuesForAllFields() {
        CustomerNotifyRecord boundaryRecord = new CustomerNotifyRecord(
                "1234567890", // 10 chars
                "A".repeat(35), // 35 chars
                "B", // 1 char
                "C".repeat(35), // 35 chars
                "2024-01-01", // 10 chars
                "2025-01-01", // 10 chars
                "2024-06-01", // 10 chars
                "M".repeat(100), // 100 chars
                "D".repeat(10), // 10 chars
                "E".repeat(45), // 45 chars
                "F".repeat(100) // 100 chars
        );
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(boundaryRecord);
        assertTrue(violations.isEmpty());
    }

    @Test
    void testEqualsAndHashCodeContract() {
        CustomerNotifyRecord r1 = new CustomerNotifyRecord(
                "POL1234567",
                "John",
                "A",
                "Doe",
                "2024-01-01",
                "2025-01-01",
                "2024-06-01",
                "Renewal notification",
                "AGT1234567",
                "Agent Smith",
                "Statutory message text"
        );
        CustomerNotifyRecord r2 = new CustomerNotifyRecord(
                "POL1234567",
                "John",
                "A",
                "Doe",
                "2024-01-01",
                "2025-01-01",
                "2024-06-01",
                "Renewal notification",
                "AGT1234567",
                "Agent Smith",
                "Statutory message text"
        );
        assertEquals(r1, r2);
        assertEquals(r1.hashCode(), r2.hashCode());
    }

    @Test
    void testToStringReturnsNonNullString() {
        String str = record.toString();
        assertNotNull(str);
        assertTrue(str.contains("POL1234567"));
        assertTrue(str.contains("John"));
    }

    @Test
    void testLombokBuilderPatternIfAvailable() {
        // Simulate builder pattern if Lombok @Builder is added in future
        try (MockedStatic<CustomerNotifyRecord> mockStatic = Mockito.mockStatic(CustomerNotifyRecord.class)) {
            // No builder in current code, so just ensure no builder exists
            assertThrows(NoSuchMethodException.class, () -> CustomerNotifyRecord.class.getMethod("builder"));
        } catch (Exception ignored) {
        }
    }

    // Integration test for JPA entity mapping (QuarkusTest)
    // This test verifies that the entity can be persisted and retrieved using Quarkus/Hibernate
    // NOTE: This test requires a Quarkus test environment with a configured datasource.
    // If not available, this test can be ignored or run in integration phase.
    /*
    @QuarkusTest
    @Test
    void testJpaEntityPersistence(@Inject EntityManager em) {
        CustomerNotifyRecord entity = new CustomerNotifyRecord(
                "POL9999999",
                "Alice",
                "C",
                "Johnson",
                "2022-01-01",
                "2023-01-01",
                "2022-12-01",
                "Expiry notification",
                "AGT9999999",
                "Agent Cooper",
                "Statutory message"
        );
        em.persist(entity);
        em.flush();
        CustomerNotifyRecord found = em.find(CustomerNotifyRecord.class, "POL9999999");
        assertNotNull(found);
        assertEquals("Alice", found.getCustFirstName());
        assertEquals("Johnson", found.getCustLastName());
    }
    */

    // Complex scenario: test that COBOL business logic for PIC X(n) truncation is preserved
    // If input exceeds max length, validation fails and truncation does NOT occur (Java validation)
    @Test
    void testCobolPicXTruncationNotAppliedInJava() {
        record.setCustFirstName("A".repeat(40)); // 40 chars, exceeds PIC X(35)
        Set<ConstraintViolation<CustomerNotifyRecord>> violations = validator.validate(record);
        assertTrue(violations.stream().anyMatch(v -> v.getPropertyPath().toString().equals("custFirstName")));
    }
}