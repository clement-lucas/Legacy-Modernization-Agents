package com.example.cobol;

import org.junit.jupiter.api.*;
import org.mockito.*;
import java.math.BigDecimal;
import static org.junit.jupiter.api.Assertions.*;

@DisplayName("PolicyRecord Unit Tests")
class PolicyRecordTest {

    private PolicyRecord policyRecord;

    @BeforeEach
    void setUp() {
        policyRecord = new PolicyRecord();
        policyRecord.policyNumber = "POL123456";
        policyRecord.policyHolderFName = "John";
        policyRecord.policyHolderMName = "A";
        policyRecord.policyHolderLName = "Doe";
        policyRecord.policyHolderAddr1 = "123 Main St";
        policyRecord.policyHolderAddr2 = "Apt 4B";
        policyRecord.policyHolderCity = "Springfield";
        policyRecord.policyHolderState = "IL";
        policyRecord.policyHolderZipCd = "62704";
        policyRecord.policyStartDate = "2024-01-01";
        policyRecord.policyExpiryDate = "2025-01-01";
        policyRecord.policyPremiumAmount = new BigDecimal("1200.50");
        policyRecord.policyAgentCode = "AGT789";
        policyRecord.policyBenefName = "Jane Doe";
        policyRecord.agentType = "CORPORATE";
    }

    @AfterEach
    void tearDown() {
        policyRecord = null;
    }

    @Test
    @DisplayName("Should correctly assign and retrieve all PolicyRecord fields")
    void testFieldAssignmentsAndRetrieval() {
        assertEquals("POL123456", policyRecord.policyNumber);
        assertEquals("John", policyRecord.policyHolderFName);
        assertEquals("A", policyRecord.policyHolderMName);
        assertEquals("Doe", policyRecord.policyHolderLName);
        assertEquals("123 Main St", policyRecord.policyHolderAddr1);
        assertEquals("Apt 4B", policyRecord.policyHolderAddr2);
        assertEquals("Springfield", policyRecord.policyHolderCity);
        assertEquals("IL", policyRecord.policyHolderState);
        assertEquals("62704", policyRecord.policyHolderZipCd);
        assertEquals("2024-01-01", policyRecord.policyStartDate);
        assertEquals("2025-01-01", policyRecord.policyExpiryDate);
        assertEquals(new BigDecimal("1200.50"), policyRecord.policyPremiumAmount);
        assertEquals("AGT789", policyRecord.policyAgentCode);
        assertEquals("Jane Doe", policyRecord.policyBenefName);
        assertEquals("CORPORATE", policyRecord.agentType);
    }

    @Test
    @DisplayName("Should handle null values for all fields")
    void testNullFieldAssignments() {
        PolicyRecord nullRecord = new PolicyRecord();
        nullRecord.policyNumber = null;
        nullRecord.policyHolderFName = null;
        nullRecord.policyHolderMName = null;
        nullRecord.policyHolderLName = null;
        nullRecord.policyHolderAddr1 = null;
        nullRecord.policyHolderAddr2 = null;
        nullRecord.policyHolderCity = null;
        nullRecord.policyHolderState = null;
        nullRecord.policyHolderZipCd = null;
        nullRecord.policyStartDate = null;
        nullRecord.policyExpiryDate = null;
        nullRecord.policyPremiumAmount = null;
        nullRecord.policyAgentCode = null;
        nullRecord.policyBenefName = null;
        nullRecord.agentType = null;

        assertNull(nullRecord.policyNumber);
        assertNull(nullRecord.policyHolderFName);
        assertNull(nullRecord.policyHolderMName);
        assertNull(nullRecord.policyHolderLName);
        assertNull(nullRecord.policyHolderAddr1);
        assertNull(nullRecord.policyHolderAddr2);
        assertNull(nullRecord.policyHolderCity);
        assertNull(nullRecord.policyHolderState);
        assertNull(nullRecord.policyHolderZipCd);
        assertNull(nullRecord.policyStartDate);
        assertNull(nullRecord.policyExpiryDate);
        assertNull(nullRecord.policyPremiumAmount);
        assertNull(nullRecord.policyAgentCode);
        assertNull(nullRecord.policyBenefName);
        assertNull(nullRecord.agentType);
    }

