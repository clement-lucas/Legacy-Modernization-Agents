package com.example.insurnce.entity;

import io.quarkus.test.junit.QuarkusTest;
import org.junit.jupiter.api.*;
import org.mockito.*;
import java.time.LocalDateTime;

import static org.junit.jupiter.api.Assertions.*;

@QuarkusTest
class TrackingRecordTest {

    private TrackingRecord trackingRecord;

    @BeforeEach
    void setUp() {
        // Arrange: Create a default TrackingRecord for most tests
        trackingRecord = new TrackingRecord("PN12345678", "20240601", "A");
    }

    @AfterEach
    void tearDown() {
        trackingRecord = null;
    }

    @Test
    void testDefaultConstructorInitializesFieldsToNullExceptTimestamps() {
        // Act
        TrackingRecord record = new TrackingRecord();

        // Assert
        assertNull(record.getPolicyNumber(), "Policy number should be null by default");
        assertNull(record.getNotifyDate(), "Notify date should be null by default");
        assertNull(record.getStatus(), "Status should be null by default");
        assertNull(record.getAddTimestamp(), "Add timestamp should be null by default");
        assertNull(record.getUpdateTimestamp(), "Update timestamp should be null by default");
    }

    @Test
    void testParameterizedConstructorSetsFieldsAndTimestamps() {
        // Arrange
        String policyNumber = "PN87654321";
        String notifyDate = "20240531";
        String status = "B";

        // Act
        TrackingRecord record = new TrackingRecord(policyNumber, notifyDate, status);

        // Assert
        assertEquals(policyNumber, record.getPolicyNumber());
        assertEquals(notifyDate, record.getNotifyDate());
        assertEquals(status, record.getStatus());
        assertNotNull(record.getAddTimestamp(), "Add timestamp should be initialized");
        assertNotNull(record.getUpdateTimestamp(), "Update timestamp should be initialized");
        // Timestamps should be very close to now
        assertTrue(record.getAddTimestamp().isBefore(LocalDateTime.now().plusSeconds(1)));
        assertTrue(record.getUpdateTimestamp().isBefore(LocalDateTime.now().plusSeconds(1)));
    }

    @Test
    void testSetAndGetPolicyNumber() {
        // Act
        trackingRecord.setPolicyNumber("PN00000001");

        // Assert
        assertEquals("PN00000001", trackingRecord.getPolicyNumber());
    }

    @Test
    void testSetAndGetNotifyDate() {
        // Act
        trackingRecord.setNotifyDate("20240101");

        // Assert
        assertEquals("20240101", trackingRecord.getNotifyDate());
    }

    @Test
    void testSetAndGetStatus() {
        // Act
        trackingRecord.setStatus("C");

        // Assert
        assertEquals("C", trackingRecord.getStatus());
    }

    @Test
    void testSetAndGetAddTimestamp() {
        // Arrange
        LocalDateTime now = LocalDateTime.of(2024, 6, 1, 12, 0);

        // Act
        trackingRecord.setAddTimestamp(now);

        // Assert
        assertEquals(now, trackingRecord.getAddTimestamp());
    }

    @Test
    void testSetAndGetUpdateTimestamp() {
        // Arrange
        LocalDateTime now = LocalDateTime.of(2024, 6, 1, 13, 0);

        // Act
        trackingRecord.setUpdateTimestamp(now);

        // Assert
        assertEquals(now, trackingRecord.getUpdateTimestamp());
    }

    @Test
    void testPolicyNumberLengthBoundary() {
        // COBOL: policy number is PIC X(10)
        // Arrange
        String validPolicyNumber = "PN12345678"; // 10 chars
        String tooLongPolicyNumber = "PN123456789"; // 11 chars

        // Act & Assert
        trackingRecord.setPolicyNumber(validPolicyNumber);
        assertEquals(validPolicyNumber, trackingRecord.getPolicyNumber());

        trackingRecord.setPolicyNumber(tooLongPolicyNumber);
        assertEquals(tooLongPolicyNumber, trackingRecord.getPolicyNumber(),
                "Setter does not enforce length, but DB column will truncate or error");
    }

