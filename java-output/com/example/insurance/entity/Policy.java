package com.example.insurance.entity;

import io.quarkus.hibernate.orm.panache.PanacheEntityBase;
import jakarta.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

/**
 * Entity class representing the INSURNCE.TPOLICY table.
 * Converted from COBOL DCLPOLICY structure.
 * 
 * - COBOL CHAR(X) → Java String
 * - COBOL COMP-3/DECIMAL → Java BigDecimal
 * - COBOL DATE → Java LocalDate
 * - COBOL TIMESTAMP → Java LocalDateTime
 * 
 * Error handling in COBOL (ERROR_CODE, ERROR_MSG, etc.) should be mapped to Java exceptions and logging in service/repository layers.
 */
@Entity
@Table(name = "TPOLICY", schema = "INSURNCE")
public class Policy extends PanacheEntityBase {

    @Id
    @Column(name = "POLICY_NUMBER", length = 10, nullable = false)
    private String policyNumber;

    @Column(name = "POLICY_HOLDER_FNAME", length = 35, nullable = false)
    private String policyHolderFname;

    @Column(name = "POLICY_HOLDER_MNAME", length = 1, nullable = false)
    private String policyHolderMname;

    @Column(name = "POLICY_HOLDER_LNAME", length = 35, nullable = false)
    private String policyHolderLname;

    @Column(name = "POLICY_BENEF_NAME", length = 60, nullable = false)
    private String policyBenefName;

    @Column(name = "POLICY_BENEF_RELATION", length = 15, nullable = false)
    private String policyBenefRelation;

    @Column(name = "POLICY_HOLDER_ADDR_1", length = 100, nullable = false)
    private String policyHolderAddr1;

    @Column(name = "POLICY_HOLDER_ADDR_2", length = 100, nullable = false)
    private String policyHolderAddr2;

    @Column(name = "POLICY_HOLDER_CITY", length = 30, nullable = false)
    private String policyHolderCity;

    @Column(name = "POLICY_HOLDER_STATE", length = 2, nullable = false)
    private String policyHolderState;

    @Column(name = "POLICY_HOLDER_ZIP_CD", length = 10, nullable = false)
    private String policyHolderZipCd;

    @Column(name = "POLICY_HOLDER_DOB", length = 10, nullable = false)
    private String policyHolderDob; // Consider converting to LocalDate if format is consistent

    @Column(name = "POLICY_HOLDER_GENDER", length = 8, nullable = false)
    private String policyHolderGender;

    @Column(name = "POLICY_HOLDER_PHONE", length = 10, nullable = false)
    private String policyHolderPhone;

    @Column(name = "POLICY_HOLDER_EMAIL", length = 30, nullable = false)
    private String policyHolderEmail;

    @Column(name = "POLICY_PAYMENT_FREQ", length = 10, nullable = false)
    private String policyPaymentFreq;

    @Column(name = "POLICY_PAYMENT_METHOD", length = 8, nullable = false)
    private String policyPaymentMethod;

    @Column(name = "POLICY_UNDERWRITER", length = 50, nullable = false)
    private String policyUnderwriter;

    @Column(name = "POLICY_TERMS_COND", length = 200, nullable = false)
    private String policyTermsCond;

    @Column(name = "POLICY_CLAIMED", length = 1, nullable = false)
    private String policyClaimed;

    @Column(name = "POLICY_DISCOUNT_CODE", length = 10, nullable = false)
    private String policyDiscountCode;

    @Column(name = "POLICY_PREMIUM_AMOUNT", precision = 7, scale = 2, nullable = false)
    private BigDecimal policyPremiumAmount;

    @Column(name = "POLICY_COVERAGE_AMOUNT", precision = 10, scale = 2, nullable = false)
    private BigDecimal policyCoverageAmount;

    @Column(name = "POLICY_TYPE", length = 50, nullable = false)
    private String policyType;

    @Column(name = "POLICY_START_DATE", nullable = false)
    private LocalDate policyStartDate;

    @Column(name = "POLICY_EXPIRY_DATE", nullable = false)
    private LocalDate policyExpiryDate;

    @Column(name = "POLICY_STATUS", length = 1, nullable = false)
    private String policyStatus;

    @Column(name = "POLICY_AGENT_CODE", length = 10, nullable = false)
    private String policyAgentCode;

    @Column(name = "POLICY_NOTIFY_FLAG", length = 1, nullable = false)
    private String policyNotifyFlag;

    @Column(name = "POLICY_ADD_TIMESTAMP", nullable = false)
    private LocalDateTime policyAddTimestamp;

    @Column(name = "POLICY_UPDATE_TIMESTAMP", nullable = false)
    private LocalDateTime policyUpdateTimestamp;

    // --- Getters and Setters ---
    // (Omitted for brevity, use Lombok @Getter/@Setter or generate via IDE)
    // For Quarkus, you may use public fields or standard getter/setter methods.

    // --- Constructors ---
    public Policy() {}

    // Optionally, add constructors for DTO mapping, etc.

    // --- Utility Methods ---
    // Add any business logic or helper methods as needed.
}