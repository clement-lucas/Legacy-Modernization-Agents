package com.example.insurnce;

import io.quarkus.test.junit.QuarkusTest;
import io.quarkus.hibernate.orm.panache.PanacheEntityBase;
import jakarta.transaction.Transactional;
import org.junit.jupiter.api.*;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

@QuarkusTest
@TestInstance(TestInstance.Lifecycle.PER_CLASS)
class TTrackingTest {

    private TTracking tTracking;
    private final String validPolicyNumber = "POL1234567";
    private final LocalDate validNotifyDate = LocalDate.of(2024, 6, 1);
    private final String validStatus = "A";
    private final LocalDateTime validAddTimestamp = LocalDateTime.of(2024, 6, 1, 10, 30, 0);
    private final LocalDateTime validUpdateTimestamp = LocalDateTime.of(2024, 6, 2, 11, 45, 0);

    @BeforeEach
    void setUp() {
        tTracking = new TTracking(validPolicyNumber, validNotifyDate, validStatus, validAddTimestamp, validUpdateTimestamp);
    }

    @AfterEach
    @Transactional
    void tearDown() {
        // Clean up persisted entities after each test
        TTracking.deleteAll();
    }

    @Test
    void testDefaultConstructorCreatesEmptyObject() {
        TTracking empty = new TTracking();
        assertNull(empty.policyNumber);
        assertNull(empty.notifyDate);
        assertNull(empty.status);
        assertNull(empty.addTimestamp);
        assertNull(empty.updateTimestamp);
    }

    @Test
    void testAllArgsConstructorSetsFieldsCorrectly() {
        assertEquals(validPolicyNumber, tTracking.policyNumber);
        assertEquals(validNotifyDate, tTracking.notifyDate);
        assertEquals(validStatus, tTracking.status);
        assertEquals(validAddTimestamp, tTracking.addTimestamp);
        assertEquals(validUpdateTimestamp, tTracking.updateTimestamp);
    }

    @Test
    void testPolicyNumberMaxLengthBoundary() {
        String maxLenPolicy = "1234567890";
        TTracking entity = new TTracking(maxLenPolicy, validNotifyDate, validStatus, validAddTimestamp, validUpdateTimestamp);
        assertEquals(10, entity.policyNumber.length());
    }

