// PolicyRecord.java
public class PolicyRecord {
    public String policyNumber;
    public String policyHolderFName;
    public String policyHolderMName;
    public String policyHolderLName;
    public String policyHolderAddr1;
    public String policyHolderAddr2;
    public String policyHolderCity;
    public String policyHolderState;
    public String policyHolderZipCd;
    public String policyStartDate;
    public String policyExpiryDate;
    public BigDecimal policyPremiumAmount;
    public String policyAgentCode;
    public String policyBenefName;
    public String agentType; // e.g., "CORPORATE"
    // Add other fields as per CPOLICY copybook
}

// AgentRecord.java
public class AgentRecord {
    public String agentCode;
    public String agentName;
    public String agentAddress1;
    public String agentAddress2;
    public String agentCity;
    public String agentState;
    public String agentZipCd;
    public String agentEmail;
    public String agentContactNo;
    public String agentType; // e.g., "CORPORATE"
    // Add other fields as per CAGENT copybook
}

// CustomerNotifyRecord.java
public class CustomerNotifyRecord {
    public String policyNumber;
    public String firstName;
    public String middleName;
    public String lastName;
    public String addr1;
    public String addr2;
    public String city;
    public String state;
    public String zipCd;
    public String startDate;
    public String expiryDate;
    public String notifyDate;
    public String notifyMsg;
    public String agentCode;
    public String agentName;
    public String email;
    public String benefName;
    public String statutoryMsg;
    // Add other fields as per CUSTNTFY copybook
}

// AgentNotifyRecord.java
public class AgentNotifyRecord {
    public String agentCode;
    public String agentName;
    public String address1;
    public String address2;
    public String city;
    public String state;
    public String zipCd;
    public String email;
    public String policyNumber;
    public String policyHolderFName;
    public String policyHolderMName;
    public String policyHolderLName;
    public String policyStartDate;
    public String policyExpiryDate;
    public String notifyDate;
    public String notifyMsg;
    // Add other fields as per AGNTNTFY copybook
}