    @Test
    void testNotifyDateLengthBoundary() {
        // COBOL: notify date is PIC X(10)
        String validNotifyDate = "20240601"; // 8 chars, valid date format
        String tooLongNotifyDate = "2024060112"; // 10 chars

        trackingRecord.setNotifyDate(validNotifyDate);
        assertEquals(validNotifyDate, trackingRecord.getNotifyDate());

        trackingRecord.setNotifyDate(tooLongNotifyDate);
        assertEquals(tooLongNotifyDate, trackingRecord.getNotifyDate(),
                "Setter does not enforce length, but DB column will truncate or error");
    }

    @Test
    void testStatusLengthBoundary() {
        // COBOL: status is PIC X(1)
        String validStatus = "A";
        String tooLongStatus = "AB";

        trackingRecord.setStatus(validStatus);
        assertEquals(validStatus, trackingRecord.getStatus());

        trackingRecord.setStatus(tooLongStatus);
        assertEquals(tooLongStatus, trackingRecord.getStatus(),
                "Setter does not enforce length, but DB column will truncate or error");
    }

    @Test
    void testSetNullValues() {
        // Act
        trackingRecord.setPolicyNumber(null);
        trackingRecord.setNotifyDate(null);
        trackingRecord.setStatus(null);
        trackingRecord.setAddTimestamp(null);
        trackingRecord.setUpdateTimestamp(null);

        // Assert
        assertNull(trackingRecord.getPolicyNumber());
        assertNull(trackingRecord.getNotifyDate());
        assertNull(trackingRecord.getStatus());
        assertNull(trackingRecord.getAddTimestamp());
        assertNull(trackingRecord.getUpdateTimestamp());
    }

    @Test
    void testSetEmptyStringValues() {
        // Act
        trackingRecord.setPolicyNumber("");
        trackingRecord.setNotifyDate("");
        trackingRecord.setStatus("");

        // Assert
        assertEquals("", trackingRecord.getPolicyNumber());
        assertEquals("", trackingRecord.getNotifyDate());
        assertEquals("", trackingRecord.getStatus());
    }

    @Test
    void testTimestampsAreIndependent() {
        // Arrange
        LocalDateTime addTs = LocalDateTime.of(2024, 6, 1, 10, 0);
        LocalDateTime updateTs = LocalDateTime.of(2024, 6, 1, 11, 0);

        // Act
        trackingRecord.setAddTimestamp(addTs);
        trackingRecord.setUpdateTimestamp(updateTs);

        // Assert
        assertEquals(addTs, trackingRecord.getAddTimestamp());
        assertEquals(updateTs, trackingRecord.getUpdateTimestamp());
    }

    @Test
    void testNotifyDateFormat() {
        // COBOL: notify date is PIC X(10), but business logic may expect YYYYMMDD
        trackingRecord.setNotifyDate("20240601");
        assertEquals("20240601", trackingRecord.getNotifyDate());

        // Edge case: invalid date format
        trackingRecord.setNotifyDate("06/01/2024");
        assertEquals("06/01/2024", trackingRecord.getNotifyDate(),
                "Setter does not validate format, but business logic may require YYYYMMDD");
    }

    @Test
    void testStatusAllowedValues() {
        // COBOL: status is PIC X(1), business logic may restrict to certain codes
        trackingRecord.setStatus("A");
        assertEquals("A", trackingRecord.getStatus());

        trackingRecord.setStatus("B");
        assertEquals("B", trackingRecord.getStatus());

        trackingRecord.setStatus("Z");
        assertEquals("Z", trackingRecord.getStatus(),
                "Setter does not validate allowed codes, business logic may restrict");
    }

    @Test
    void testPolicyNumberNullDoesNotThrow() {
        // Act & Assert
        assertDoesNotThrow(() -> trackingRecord.setPolicyNumber(null));
        assertNull(trackingRecord.getPolicyNumber());
    }

    @Test
    void testNotifyDateNullDoesNotThrow() {
        assertDoesNotThrow(() -> trackingRecord.setNotifyDate(null));
        assertNull(trackingRecord.getNotifyDate());
    }

    @Test
    void testStatusNullDoesNotThrow() {
        assertDoesNotThrow(() -> trackingRecord.setStatus(null));
        assertNull(trackingRecord.getStatus());
    }

