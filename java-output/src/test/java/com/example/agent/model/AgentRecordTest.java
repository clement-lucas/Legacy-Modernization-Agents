package com.example.agent.model;

import io.quarkus.test.junit.QuarkusTest;
import org.junit.jupiter.api.*;
import org.mockito.MockitoAnnotations;

import java.lang.reflect.Field;

import static org.junit.jupiter.api.Assertions.*;

@QuarkusTest
class AgentRecordTest {

    private AgentRecord agentRecord;

    @BeforeEach
    void setUp() {
        MockitoAnnotations.openMocks(this);
        agentRecord = new AgentRecord();
    }

    @AfterEach
    void tearDown() {
        agentRecord = null;
    }

    // Helper method to set private fields via reflection for testing purposes
    private void setField(String fieldName, String value) {
        try {
            Field field = AgentRecord.class.getDeclaredField(fieldName);
            field.setAccessible(true);
            field.set(agentRecord, value);
        } catch (Exception e) {
            fail("Failed to set field: " + fieldName, e);
        }
    }

    // Helper method to get private fields via reflection for testing purposes
    private String getField(String fieldName) {
        try {
            Field field = AgentRecord.class.getDeclaredField(fieldName);
            field.setAccessible(true);
            return (String) field.get(agentRecord);
        } catch (Exception e) {
            fail("Failed to get field: " + fieldName, e);
            return null;
        }
    }

