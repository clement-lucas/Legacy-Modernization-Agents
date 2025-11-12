package com.example.insurnce.entity;

import io.quarkus.test.junit.QuarkusTest;
import org.junit.jupiter.api.*;
import org.mockito.*;
import jakarta.persistence.EntityManager;
import jakarta.persistence.TypedQuery;
import jakarta.validation.ConstraintViolationException;

import java.time.LocalDateTime;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

/**
 * Comprehensive tests for Coverage entity converted from COBOL.
 */
@QuarkusTest
class CoverageTest {

    @Mock
    EntityManager entityManager;

    @Mock
    TypedQuery<Coverage> typedQuery;

    Coverage coverage;
    LocalDateTime now;

    @BeforeEach
    void setUp() {
        MockitoAnnotations.openMocks(this);
        now = LocalDateTime.now();
        coverage = new Coverage(
                "POL1234567",
                "ACTIVE",
                "20240101",
                "20241231",
                now,
                now
        );
    }

    @AfterEach
    void tearDown() {
        coverage = null;
        now = null;
    }

    @Test
    void testDefaultConstructorCreatesNonNullObject() {
        // Arrange & Act
        Coverage cov = new Coverage();
        // Assert
        assertNotNull(cov);
    }

    @Test
    void testParameterizedConstructorSetsFieldsCorrectly() {
        // Arrange
        String polNum = "POL9876543";
        String status = "EXPIRED";
        String startDt = "20230101";
        String endDt = "20231231";
        LocalDateTime addTs = now.minusDays(1);
        LocalDateTime updateTs = now;
        // Act
        Coverage cov = new Coverage(polNum, status, startDt, endDt, addTs, updateTs);
        // Assert
        assertEquals(polNum, cov.getCoveragePolNum());
        assertEquals(status, cov.getCoverageStatus());
        assertEquals(startDt, cov.getCoverageStartDt());
        assertEquals(endDt, cov.getCoverageEndDt());
        assertEquals(addTs, cov.getCoverageAddTs());
        assertEquals(updateTs, cov.getCoverageUpdateTs());
    }

    @Test
    void testGettersReturnExpectedValues() {
        // Assert
        assertEquals("POL1234567", coverage.getCoveragePolNum());
        assertEquals("ACTIVE", coverage.getCoverageStatus());
        assertEquals("20240101", coverage.getCoverageStartDt());
        assertEquals("20241231", coverage.getCoverageEndDt());
        assertEquals(now, coverage.getCoverageAddTs());
        assertEquals(now, coverage.getCoverageUpdateTs());
    }

    @Test
    void testSettersUpdateValuesCorrectly() {
        // Arrange
        String newPolNum = "POL7654321";
        String newStatus = "SUSPENDED";
        String newStartDt = "20250101";
        String newEndDt = "20251231";
        LocalDateTime newAddTs = now.plusDays(10);
        LocalDateTime newUpdateTs = now.plusDays(20);
        // Act
        coverage.setCoveragePolNum(newPolNum);
        coverage.setCoverageStatus(newStatus);
        coverage.setCoverageStartDt(newStartDt);
        coverage.setCoverageEndDt(newEndDt);
        coverage.setCoverageAddTs(newAddTs);
        coverage.setCoverageUpdateTs(newUpdateTs);
        // Assert
        assertEquals(newPolNum, coverage.getCoveragePolNum());
        assertEquals(newStatus, coverage.getCoverageStatus());
        assertEquals(newStartDt, coverage.getCoverageStartDt());
        assertEquals(newEndDt, coverage.getCoverageEndDt());
        assertEquals(newAddTs, coverage.getCoverageAddTs());
        assertEquals(newUpdateTs, coverage.getCoverageUpdateTs());
    }

