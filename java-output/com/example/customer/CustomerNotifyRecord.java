package com.example.customer;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Column;
import jakarta.persistence.Table;
import jakarta.validation.constraints.Size;
import jakarta.validation.constraints.NotBlank;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.AllArgsConstructor;

/**
 * Java representation of the COBOL WS-CUSTOMER-NOTIFY-RECORD structure.
 * 
 * Conversion notes:
 * - Each COBOL PIC X(n) field is mapped to a Java String with @Size constraint.
 * - Field names are converted to camelCase for Java conventions.
 * - JPA annotations (@Entity, @Column) are used for Quarkus compatibility.
 * - Lombok is used to reduce boilerplate (getters/setters, constructors).
 * - Comments explain mapping decisions.
 */
@Entity
@Table(name = "customer_notify_record")
@Data
@NoArgsConstructor
@AllArgsConstructor
public class CustomerNotifyRecord {

    /**
     * Customer policy number (COBOL: CN-CUST-POLICY-NUMBER PIC X(10))
     * Used as primary key for demonstration.
     */
    @Id
    @Column(name = "cust_policy_number", length = 10, nullable = false)
    @NotBlank
    @Size(max = 10)
    private String custPolicyNumber;

    /**
     * Customer first name (COBOL: CN-CUST-FIRST-NAME PIC X(35))
     */
    @Column(name = "cust_first_name", length = 35)
    @Size(max = 35)
    private String custFirstName;

    /**
     * Customer middle initial (COBOL: CN-CUST-MIDDLE-NAME PIC X(1))
     */
    @Column(name = "cust_middle_name", length = 1)
    @Size(max = 1)
    private String custMiddleName;

    /**
     * Customer last name (COBOL: CN-CUST-LAST-NAME PIC X(35))
     */
    @Column(name = "cust_last_name", length = 35)
    @Size(max = 35)
    private String custLastName;

    /**
     * Policy start date (COBOL: CN-CUST-START-DATE PIC X(10))
     * Consider using LocalDate if format is always 'yyyy-MM-dd'.
     */
    @Column(name = "cust_start_date", length = 10)
    @Size(max = 10)
    private String custStartDate;

    /**
     * Policy expiry date (COBOL: CN-CUST-EXPIRY-DATE PIC X(10))
     */
    @Column(name = "cust_expiry_date", length = 10)
    @Size(max = 10)
    private String custExpiryDate;

    /**
     * Notification date (COBOL: CN-CUST-NOTIFY-DATE PIC X(10))
     */
    @Column(name = "cust_notify_date", length = 10)
    @Size(max = 10)
    private String custNotifyDate;

    /**
     * Notification messages (COBOL: CN-CUST-NOTIFY-MESSAGES PIC X(100))
     */
    @Column(name = "cust_notify_messages", length = 100)
    @Size(max = 100)
    private String custNotifyMessages;

    /**
     * Agent code (COBOL: CN-CUST-AGENT-CODE PIC X(10))
     */
    @Column(name = "cust_agent_code", length = 10)
    @Size(max = 10)
    private String custAgentCode;

    /**
     * Agent name (COBOL: CN-CUST-AGENT-NAME PIC X(45))
     */
    @Column(name = "cust_agent_name", length = 45)
    @Size(max = 45)
    private String custAgentName;

    /**
     * Statutory message (COBOL: CN-STATUTORY-MESSAGE PIC X(100))
     */
    @Column(name = "statutory_message", length = 100)
    @Size(max = 100)
    private String statutoryMessage;

    // Additional conversion notes:
    // - If dates are always in ISO format, consider using LocalDate and @Convert.
    // - For error handling, see REST resource example below.
}