    @Test
    @DisplayName("Should handle boundary values for policyPremiumAmount")
    void testPolicyPremiumAmountBoundaries() {
        PolicyRecord minPremiumRecord = new PolicyRecord();
        minPremiumRecord.policyPremiumAmount = BigDecimal.ZERO;
        assertEquals(BigDecimal.ZERO, minPremiumRecord.policyPremiumAmount);

        PolicyRecord maxPremiumRecord = new PolicyRecord();
        maxPremiumRecord.policyPremiumAmount = new BigDecimal("999999999.99");
        assertEquals(new BigDecimal("999999999.99"), maxPremiumRecord.policyPremiumAmount);

        PolicyRecord negativePremiumRecord = new PolicyRecord();
        negativePremiumRecord.policyPremiumAmount = new BigDecimal("-1.00");
        assertEquals(new BigDecimal("-1.00"), negativePremiumRecord.policyPremiumAmount);
    }

    @Test
    @DisplayName("Should handle empty strings for all String fields")
    void testEmptyStringAssignments() {
        PolicyRecord emptyRecord = new PolicyRecord();
        emptyRecord.policyNumber = "";
        emptyRecord.policyHolderFName = "";
        emptyRecord.policyHolderMName = "";
        emptyRecord.policyHolderLName = "";
        emptyRecord.policyHolderAddr1 = "";
        emptyRecord.policyHolderAddr2 = "";
        emptyRecord.policyHolderCity = "";
        emptyRecord.policyHolderState = "";
        emptyRecord.policyHolderZipCd = "";
        emptyRecord.policyStartDate = "";
        emptyRecord.policyExpiryDate = "";
        emptyRecord.policyAgentCode = "";
        emptyRecord.policyBenefName = "";
        emptyRecord.agentType = "";

        assertEquals("", emptyRecord.policyNumber);
        assertEquals("", emptyRecord.policyHolderFName);
        assertEquals("", emptyRecord.policyHolderMName);
        assertEquals("", emptyRecord.policyHolderLName);
        assertEquals("", emptyRecord.policyHolderAddr1);
        assertEquals("", emptyRecord.policyHolderAddr2);
        assertEquals("", emptyRecord.policyHolderCity);
        assertEquals("", emptyRecord.policyHolderState);
        assertEquals("", emptyRecord.policyHolderZipCd);
        assertEquals("", emptyRecord.policyStartDate);
        assertEquals("", emptyRecord.policyExpiryDate);
        assertEquals("", emptyRecord.policyAgentCode);
        assertEquals("", emptyRecord.policyBenefName);
        assertEquals("", emptyRecord.agentType);
    }

    @Test
    @DisplayName("Should preserve COBOL business logic for agentType field")
    void testAgentTypeBusinessLogic() {
        policyRecord.agentType = "CORPORATE";
        assertEquals("CORPORATE", policyRecord.agentType);

        policyRecord.agentType = "INDIVIDUAL";
        assertEquals("INDIVIDUAL", policyRecord.agentType);

        // Edge case: unexpected agentType value
        policyRecord.agentType = "UNKNOWN";
        assertEquals("UNKNOWN", policyRecord.agentType);
    }

    @Test
    @DisplayName("Should allow updating fields after initial assignment")
    void testFieldUpdateAfterAssignment() {
        policyRecord.policyHolderFName = "Jane";
        policyRecord.policyPremiumAmount = new BigDecimal("1500.00");
        policyRecord.policyExpiryDate = "2026-01-01";

        assertEquals("Jane", policyRecord.policyHolderFName);
        assertEquals(new BigDecimal("1500.00"), policyRecord.policyPremiumAmount);
        assertEquals("2026-01-01", policyRecord.policyExpiryDate);
    }