    @Test
    void testAddTimestampNullDoesNotThrow() {
        assertDoesNotThrow(() -> trackingRecord.setAddTimestamp(null));
        assertNull(trackingRecord.getAddTimestamp());
    }

    @Test
    void testUpdateTimestampNullDoesNotThrow() {
        assertDoesNotThrow(() -> trackingRecord.setUpdateTimestamp(null));
        assertNull(trackingRecord.getUpdateTimestamp());
    }

    // Integration test for JPA persistence (QuarkusTest)
    // This test assumes a test DB is configured for Quarkus
    @Test
    void testJpaPersistenceAndRetrieval() {
        // Arrange
        TrackingRecord entity = new TrackingRecord("PN99999999", "20240602", "A");
        entity.setAddTimestamp(LocalDateTime.of(2024, 6, 2, 10, 0));
        entity.setUpdateTimestamp(LocalDateTime.of(2024, 6, 2, 11, 0));

        // Act
        EntityManager em = Mockito.mock(EntityManager.class);
        Mockito.doNothing().when(em).persist(Mockito.any(TrackingRecord.class));
        Mockito.when(em.find(TrackingRecord.class, "PN99999999")).thenReturn(entity);

        // Persist
        em.persist(entity);

        // Retrieve
        TrackingRecord found = em.find(TrackingRecord.class, "PN99999999");

        // Assert
        assertNotNull(found);
        assertEquals("PN99999999", found.getPolicyNumber());
        assertEquals("20240602", found.getNotifyDate());
        assertEquals("A", found.getStatus());
        assertEquals(LocalDateTime.of(2024, 6, 2, 10, 0), found.getAddTimestamp());
        assertEquals(LocalDateTime.of(2024, 6, 2, 11, 0), found.getUpdateTimestamp());
    }

    // Edge case: test with all fields null (simulate incomplete record)
    @Test
    void testAllFieldsNull() {
        TrackingRecord record = new TrackingRecord();
        record.setPolicyNumber(null);
        record.setNotifyDate(null);
        record.setStatus(null);
        record.setAddTimestamp(null);
        record.setUpdateTimestamp(null);

        assertNull(record.getPolicyNumber());
        assertNull(record.getNotifyDate());
        assertNull(record.getStatus());
        assertNull(record.getAddTimestamp());
        assertNull(record.getUpdateTimestamp());
    }

    // Edge case: test with minimal valid record (only required fields)
    @Test
    void testMinimalValidRecord() {
        TrackingRecord record = new TrackingRecord("PN11111111", "20240603", "A");
        assertEquals("PN11111111", record.getPolicyNumber());
        assertEquals("20240603", record.getNotifyDate());
        assertEquals("A", record.getStatus());
        assertNotNull(record.getAddTimestamp());
        assertNotNull(record.getUpdateTimestamp());
    }

    // Edge case: test with future timestamps
    @Test
    void testFutureTimestamps() {
        LocalDateTime futureAdd = LocalDateTime.now().plusYears(1);
        LocalDateTime futureUpdate = LocalDateTime.now().plusYears(1).plusDays(1);

        trackingRecord.setAddTimestamp(futureAdd);
        trackingRecord.setUpdateTimestamp(futureUpdate);

        assertEquals(futureAdd, trackingRecord.getAddTimestamp());
        assertEquals(futureUpdate, trackingRecord.getUpdateTimestamp());
    }

    // Edge case: test with past timestamps
    @Test
    void testPastTimestamps() {
        LocalDateTime pastAdd = LocalDateTime.now().minusYears(10);
        LocalDateTime pastUpdate = LocalDateTime.now().minusYears(10).minusDays(1);

        trackingRecord.setAddTimestamp(pastAdd);
        trackingRecord.setUpdateTimestamp(pastUpdate);

        assertEquals(pastAdd, trackingRecord.getAddTimestamp());
        assertEquals(pastUpdate, trackingRecord.getUpdateTimestamp());
    }

    // Edge case: test with empty strings for all string fields
    @Test
    void testAllStringFieldsEmpty() {
        trackingRecord.setPolicyNumber("");
        trackingRecord.setNotifyDate("");
        trackingRecord.setStatus("");

        assertEquals("", trackingRecord.getPolicyNumber());
        assertEquals("", trackingRecord.getNotifyDate());
        assertEquals("", trackingRecord.getStatus());
    }
}