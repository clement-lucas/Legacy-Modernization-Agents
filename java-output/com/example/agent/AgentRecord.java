package com.example.agent;

import jakarta.persistence.*;
import jakarta.validation.constraints.*;
import java.time.LocalDate;

/**
 * Java representation of the COBOL AGENT-RECORD structure.
 * - All COBOL PIC X(n) fields are mapped to Java String fields.
 * - Field lengths are enforced via @Size annotations.
 * - Dates are stored as String for direct mapping, but can be converted to LocalDate if needed.
 * - Entity is compatible with Quarkus and JPA/Hibernate.
 */
@Entity
@Table(name = "agent_record")
public class AgentRecord {

    @Id
    @Column(name = "agent_code", length = 10)
    @Size(max = 10)
    @NotBlank
    private String agentCode;

    @Column(name = "agent_name", length = 45)
    @Size(max = 45)
    @NotBlank
    private String agentName;

    @Column(name = "agent_address_1", length = 50)
    @Size(max = 50)
    private String agentAddress1;

    @Column(name = "agent_address_2", length = 50)
    @Size(max = 50)
    private String agentAddress2;

    @Column(name = "agent_city", length = 20)
    @Size(max = 20)
    private String agentCity;

    @Column(name = "agent_state", length = 2)
    @Size(max = 2)
    private String agentState;

    @Column(name = "agent_zip_cd", length = 10)
    @Size(max = 10)
    private String agentZipCd;

    @Column(name = "agent_dob", length = 10)
    @Size(max = 10)
    private String agentDob; // Consider LocalDate if format is known

    @Column(name = "agent_type", length = 10)
    @Size(max = 10)
    private String agentType;

    @Column(name = "agent_status", length = 1)
    @Size(max = 1)
    private String agentStatus;

    @Column(name = "agent_email", length = 30)
    @Size(max = 30)
    @Email
    private String agentEmail;

    @Column(name = "agent_contact_no", length = 10)
    @Size(max = 10)
    private String agentContactNo;

    @Column(name = "agent_start_date", length = 10)
    @Size(max = 10)
    private String agentStartDate; // Consider LocalDate if format is known

    @Column(name = "agent_end_date", length = 10)
    @Size(max = 10)
    private String agentEndDate; // Consider LocalDate if format is known

    // --- Constructors ---
    public AgentRecord() {}

    // Getters and Setters for all fields
    // (Omitted for brevity, but should be present in production code)
    // Use Lombok @Getter/@Setter if preferred

    // Example getter/setter for agentCode:
    public String getAgentCode() { return agentCode; }
    public void setAgentCode(String agentCode) { this.agentCode = agentCode; }

    // ...repeat for all fields...
}