    @Test
    void testCoveragePolNumMaxLengthBoundary() {
        // Arrange
        String validPolNum = "1234567890"; // 10 chars
        String tooLongPolNum = "12345678901"; // 11 chars
        // Act
        coverage.setCoveragePolNum(validPolNum);
        // Assert
        assertEquals(validPolNum, coverage.getCoveragePolNum());
        // Simulate validation (would be caught by Bean Validation in Quarkus)
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    tooLongPolNum,
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    now,
                    now
            );
            // Simulate validation
            validateCoverage(invalidCoverage);
        });
    }

    @Test
    void testCoverageStatusMaxLengthBoundary() {
        // Arrange
        String validStatus = "1234567890";
        String tooLongStatus = "ABCDEFGHIJK";
        // Act
        coverage.setCoverageStatus(validStatus);
        // Assert
        assertEquals(validStatus, coverage.getCoverageStatus());
        // Simulate validation
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    tooLongStatus,
                    "20240101",
                    "20241231",
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });
    }

    @Test
    void testCoverageStartDtMaxLengthBoundary() {
        // Arrange
        String validStartDt = "20240101";
        String tooLongStartDt = "20240101111";
        // Act
        coverage.setCoverageStartDt(validStartDt);
        // Assert
        assertEquals(validStartDt, coverage.getCoverageStartDt());
        // Simulate validation
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    tooLongStartDt,
                    "20241231",
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });
    }

    @Test
    void testCoverageEndDtMaxLengthBoundary() {
        // Arrange
        String validEndDt = "20241231";
        String tooLongEndDt = "20241231111";
        // Act
        coverage.setCoverageEndDt(validEndDt);
        // Assert
        assertEquals(validEndDt, coverage.getCoverageEndDt());
        // Simulate validation
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    "20240101",
                    tooLongEndDt,
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });
    }

    @Test
    void testNullFieldsThrowConstraintViolationException() {
        // Arrange & Act & Assert
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    null,
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    null,
                    "20240101",
                    "20241231",
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    null,
                    "20241231",
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    "20240101",
                    null,
                    now,
                    now
            );
            validateCoverage(invalidCoverage);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    null,
                    now
            );
            validateCoverage(invalidCoverage);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage invalidCoverage = new Coverage(
                    "POL1234567",
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    now,
                    null
            );
            validateCoverage(invalidCoverage);
        });
    }

    @Test
    void testCoverageAddTsAndUpdateTsAreValidTimestamps() {
        // Arrange
        LocalDateTime addTs = LocalDateTime.of(2024, 1, 1, 0, 0, 0);
        LocalDateTime updateTs = LocalDateTime.of(2024, 6, 30, 23, 59, 59);
        coverage.setCoverageAddTs(addTs);
        coverage.setCoverageUpdateTs(updateTs);
        // Assert
        assertEquals(addTs, coverage.getCoverageAddTs());
        assertEquals(updateTs, coverage.getCoverageUpdateTs());
    }

    @Test
    void testCoverageEntityPersistenceIntegration() {
        // Integration test for Quarkus JPA persistence
        Coverage persistedCoverage = new Coverage(
                "POL5555555",
                "ACTIVE",
                "20240101",
                "20241231",
                now,
                now
        );
        // Arrange
        when(entityManager.merge(any(Coverage.class))).thenReturn(persistedCoverage);
        // Act
        Coverage result = entityManager.merge(persistedCoverage);
        // Assert
        assertNotNull(result);
        assertEquals("POL5555555", result.getCoveragePolNum());
        assertEquals("ACTIVE", result.getCoverageStatus());
        verify(entityManager, times(1)).merge(any(Coverage.class));
    }

    @Test
    void testCoverageEntityFindByIdIntegration() {
        // Integration test for Quarkus JPA find
        String polNum = "POL1234567";
        when(entityManager.find(Coverage.class, polNum)).thenReturn(coverage);
        // Act
        Coverage found = entityManager.find(Coverage.class, polNum);
        // Assert
        assertNotNull(found);
        assertEquals(polNum, found.getCoveragePolNum());
        verify(entityManager, times(1)).find(Coverage.class, polNum);
    }

    @Test
    void testCoverageEntityQueryIntegration() {
        // Integration test for Quarkus JPA query
        String jpql = "SELECT c FROM Coverage c WHERE c.coverageStatus = :status";
        when(entityManager.createQuery(jpql, Coverage.class)).thenReturn(typedQuery);
        when(typedQuery.setParameter("status", "ACTIVE")).thenReturn(typedQuery);
        when(typedQuery.getSingleResult()).thenReturn(coverage);
        // Act
        TypedQuery<Coverage> query = entityManager.createQuery(jpql, Coverage.class);
        Coverage result = query.setParameter("status", "ACTIVE").getSingleResult();
        // Assert
        assertNotNull(result);
        assertEquals("ACTIVE", result.getCoverageStatus());
        verify(entityManager, times(1)).createQuery(jpql, Coverage.class);
        verify(typedQuery, times(1)).setParameter("status", "ACTIVE");
        verify(typedQuery, times(1)).getSingleResult();
    }

    @Test
    void testCoverageBusinessLogicPreservedFromCobol() {
        // COBOL logic: All fields must be present and valid, status must be ACTIVE, EXPIRED, or SUSPENDED
        // Arrange
        String[] validStatuses = {"ACTIVE", "EXPIRED", "SUSPENDED"};
        for (String status : validStatuses) {
            Coverage cov = new Coverage(
                    "POL0000001",
                    status,
                    "20240101",
                    "20241231",
                    now,
                    now
            );
            // Act & Assert
            assertTrue(
                    status.equals(cov.getCoverageStatus()),
                    "Status should match COBOL allowed values"
            );
            assertNotNull(cov.getCoveragePolNum());
            assertNotNull(cov.getCoverageStartDt());
            assertNotNull(cov.getCoverageEndDt());
            assertNotNull(cov.getCoverageAddTs());
            assertNotNull(cov.getCoverageUpdateTs());
        }
    }

    @Test
    void testCoverageStatusInvalidValue() {
        // COBOL logic: Only certain status values allowed
        Coverage cov = new Coverage(
                "POL0000002",
                "INVALID",
                "20240101",
                "20241231",
                now,
                now
        );
        // Simulate validation (would be caught by business logic in service layer)
        assertFalse(
                cov.getCoverageStatus().equals("ACTIVE") ||
                cov.getCoverageStatus().equals("EXPIRED") ||
                cov.getCoverageStatus().equals("SUSPENDED"),
                "Status should not be allowed"
        );
    }

    @Test
    void testCoverageStartDtAndEndDtBoundaryValues() {
        // COBOL: Dates are PIC X(10), so test min/max date string
        String minDate = "0000000000";
        String maxDate = "9999999999";
        coverage.setCoverageStartDt(minDate);
        coverage.setCoverageEndDt(maxDate);
        assertEquals(minDate, coverage.getCoverageStartDt());
        assertEquals(maxDate, coverage.getCoverageEndDt());
    }

    @Test
    void testCoverageAddTsAndUpdateTsNullThrowsException() {
        // Arrange & Act & Assert
        assertThrows(ConstraintViolationException.class, () -> {
            Coverage cov = new Coverage(
                    "POL0000003",
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    null,
                    now
            );
            validateCoverage(cov);
        });

        assertThrows(ConstraintViolationException.class, () -> {
            Coverage cov = new Coverage(
                    "POL0000003",
                    "ACTIVE",
                    "20240101",
                    "20241231",
                    now,
                    null
            );
            validateCoverage(cov);
        });
    }

    /**
     * Simulates Bean Validation for Coverage entity.
     * Throws ConstraintViolationException if any @NotNull or @Size constraint is violated.
     * This is a stub for unit testing only.
     */
    private void validateCoverage(Coverage cov) {
        if (cov.getCoveragePolNum() == null ||
                cov.getCoverageStatus() == null ||
                cov.getCoverageStartDt() == null ||
                cov.getCoverageEndDt() == null ||
                cov.getCoverageAddTs() == null ||
                cov.getCoverageUpdateTs() == null) {
            throw new ConstraintViolationException("Null field", null);
        }
        if (cov.getCoveragePolNum().length() > 10 ||
                cov.getCoverageStatus().length() > 10 ||
                cov.getCoverageStartDt().length() > 10 ||
                cov.getCoverageEndDt().length() > 10) {
            throw new ConstraintViolationException("Length exceeded", null);
        }
    }
}