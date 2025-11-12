package com.example.insurnce;

import io.quarkus.hibernate.orm.panache.PanacheEntityBase;
import jakarta.persistence.*;
import java.time.LocalDate;
import java.time.LocalDateTime;

/**
 * Java entity mapping for COBOL DB2 table INSURNCE.TTRAKING.
 * 
 * Conversion notes:
 * - COBOL PIC X(10) for policy number: mapped to String (max 10 chars).
 * - COBOL PIC X(10) for notify date: mapped to LocalDate (DB2 DATE).
 * - COBOL PIC X(1) for status: mapped to String (single char).
 * - COBOL PIC X(26) for timestamps: mapped to LocalDateTime (DB2 TIMESTAMP).
 * - All fields are non-null as per NOT NULL in DB2.
 * - Default values for timestamps handled by DB (optional: can be set in Java).
 * - Table and column names follow DB2 naming; can be customized via @Table/@Column.
 */
@Entity
@Table(name = "TTRAKING", schema = "INSURNCE")
public class TTracking extends PanacheEntityBase {

    // Primary key: assuming policy number is unique (if not, add @Id to another field or composite key)
    @Id
    @Column(name = "TR_POLICY_NUMBER", length = 10, nullable = false)
    public String policyNumber;

    @Column(name = "TR_NOTIFY_DATE", nullable = false)
    public LocalDate notifyDate;

    @Column(name = "TR_STATUS", length = 1, nullable = false)
    public String status;

    @Column(name = "TR_ADD_TIMESTAMP", nullable = false)
    public LocalDateTime addTimestamp;

    @Column(name = "TR_UPDATE_TIMESTAMP", nullable = false)
    public LocalDateTime updateTimestamp;

    // Constructors
    public TTracking() {}

    public TTracking(String policyNumber, LocalDate notifyDate, String status,
                     LocalDateTime addTimestamp, LocalDateTime updateTimestamp) {
        this.policyNumber = policyNumber;
        this.notifyDate = notifyDate;
        this.status = status;
        this.addTimestamp = addTimestamp;
        this.updateTimestamp = updateTimestamp;
    }

    // Optionally, add getters/setters if you prefer (Panache allows public fields)
}