    @Test
    void testSetAndGetAgentCode() {
        // Arrange
        String expected = "AGT1234567";
        // Act
        setField("agentCode", expected);
        // Assert
        assertEquals(expected, getField("agentCode"), "Agent code should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentName() {
        String expected = "John Doe";
        setField("agentName", expected);
        assertEquals(expected, getField("agentName"), "Agent name should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentAddress1() {
        String expected = "123 Main St";
        setField("agentAddress1", expected);
        assertEquals(expected, getField("agentAddress1"), "Agent address 1 should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentAddress2() {
        String expected = "Suite 100";
        setField("agentAddress2", expected);
        assertEquals(expected, getField("agentAddress2"), "Agent address 2 should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentCity() {
        String expected = "Springfield";
        setField("agentCity", expected);
        assertEquals(expected, getField("agentCity"), "Agent city should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentState() {
        String expected = "CA";
        setField("agentState", expected);
        assertEquals(expected, getField("agentState"), "Agent state should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentZipCode() {
        String expected = "90210";
        setField("agentZipCode", expected);
        assertEquals(expected, getField("agentZipCode"), "Agent zip code should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentStatus() {
        String expected = "A";
        setField("agentStatus", expected);
        assertEquals(expected, getField("agentStatus"), "Agent status should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentType() {
        String expected = "BROKER";
        setField("agentType", expected);
        assertEquals(expected, getField("agentType"), "Agent type should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentEmail() {
        String expected = "john.doe@example.com";
        setField("agentEmail", expected);
        assertEquals(expected, getField("agentEmail"), "Agent email should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentContactNo() {
        String expected = "5551234567";
        setField("agentContactNo", expected);
        assertEquals(expected, getField("agentContactNo"), "Agent contact number should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentStartDate() {
        String expected = "2023-01-01";
        setField("agentStartDate", expected);
        assertEquals(expected, getField("agentStartDate"), "Agent start date should be set and retrieved correctly");
    }

    @Test
    void testSetAndGetAgentEndDate() {
        String expected = "2023-12-31";
        setField("agentEndDate", expected);
        assertEquals(expected, getField("agentEndDate"), "Agent end date should be set and retrieved correctly");
    }

    @Test
    void testNullValuesForAllFields() {
        // Arrange & Act
        setField("agentCode", null);
        setField("agentName", null);
        setField("agentAddress1", null);
        setField("agentAddress2", null);
        setField("agentCity", null);
        setField("agentState", null);
        setField("agentZipCode", null);
        setField("agentStatus", null);
        setField("agentType", null);
        setField("agentEmail", null);
        setField("agentContactNo", null);
        setField("agentStartDate", null);
        setField("agentEndDate", null);

        // Assert
        assertNull(getField("agentCode"), "Agent code should allow null");
        assertNull(getField("agentName"), "Agent name should allow null");
        assertNull(getField("agentAddress1"), "Agent address 1 should allow null");
        assertNull(getField("agentAddress2"), "Agent address 2 should allow null");
        assertNull(getField("agentCity"), "Agent city should allow null");
        assertNull(getField("agentState"), "Agent state should allow null");
        assertNull(getField("agentZipCode"), "Agent zip code should allow null");
        assertNull(getField("agentStatus"), "Agent status should allow null");
        assertNull(getField("agentType"), "Agent type should allow null");
        assertNull(getField("agentEmail"), "Agent email should allow null");
        assertNull(getField("agentContactNo"), "Agent contact number should allow null");
        assertNull(getField("agentStartDate"), "Agent start date should allow null");
        assertNull(getField("agentEndDate"), "Agent end date should allow null");
    }

    @Test
    void testBoundaryConditionsForFieldLengths() {
        // COBOL PIC X(n) means max n chars, test exactly n and n+1 for truncation/acceptance

        setField("agentCode", "1234567890"); // 10 chars
        assertEquals(10, getField("agentCode").length(), "Agent code should accept 10 characters");

        setField("agentName", "A".repeat(30));
        assertEquals(30, getField("agentName").length(), "Agent name should accept 30 characters");

        setField("agentAddress1", "B".repeat(50));
        assertEquals(50, getField("agentAddress1").length(), "Agent address 1 should accept 50 characters");

        setField("agentAddress2", "C".repeat(50));
        assertEquals(50, getField("agentAddress2").length(), "Agent address 2 should accept 50 characters");

        setField("agentCity", "D".repeat(20));
        assertEquals(20, getField("agentCity").length(), "Agent city should accept 20 characters");

        setField("agentState", "CA");
        assertEquals(2, getField("agentState").length(), "Agent state should accept 2 characters");

        setField("agentZipCode", "1234567890");
        assertEquals(10, getField("agentZipCode").length(), "Agent zip code should accept 10 characters");

        setField("agentStatus", "X");
        assertEquals(1, getField("agentStatus").length(), "Agent status should accept 1 character");

        setField("agentType", "TYPE123456");
        assertEquals(10, getField("agentType").length(), "Agent type should accept 10 characters");

        setField("agentEmail", "E".repeat(30));
        assertEquals(30, getField("agentEmail").length(), "Agent email should accept 30 characters");

        setField("agentContactNo", "1234567890");
        assertEquals(10, getField("agentContactNo").length(), "Agent contact number should accept 10 characters");

        setField("agentStartDate", "2023-12-31");
        assertEquals(10, getField("agentStartDate").length(), "Agent start date should accept 10 characters");

        setField("agentEndDate", "2024-01-01");
        assertEquals(10, getField("agentEndDate").length(), "Agent end date should accept 10 characters");
    }

    @Test
    void testFieldLengthExceedsBoundary() {
        // Test that setting more than allowed length does not throw but stores as is (Java String)
        setField("agentCode", "12345678901"); // 11 chars
        assertEquals(11, getField("agentCode").length(), "Agent code should store more than 10 characters if set");

        setField("agentName", "A".repeat(31));
        assertEquals(31, getField("agentName").length(), "Agent name should store more than 30 characters if set");

        setField("agentAddress1", "B".repeat(51));
        assertEquals(51, getField("agentAddress1").length(), "Agent address 1 should store more than 50 characters if set");

        setField("agentAddress2", "C".repeat(51));
        assertEquals(51, getField("agentAddress2").length(), "Agent address 2 should store more than 50 characters if set");

        setField("agentCity", "D".repeat(21));
        assertEquals(21, getField("agentCity").length(), "Agent city should store more than 20 characters if set");

        setField("agentState", "CAL");
        assertEquals(3, getField("agentState").length(), "Agent state should store more than 2 characters if set");

        setField("agentZipCode", "12345678901");
        assertEquals(11, getField("agentZipCode").length(), "Agent zip code should store more than 10 characters if set");

        setField("agentStatus", "XY");
        assertEquals(2, getField("agentStatus").length(), "Agent status should store more than 1 character if set");

        setField("agentType", "TYPE1234567");
        assertEquals(11, getField("agentType").length(), "Agent type should store more than 10 characters if set");

        setField("agentEmail", "E".repeat(31));
        assertEquals(31, getField("agentEmail").length(), "Agent email should store more than 30 characters if set");

        setField("agentContactNo", "12345678901");
        assertEquals(11, getField("agentContactNo").length(), "Agent contact number should store more than 10 characters if set");

        setField("agentStartDate", "2023-12-311");
        assertEquals(11, getField("agentStartDate").length(), "Agent start date should store more than 10 characters if set");

        setField("agentEndDate", "2024-01-011");
        assertEquals(11, getField("agentEndDate").length(), "Agent end date should store more than 10 characters if set");
    }

    @Test
    void testEmptyStringValues() {
        setField("agentCode", "");
        setField("agentName", "");
        setField("agentAddress1", "");
        setField("agentAddress2", "");
        setField("agentCity", "");
        setField("agentState", "");
        setField("agentZipCode", "");
        setField("agentStatus", "");
        setField("agentType", "");
        setField("agentEmail", "");
        setField("agentContactNo", "");
        setField("agentStartDate", "");
        setField("agentEndDate", "");

        assertEquals("", getField("agentCode"), "Agent code should allow empty string");
        assertEquals("", getField("agentName"), "Agent name should allow empty string");
        assertEquals("", getField("agentAddress1"), "Agent address 1 should allow empty string");
        assertEquals("", getField("agentAddress2"), "Agent address 2 should allow empty string");
        assertEquals("", getField("agentCity"), "Agent city should allow empty string");
        assertEquals("", getField("agentState"), "Agent state should allow empty string");
        assertEquals("", getField("agentZipCode"), "Agent zip code should allow empty string");
        assertEquals("", getField("agentStatus"), "Agent status should allow empty string");
        assertEquals("", getField("agentType"), "Agent type should allow empty string");
        assertEquals("", getField("agentEmail"), "Agent email should allow empty string");
        assertEquals("", getField("agentContactNo"), "Agent contact number should allow empty string");
        assertEquals("", getField("agentStartDate"), "Agent start date should allow empty string");
        assertEquals("", getField("agentEndDate"), "Agent end date should allow empty string");
    }

    @Test
    void testAllFieldsSetAndRetrievedCorrectly() {
        setField("agentCode", "AGT9999999");
        setField("agentName", "Jane Smith");
        setField("agentAddress1", "456 Elm St");
        setField("agentAddress2", "Apt 2B");
        setField("agentCity", "Metropolis");
        setField("agentState", "NY");
        setField("agentZipCode", "10001");
        setField("agentStatus", "I");
        setField("agentType", "DIRECT");
        setField("agentEmail", "jane.smith@example.com");
        setField("agentContactNo", "5559876543");
        setField("agentStartDate", "2022-05-01");
        setField("agentEndDate", "2022-12-31");

        assertEquals("AGT9999999", getField("agentCode"));
        assertEquals("Jane Smith", getField("agentName"));
        assertEquals("456 Elm St", getField("agentAddress1"));
        assertEquals("Apt 2B", getField("agentAddress2"));
        assertEquals("Metropolis", getField("agentCity"));
        assertEquals("NY", getField("agentState"));
        assertEquals("10001", getField("agentZipCode"));
        assertEquals("I", getField("agentStatus"));
        assertEquals("DIRECT", getField("agentType"));
        assertEquals("jane.smith@example.com", getField("agentEmail"));
        assertEquals("5559876543", getField("agentContactNo"));
        assertEquals("2022-05-01", getField("agentStartDate"));
        assertEquals("2022-12-31", getField("agentEndDate"));
    }

    @Test
    void testEqualsAndHashCodeConsistency() {
        // Comment: This test assumes equals/hashCode are implemented or default (identity)
        AgentRecord agentRecord2 = new AgentRecord();
        setField("agentCode", "AGT1000000");
        try {
            Field field = AgentRecord.class.getDeclaredField("agentCode");
            field.setAccessible(true);
            field.set(agentRecord2, "AGT1000000");
        } catch (Exception e) {
            fail("Failed to set field for agentRecord2");
        }
        // Since equals/hashCode not overridden, should not be equal unless same instance
        assertNotEquals(agentRecord, agentRecord2, "Different instances should not be equal by default");
        assertNotEquals(agentRecord.hashCode(), agentRecord2.hashCode(), "Different instances should have different hash codes by default");
    }

    @Test
    void testToStringNotNull() {
        assertNotNull(agentRecord.toString(), "toString should not return null");
    }

    // Integration test for database mapping (Quarkus Entity)
    // This test verifies that the entity can be persisted and retrieved via JPA
    // NOTE: This test requires a configured test datasource and JPA setup in Quarkus
    @Test
    @Disabled("Enable if Quarkus test datasource and JPA are configured")
    void testPersistAndRetrieveAgentRecordEntity() {
        /*
        // Arrange
        EntityManager em = ... // inject or obtain from Quarkus
        agentRecord = new AgentRecord();
        setField("agentCode", "AGTDB10001");
        setField("agentName", "DB Agent");
        // ... set other fields

        // Act
        em.getTransaction().begin();
        em.persist(agentRecord);
        em.getTransaction().commit();

        AgentRecord found = em.find(AgentRecord.class, "AGTDB10001");

        // Assert
        assertNotNull(found, "AgentRecord should be persisted and found");
        assertEquals("DB Agent", getField("agentName"), "Agent name should match after retrieval");
        */
    }

    // Edge case: Test with special characters and unicode
    @Test
    void testFieldsWithSpecialCharactersAndUnicode() {
        setField("agentName", "Jöhn Dœ!@#$%^&*()");
        setField("agentAddress1", "123 Mäin Straße");
        setField("agentCity", "München");
        setField("agentEmail", "jöhn.dœ@example.com");

        assertEquals("Jöhn Dœ!@#$%^&*()", getField("agentName"), "Agent name should accept special characters and unicode");
        assertEquals("123 Mäin Straße", getField("agentAddress1"), "Agent address 1 should accept unicode");
        assertEquals("München", getField("agentCity"), "Agent city should accept unicode");
        assertEquals("jöhn.dœ@example.com", getField("agentEmail"), "Agent email should accept unicode");
    }

    // Edge case: Test with whitespace only
    @Test
    void testFieldsWithWhitespaceOnly() {
        setField("agentName", "     ");
        setField("agentAddress1", "   ");
        setField("agentCity", " ");
        setField("agentEmail", " ");

        assertEquals("     ", getField("agentName"), "Agent name should accept whitespace only");
        assertEquals("   ", getField("agentAddress1"), "Agent address 1 should accept whitespace only");
        assertEquals(" ", getField("agentCity"), "Agent city should accept whitespace only");
        assertEquals(" ", getField("agentEmail"), "Agent email should accept whitespace only");
    }

    // Edge case: Test with numeric strings in all fields
    @Test
    void testFieldsWithNumericStrings() {
        setField("agentCode", "1234567890");
        setField("agentName", "1234567890");
        setField("agentAddress1", "1234567890");
        setField("agentAddress2", "1234567890");
        setField("agentCity", "1234567890");
        setField("agentState", "12");
        setField("agentZipCode", "1234567890");
        setField("agentStatus", "1");
        setField("agentType", "1234567890");
        setField("agentEmail", "1234567890");
        setField("agentContactNo", "1234567890");
        setField("agentStartDate", "1234567890");
        setField("agentEndDate", "1234567890");

        assertEquals("1234567890", getField("agentCode"));
        assertEquals("1234567890", getField("agentName"));
        assertEquals("1234567890", getField("agentAddress1"));
        assertEquals("1234567890", getField("agentAddress2"));
        assertEquals("1234567890", getField("agentCity"));
        assertEquals("12", getField("agentState"));
        assertEquals("1234567890", getField("agentZipCode"));
        assertEquals("1", getField("agentStatus"));
        assertEquals("1234567890", getField("agentType"));
        assertEquals("1234567890", getField("agentEmail"));
        assertEquals("1234567890", getField("agentContactNo"));
        assertEquals("1234567890", getField("agentStartDate"));
        assertEquals("1234567890", getField("agentEndDate"));
    }
}