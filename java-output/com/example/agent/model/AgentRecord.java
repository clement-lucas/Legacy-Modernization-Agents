package com.example.agent.model;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

// The @Entity annotation allows Quarkus to map this to a database table if needed.
// For VSAM replacement, you may use a database or another persistent store.
@Entity
@Table(name = "agent")
public class AgentRecord {

    @Id
    private String agentCode; // AGENT-CODE (PIC X(10))

    private String agentName;         // AGENT-NAME (PIC X(30))
    private String agentAddress1;     // AGENT-ADDRESS-1 (PIC X(50))
    private String agentAddress2;     // AGENT-ADDRESS-2 (PIC X(50))
    private String agentCity;         // AGENT-CITY (PIC X(20))
    private String agentState;        // AGENT-STATE (PIC X(2))
    private String agentZipCode;      // AGENT-ZIP-CD (PIC X(10))
    private String agentStatus;       // AGENT-STATUS (PIC X(1))
    private String agentType;         // AGENT-TYPE (PIC X(10))
    private String agentEmail;        // AGENT-EMAIL (PIC X(30))
    private String agentContactNo;    // AGENT-CONTACT-NO (PIC X(10))
    private String agentStartDate;    // AGENT-START-DATE (PIC X(10))
    private String agentEndDate;      // AGENT-END-DATE (PIC X(10))

    // Getters and setters omitted for brevity, but should be present for all fields.
    // You can use Lombok's @Data for automatic generation if preferred.
    // For clarity, explicit getters/setters are recommended in migration projects.
    // ...
}