    @Test
    @DisplayName("Should handle invalid date formats in policyStartDate and policyExpiryDate")
    void testInvalidDateFormats() {
        policyRecord.policyStartDate = "01-01-2024";
        policyRecord.policyExpiryDate = "2025/01/01";
        assertEquals("01-01-2024", policyRecord.policyStartDate);
        assertEquals("2025/01/01", policyRecord.policyExpiryDate);

        policyRecord.policyStartDate = "invalid-date";
        policyRecord.policyExpiryDate = "";
        assertEquals("invalid-date", policyRecord.policyStartDate);
        assertEquals("", policyRecord.policyExpiryDate);
    }

    @Test
    @DisplayName("Should handle long strings exceeding typical COBOL field length")
    void testLongStringAssignments() {
        String longString = "A".repeat(256);
        policyRecord.policyHolderFName = longString;
        policyRecord.policyHolderLName = longString;
        policyRecord.policyHolderAddr1 = longString;
        policyRecord.policyHolderCity = longString;
        policyRecord.policyHolderState = longString;
        policyRecord.policyHolderZipCd = longString;
        policyRecord.policyAgentCode = longString;
        policyRecord.policyBenefName = longString;

        assertEquals(longString, policyRecord.policyHolderFName);
        assertEquals(longString, policyRecord.policyHolderLName);
        assertEquals(longString, policyRecord.policyHolderAddr1);
        assertEquals(longString, policyRecord.policyHolderCity);
        assertEquals(longString, policyRecord.policyHolderState);
        assertEquals(longString, policyRecord.policyHolderZipCd);
        assertEquals(longString, policyRecord.policyAgentCode);
        assertEquals(longString, policyRecord.policyBenefName);
    }

    @Test
    @DisplayName("Should not throw exceptions when all fields are unset (default constructor)")
    void testDefaultConstructorNoException() {
        assertDoesNotThrow(() -> {
            PolicyRecord defaultRecord = new PolicyRecord();
            assertNull(defaultRecord.policyNumber);
            assertNull(defaultRecord.policyHolderFName);
            assertNull(defaultRecord.policyPremiumAmount);
        });
    }

    @Test
    @DisplayName("Should handle assignment of special characters in string fields")
    void testSpecialCharacterAssignments() {
        policyRecord.policyHolderFName = "J@hn#Doe!";
        policyRecord.policyHolderAddr1 = "123 Main St. #$%";
        policyRecord.policyHolderCity = "Spring!field";
        policyRecord.policyHolderState = "I*L";
        policyRecord.policyHolderZipCd = "62@704";
        policyRecord.policyAgentCode = "AGT*789";
        policyRecord.policyBenefName = "Jane*Doe";

        assertEquals("J@hn#Doe!", policyRecord.policyHolderFName);
        assertEquals("123 Main St. #$%", policyRecord.policyHolderAddr1);
        assertEquals("Spring!field", policyRecord.policyHolderCity);
        assertEquals("I*L", policyRecord.policyHolderState);
        assertEquals("62@704", policyRecord.policyHolderZipCd);
        assertEquals("AGT*789", policyRecord.policyAgentCode);
        assertEquals("Jane*Doe", policyRecord.policyBenefName);
    }