    @Test
    void testPolicyNumberTooLongThrowsException() {
        String tooLongPolicy = "12345678901"; // 11 chars
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(tooLongPolicy, validNotifyDate, validStatus, validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
        // DB2 schema should reject this, but Panache may throw on flush
    }

    @Test
    void testStatusSingleCharBoundary() {
        TTracking entity = new TTracking(validPolicyNumber, validNotifyDate, "Z", validAddTimestamp, validUpdateTimestamp);
        assertEquals(1, entity.status.length());
    }

    @Test
    void testStatusEmptyStringThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(validPolicyNumber, validNotifyDate, "", validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
        // DB2 schema should reject this, but Panache may throw on flush
    }

    @Test
    void testNotifyDateNotNull() {
        assertNotNull(tTracking.notifyDate);
    }

    @Test
    void testAddTimestampNotNull() {
        assertNotNull(tTracking.addTimestamp);
    }

    @Test
    void testUpdateTimestampNotNull() {
        assertNotNull(tTracking.updateTimestamp);
    }

    @Test
    void testPersistAndFindByIdIntegration() {
        // Integration test: persist and retrieve by ID
        tTracking.persistAndFlush();
        TTracking found = TTracking.findById(validPolicyNumber);
        assertNotNull(found);
        assertEquals(validPolicyNumber, found.policyNumber);
        assertEquals(validNotifyDate, found.notifyDate);
        assertEquals(validStatus, found.status);
        assertEquals(validAddTimestamp, found.addTimestamp);
        assertEquals(validUpdateTimestamp, found.updateTimestamp);
    }

    @Test
    void testPersistMultipleEntitiesAndListAll() {
        TTracking t1 = new TTracking("POL0000001", validNotifyDate, "A", validAddTimestamp, validUpdateTimestamp);
        TTracking t2 = new TTracking("POL0000002", validNotifyDate.plusDays(1), "B", validAddTimestamp.plusHours(1), validUpdateTimestamp.plusHours(1));
        t1.persistAndFlush();
        t2.persistAndFlush();

        List<TTracking> all = TTracking.listAll();
        assertEquals(2, all.size());
        assertTrue(all.stream().anyMatch(e -> e.policyNumber.equals("POL0000001")));
        assertTrue(all.stream().anyMatch(e -> e.policyNumber.equals("POL0000002")));
    }

    @Test
    void testDeleteEntity() {
        tTracking.persistAndFlush();
        TTracking found = TTracking.findById(validPolicyNumber);
        assertNotNull(found);
        found.delete();
        assertNull(TTracking.findById(validPolicyNumber));
    }

    @Test
    void testFindByNonExistentIdReturnsNull() {
        assertNull(TTracking.findById("NONEXISTENT"));
    }

    @Test
    void testNullPolicyNumberThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(null, validNotifyDate, validStatus, validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
    }

    @Test
    void testNullNotifyDateThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(validPolicyNumber, null, validStatus, validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
    }

    @Test
    void testNullStatusThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(validPolicyNumber, validNotifyDate, null, validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
    }

    @Test
    void testNullAddTimestampThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(validPolicyNumber, validNotifyDate, validStatus, null, validUpdateTimestamp);
            entity.persistAndFlush();
        });
    }

    @Test
    void testNullUpdateTimestampThrowsException() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking(validPolicyNumber, validNotifyDate, validStatus, validAddTimestamp, null);
            entity.persistAndFlush();
        });
    }

    @Test
    void testPersistDuplicatePrimaryKeyThrowsException() {
        tTracking.persistAndFlush();
        TTracking duplicate = new TTracking(validPolicyNumber, validNotifyDate, validStatus, validAddTimestamp, validUpdateTimestamp);
        Exception ex = assertThrows(Exception.class, duplicate::persistAndFlush);
    }

    @Test
    void testTimestampPrecisionPreserved() {
        LocalDateTime preciseTimestamp = LocalDateTime.of(2024, 6, 1, 10, 30, 15, 123456789);
        TTracking entity = new TTracking("POLPREC001", validNotifyDate, "A", preciseTimestamp, preciseTimestamp);
        entity.persistAndFlush();
        TTracking found = TTracking.findById("POLPREC001");
        assertEquals(preciseTimestamp, found.addTimestamp);
        assertEquals(preciseTimestamp, found.updateTimestamp);
    }

    @Test
    void testBusinessLogicStatusActive() {
        // COBOL logic: status "A" means active
        TTracking active = new TTracking("POLACTIVE", validNotifyDate, "A", validAddTimestamp, validUpdateTimestamp);
        active.persistAndFlush();
        TTracking found = TTracking.findById("POLACTIVE");
        assertEquals("A", found.status);
    }

    @Test
    void testBusinessLogicStatusInactive() {
        // COBOL logic: status "I" means inactive
        TTracking inactive = new TTracking("POLINACTIV", validNotifyDate, "I", validAddTimestamp, validUpdateTimestamp);
        inactive.persistAndFlush();
        TTracking found = TTracking.findById("POLINACTIV");
        assertEquals("I", found.status);
    }

    @Test
    void testBoundaryDates() {
        LocalDate minDate = LocalDate.of(1900, 1, 1);
        LocalDate maxDate = LocalDate.of(9999, 12, 31);
        TTracking minEntity = new TTracking("POLMINDATE", minDate, "A", validAddTimestamp, validUpdateTimestamp);
        TTracking maxEntity = new TTracking("POLMAXDATE", maxDate, "A", validAddTimestamp, validUpdateTimestamp);
        minEntity.persistAndFlush();
        maxEntity.persistAndFlush();
        assertEquals(minDate, TTracking.findById("POLMINDATE").notifyDate);
        assertEquals(maxDate, TTracking.findById("POLMAXDATE").notifyDate);
    }

    @Test
    void testBoundaryTimestamps() {
        LocalDateTime minTimestamp = LocalDateTime.of(1900, 1, 1, 0, 0, 0);
        LocalDateTime maxTimestamp = LocalDateTime.of(9999, 12, 31, 23, 59, 59);
        TTracking minEntity = new TTracking("POLMINTSMP", validNotifyDate, "A", minTimestamp, minTimestamp);
        TTracking maxEntity = new TTracking("POLMAXTSMP", validNotifyDate, "A", maxTimestamp, maxTimestamp);
        minEntity.persistAndFlush();
        maxEntity.persistAndFlush();
        assertEquals(minTimestamp, TTracking.findById("POLMINTSMP").addTimestamp);
        assertEquals(maxTimestamp, TTracking.findById("POLMAXTSMP").addTimestamp);
    }

    // Additional test for PanacheEntityBase equals/hashCode contract
    @Test
    void testEqualsAndHashCodeContract() {
        TTracking t1 = new TTracking("POLHASH001", validNotifyDate, "A", validAddTimestamp, validUpdateTimestamp);
        TTracking t2 = new TTracking("POLHASH001", validNotifyDate, "A", validAddTimestamp, validUpdateTimestamp);
        assertNotEquals(t1, t2); // PanacheEntityBase does not override equals/hashCode by default
    }

    // Test for toString method (inherited from Object)
    @Test
    void testToStringNotNull() {
        assertNotNull(tTracking.toString());
    }

    // Test for business logic: status must be one of allowed values (A/I/C)
    @Test
    void testStatusAllowedValues() {
        String[] allowed = {"A", "I", "C"};
        for (String s : allowed) {
            TTracking entity = new TTracking("POL" + s + "VAL", validNotifyDate, s, validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
            TTracking found = TTracking.findById("POL" + s + "VAL");
            assertEquals(s, found.status);
        }
    }

    // Test for business logic: status not allowed value should throw
    @Test
    void testStatusNotAllowedValueThrows() {
        Exception ex = assertThrows(Exception.class, () -> {
            TTracking entity = new TTracking("POLBADVAL", validNotifyDate, "X", validAddTimestamp, validUpdateTimestamp);
            entity.persistAndFlush();
        });
    }
}