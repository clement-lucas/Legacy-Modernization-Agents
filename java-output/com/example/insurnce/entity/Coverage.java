package com.example.insurnce.entity;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import java.time.LocalDateTime;

/**
 * Java entity mapping for COBOL group variable DCLCOVGE and DB2 table INSURNCE.TCOVERAG.
 * 
 * Conversion notes:
 * - COBOL PIC X(10) → Java String with @Size(max=10)
 * - COBOL PIC X(26) for TIMESTAMP → Java LocalDateTime
 * - Duplicate COVERAGE-STATUS removed
 * - Error handling is managed via JPA/Quarkus exceptions and validation
 */
@Entity
@Table(name = "TCOVERAG", schema = "INSURNCE")
public class Coverage {

    @Id
    @Column(name = "COVERAGE_POL_NUM", length = 10, nullable = false)
    @NotNull
    @Size(max = 10)
    private String coveragePolNum;

    @Column(name = "COVERAGE_STATUS", length = 10, nullable = false)
    @NotNull
    @Size(max = 10)
    private String coverageStatus;

    @Column(name = "COVERAGE_START_DT", length = 10, nullable = false)
    @NotNull
    @Size(max = 10)
    private String coverageStartDt;

    @Column(name = "COVERAGE_END_DT", length = 10, nullable = false)
    @NotNull
    @Size(max = 10)
    private String coverageEndDt;

    @Column(name = "COVERAGE_ADD_TS", nullable = false)
    @NotNull
    private LocalDateTime coverageAddTs;

    @Column(name = "COVERAGE_UPDATE_TS", nullable = false)
    @NotNull
    private LocalDateTime coverageUpdateTs;

    // Constructors
    public Coverage() {}

    public Coverage(String coveragePolNum, String coverageStatus, String coverageStartDt, String coverageEndDt,
                    LocalDateTime coverageAddTs, LocalDateTime coverageUpdateTs) {
        this.coveragePolNum = coveragePolNum;
        this.coverageStatus = coverageStatus;
        this.coverageStartDt = coverageStartDt;
        this.coverageEndDt = coverageEndDt;
        this.coverageAddTs = coverageAddTs;
        this.coverageUpdateTs = coverageUpdateTs;
    }

    // Getters and Setters
    public String getCoveragePolNum() {
        return coveragePolNum;
    }

    public void setCoveragePolNum(String coveragePolNum) {
        this.coveragePolNum = coveragePolNum;
    }

    public String getCoverageStatus() {
        return coverageStatus;
    }

    public void setCoverageStatus(String coverageStatus) {
        this.coverageStatus = coverageStatus;
    }

    public String getCoverageStartDt() {
        return coverageStartDt;
    }

    public void setCoverageStartDt(String coverageStartDt) {
        this.coverageStartDt = coverageStartDt;
    }

    public String getCoverageEndDt() {
        return coverageEndDt;
    }

    public void setCoverageEndDt(String coverageEndDt) {
        this.coverageEndDt = coverageEndDt;
    }

    public LocalDateTime getCoverageAddTs() {
        return coverageAddTs;
    }

    public void setCoverageAddTs(LocalDateTime coverageAddTs) {
        this.coverageAddTs = coverageAddTs;
    }

    public LocalDateTime getCoverageUpdateTs() {
        return coverageUpdateTs;
    }

    public void setCoverageUpdateTs(LocalDateTime coverageUpdateTs) {
        this.coverageUpdateTs = coverageUpdateTs;
    }
}