    // Complex scenario: Simulate mapping from COBOL copybook to Java object
    @Test
    @DisplayName("Should correctly map COBOL copybook fields to PolicyRecord")
    void testCobolCopybookMapping() {
        // Simulate a COBOL record with typical values
        String cobolPolicyNumber = "POL987654";
        String cobolFName = "Alice";
        String cobolMName = "B";
        String cobolLName = "Smith";
        String cobolAddr1 = "456 Elm St";
        String cobolAddr2 = "Suite 100";
        String cobolCity = "Metropolis";
        String cobolState = "NY";
        String cobolZip = "10001";
        String cobolStartDate = "2023-06-01";
        String cobolExpiryDate = "2024-06-01";
        BigDecimal cobolPremium = new BigDecimal("2500.75");
        String cobolAgentCode = "AGT123";
        String cobolBenefName = "Bob Smith";
        String cobolAgentType = "INDIVIDUAL";

        PolicyRecord cobolRecord = new PolicyRecord();
        cobolRecord.policyNumber = cobolPolicyNumber;
        cobolRecord.policyHolderFName = cobolFName;
        cobolRecord.policyHolderMName = cobolMName;
        cobolRecord.policyHolderLName = cobolLName;
        cobolRecord.policyHolderAddr1 = cobolAddr1;
        cobolRecord.policyHolderAddr2 = cobolAddr2;
        cobolRecord.policyHolderCity = cobolCity;
        cobolRecord.policyHolderState = cobolState;
        cobolRecord.policyHolderZipCd = cobolZip;
        cobolRecord.policyStartDate = cobolStartDate;
        cobolRecord.policyExpiryDate = cobolExpiryDate;
        cobolRecord.policyPremiumAmount = cobolPremium;
        cobolRecord.policyAgentCode = cobolAgentCode;
        cobolRecord.policyBenefName = cobolBenefName;
        cobolRecord.agentType = cobolAgentType;

        assertEquals(cobolPolicyNumber, cobolRecord.policyNumber);
        assertEquals(cobolFName, cobolRecord.policyHolderFName);
        assertEquals(cobolMName, cobolRecord.policyHolderMName);
        assertEquals(cobolLName, cobolRecord.policyHolderLName);
        assertEquals(cobolAddr1, cobolRecord.policyHolderAddr1);
        assertEquals(cobolAddr2, cobolRecord.policyHolderAddr2);
        assertEquals(cobolCity, cobolRecord.policyHolderCity);
        assertEquals(cobolState, cobolRecord.policyHolderState);
        assertEquals(cobolZip, cobolRecord.policyHolderZipCd);
        assertEquals(cobolStartDate, cobolRecord.policyStartDate);
        assertEquals(cobolExpiryDate, cobolRecord.policyExpiryDate);
        assertEquals(cobolPremium, cobolRecord.policyPremiumAmount);
        assertEquals(cobolAgentCode, cobolRecord.policyAgentCode);
        assertEquals(cobolBenefName, cobolRecord.policyBenefName);
        assertEquals(cobolAgentType, cobolRecord.agentType);
    }

    // Integration test: If PolicyRecord is persisted to DB via Quarkus, simulate with mock (no DB code in class)
    @Test
    @DisplayName("Integration: Should allow PolicyRecord to be used in persistence scenario")
    void testPolicyRecordPersistenceIntegration() {
        // Simulate a repository save (mocked)
        PolicyRecord recordToPersist = new PolicyRecord();
        recordToPersist.policyNumber = "POL555555";
        recordToPersist.policyHolderFName = "Persist";
        recordToPersist.policyPremiumAmount = new BigDecimal("999.99");

        // Simulate persistence by copying to a new object (as a stand-in for DB round-trip)
        PolicyRecord persistedRecord = new PolicyRecord();
        persistedRecord.policyNumber = recordToPersist.policyNumber;
        persistedRecord.policyHolderFName = recordToPersist.policyHolderFName;
        persistedRecord.policyPremiumAmount = recordToPersist.policyPremiumAmount;

        assertEquals(recordToPersist.policyNumber, persistedRecord.policyNumber);
        assertEquals(recordToPersist.policyHolderFName, persistedRecord.policyHolderFName);
        assertEquals(recordToPersist.policyPremiumAmount, persistedRecord.policyPremiumAmount);
    }

    // Edge case: Assign null to BigDecimal field and use it
    @Test
    @DisplayName("Should handle null BigDecimal in policyPremiumAmount gracefully")
    void testNullBigDecimalPolicyPremiumAmount() {
        policyRecord.policyPremiumAmount = null;
        assertNull(policyRecord.policyPremiumAmount);
    }
}