package com.example.insurance.model;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

import jakarta.validation.constraints.*;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.AllArgsConstructor;

/**
 * PolicyRecord represents the COBOL POLICY-RECORD structure.
 * 
 * Conversion notes:
 * - COBOL PIC X(n) fields are mapped to Java String.
 * - COBOL packed decimal (COMP-3) is mapped to BigDecimal.
 * - Date fields (PIC X(10)) are kept as String for flexibility, but can be parsed to LocalDate.
 * - Timestamp fields (PIC X(26)) are kept as String, but can be parsed to LocalDateTime.
 * - Field lengths are documented for validation and migration purposes.
 * - No error handling is present, as the COBOL code is a pure data definition.
 * - Lombok is used for boilerplate reduction.
 * - Validation annotations are provided for future REST API use.
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class PolicyRecord {

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyNumber;

    // COBOL: PIC X(35)
    @Size(max = 35)
    private String policyHolderFname;

    // COBOL: PIC X(01)
    @Size(max = 1)
    private String policyHolderMname;

    // COBOL: PIC X(35)
    @Size(max = 35)
    private String policyHolderLname;

    // COBOL: PIC X(60)
    @Size(max = 60)
    private String policyBenefName;

    // COBOL: PIC X(15)
    @Size(max = 15)
    private String policyBenefRelation;

    // COBOL: PIC X(100)
    @Size(max = 100)
    private String policyHolderAddr1;

    // COBOL: PIC X(100)
    @Size(max = 100)
    private String policyHolderAddr2;

    // COBOL: PIC X(30)
    @Size(max = 30)
    private String policyHolderCity;

    // COBOL: PIC X(2)
    @Size(max = 2)
    private String policyHolderState;

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyHolderZipCd;

    // COBOL: PIC X(10) (Date, format: yyyy-MM-dd or similar)
    @Size(max = 10)
    private String policyHolderDob;

    // COBOL: PIC X(8)
    @Size(max = 8)
    private String policyHolderGender;

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyHolderPhone;

    // COBOL: PIC X(30)
    @Size(max = 30)
    private String policyHolderEmail;

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyPaymentFreq;

    // COBOL: PIC X(8)
    @Size(max = 8)
    private String policyPaymentMethod;

    // COBOL: PIC X(50)
    @Size(max = 50)
    private String policyUnderwriter;

    // COBOL: PIC X(200)
    @Size(max = 200)
    private String policyTermsCond;

    // COBOL: PIC X(1) (Claimed flag, e.g. 'Y'/'N')
    @Size(max = 1)
    private String policyClaimed;

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyDiscountCode;

    // COBOL: PIC S9(5)V9(2) COMP-3 (Packed decimal: signed, 5 digits before decimal, 2 after)
    // Java BigDecimal is used for precision and scale.
    @Digits(integer = 7, fraction = 2)
    private BigDecimal policyPremiumAmount;

    // COBOL: PIC X(50)
    @Size(max = 50)
    private String policyType;

    // COBOL: PIC X(10) (Date, format: yyyy-MM-dd or similar)
    @Size(max = 10)
    private String policyStartDate;

    // COBOL: PIC X(10) (Date, format: yyyy-MM-dd or similar)
    @Size(max = 10)
    private String policyExpiryDate;

    // COBOL: PIC X(1) (Status flag, e.g. 'A'/'I')
    @Size(max = 1)
    private String policyStatus;

    // COBOL: PIC X(10)
    @Size(max = 10)
    private String policyAgentCode;

    // COBOL: PIC X(1) (Notify flag, e.g. 'Y'/'N')
    @Size(max = 1)
    private String policyNotifyFlag;

    // COBOL: PIC X(26) (Timestamp, e.g. 'yyyy-MM-dd HH:mm:ss.SSSSSS')
    @Size(max = 26)
    private String policyAddTimestamp;

    // COBOL: PIC X(26) (Timestamp, e.g. 'yyyy-MM-dd HH:mm:ss.SSSSSS')
    @Size(max = 26)
    private String policyUpdateTimestamp;

    // --- Conversion Decisions ---
    // - All fields are Strings except for policyPremiumAmount (BigDecimal).
    // - Date and timestamp fields are kept as String for flexibility, but can be parsed to LocalDate/LocalDateTime.
    // - Validation annotations are provided for REST API input validation.
    // - Lombok generates getters/setters, constructors, equals/hashCode, toString.
    // - For persistence, annotate with @Entity and add @Id if needed.
    // - For error handling, add validation logic in service/controller layers.
}