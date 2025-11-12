package com.example.insurnce.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;

@Entity
@Table(name = "TTRAKING", schema = "INSURNCE")
public class TrackingRecord {

    @Id
    @Column(name = "TR_POLICY_NUMBER", length = 10)
    private String policyNumber;

    @Column(name = "TR_NOTIFY_DATE", length = 10)
    private String notifyDate;

    @Column(name = "TR_STATUS", length = 1)
    private String status;

    @Column(name = "TR_ADD_TIMESTAMP")
    private LocalDateTime addTimestamp;

    @Column(name = "TR_UPDATE_TIMESTAMP")
    private LocalDateTime updateTimestamp;

    // Getters and setters omitted for brevity
    // ... (generate with your IDE or Lombok if preferred)

    // Constructors
    public TrackingRecord() {}

    public TrackingRecord(String policyNumber, String notifyDate, String status) {
        this.policyNumber = policyNumber;
        this.notifyDate = notifyDate;
        this.status = status;
        this.addTimestamp = LocalDateTime.now();
        this.updateTimestamp = LocalDateTime.now();
    }

    // Getters and setters...
    public String getPolicyNumber() { return policyNumber; }
    public void setPolicyNumber(String policyNumber) { this.policyNumber = policyNumber; }

    public String getNotifyDate() { return notifyDate; }
    public void setNotifyDate(String notifyDate) { this.notifyDate = notifyDate; }

    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }

    public LocalDateTime getAddTimestamp() { return addTimestamp; }
    public void setAddTimestamp(LocalDateTime addTimestamp) { this.addTimestamp = addTimestamp; }

    public LocalDateTime getUpdateTimestamp() { return updateTimestamp; }
    public void setUpdateTimestamp(LocalDateTime updateTimestamp) { this.updateTimestamp = updateTimestamp; }
}