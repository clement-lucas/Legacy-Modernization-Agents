package com.example.agentnotify.model;

import io.quarkus.runtime.annotations.RegisterForReflection;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Size;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.AllArgsConstructor;

/**
 * Java representation of the COBOL WS-AGENT-NOTIFY-RECORD structure.
 * 
 * Conversion notes:
 * - All COBOL PIC X(n) fields are mapped to Java String.
 * - Field lengths are preserved via @Size annotations for validation.
 * - Dates are kept as String for direct mapping, but could be converted to LocalDate for better type safety.
 * - No procedural logic or error handling present in the COBOL fragment.
 * - This class is suitable for use as a DTO in Quarkus REST endpoints or as a JPA entity.
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@RegisterForReflection // Ensures compatibility with Quarkus native mode
public class AgentNotifyRecord {

    @NotBlank
    @Size(max = 10)
    private String agentCode; // AN-AGENT-CODE

    @NotBlank
    @Size(max = 45)
    private String agentName; // AN-AGENT-NAME

    @Size(max = 50)
    private String agentAddress1; // AN-AGENT-ADDRESS-1

    @Size(max = 50)
    private String agentAddress2; // AN-AGENT-ADDRESS-2

    @Size(max = 20)
    private String agentCity; // AN-AGENT-CITY

    @Size(max = 2)
    private String agentState; // AN-AGENT-STATE

    @NotBlank
    @Size(max = 10)
    private String policyNumber; // AN-POLICY-NUMBER

    @Size(max = 35)
    private String policyHolderFName; // AN-POLICY-HOLDER-FNAME

    @Size(max = 1)
    private String policyHolderMName; // AN-POLICY-HOLDER-MNAME

    @Size(max = 35)
    private String policyHolderLName; // AN-POLICY-HOLDER-LNAME

    @Size(max = 10)
    private String policyStartDate; // AN-POLICY-START-DATE (Consider LocalDate)

    @Size(max = 10)
    private String policyExpiryDate; // AN-POLICY-EXPIRY-DATE (Consider LocalDate)

    @Size(max = 10)
    private String notifyDate; // AN-NOTIFY-DATE (Consider LocalDate)

    @Size(max = 100)
    private String notifyMessages; // AN-NOTIFY-MESSAGES

    // If you want to use LocalDate for date fields, add conversion logic in service layer.
}