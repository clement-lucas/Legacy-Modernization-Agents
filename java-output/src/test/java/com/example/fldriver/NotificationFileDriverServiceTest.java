package com.example.fldriver;

import io.quarkus.test.junit.QuarkusTest;
import org.junit.jupiter.api.*;
import org.mockito.*;
import org.mockito.junit.jupiter.MockitoExtension;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

@QuarkusTest
@ExtendWith(MockitoExtension.class)
class NotificationFileDriverServiceTest {

    @InjectMocks
    NotificationFileDriverService service;

    // Mocks for static Files.newBufferedWriter
    @Mock
    BufferedWriter agentWriterMock;
    @Mock
    BufferedWriter customerWriterMock;
    @Mock
    BufferedWriter reportWriterMock;

    // Test file paths
    private static final String AGENT_FILE_PATH = "AGENTFLE";
    private static final String CUSTOMER_FILE_PATH = "CUSTFLE";
    private static final String REPORT_FILE_PATH = "RPTFLE";

    // Test file names
    private static final String AGENT_NOTIFY_FILE = "AGENT-NOTIFY-FILE";
    private static final String CUSTOMER_NOTIFY_FILE = "CUSTOMER-NOTIFY-FILE";
    private static final String NOTIFY_REPORT_FILE = "NOTIFY-REPORT-FILE";

    // Temporary files for integration tests
    private Path agentTempFile;
    private Path customerTempFile;
    private Path reportTempFile;

    @BeforeEach
    void setUp() throws Exception {
        // Create temp files for integration tests
        agentTempFile = Files.createTempFile("agent", ".txt");
        customerTempFile = Files.createTempFile("customer", ".txt");
        reportTempFile = Files.createTempFile("report", ".txt");

        // Override static file paths using reflection for integration tests
        setPrivateStaticField(NotificationFileDriverService.class, "AGENT_FILE_PATH", agentTempFile.toString());
        setPrivateStaticField(NotificationFileDriverService.class, "CUSTOMER_FILE_PATH", customerTempFile.toString());
        setPrivateStaticField(NotificationFileDriverService.class, "REPORT_FILE_PATH", reportTempFile.toString());
    }

    @AfterEach
    void tearDown() throws Exception {
        // Delete temp files
        Files.deleteIfExists(agentTempFile);
        Files.deleteIfExists(customerTempFile);
        Files.deleteIfExists(reportTempFile);
    }

    // Utility to set private static fields via reflection
    private static void setPrivateStaticField(Class<?> clazz, String fieldName, Object value) throws Exception {
        var field = clazz.getDeclaredField(fieldName);
        field.setAccessible(true);
        field.set(null, value);
    }

    // Utility DTOs and records for testing
    private FileOperationRequest agentOpenRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(AGENT_NOTIFY_FILE);
        req.setOperationType("OPEN");
        return req;
    }

    private FileOperationRequest agentWriteRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(AGENT_NOTIFY_FILE);
        req.setOperationType("WRITE");
        req.setAgentNotifyRecord(new AgentNotifyRecord("A123", "John Doe", "20240601"));
        return req;
    }

    private FileOperationRequest agentCloseRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(AGENT_NOTIFY_FILE);
        req.setOperationType("CLOSE");
        return req;
    }

    private FileOperationRequest customerOpenRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(CUSTOMER_NOTIFY_FILE);
        req.setOperationType("OPEN");
        return req;
    }

    private FileOperationRequest customerWriteRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(CUSTOMER_NOTIFY_FILE);
        req.setOperationType("WRITE");
        req.setCustomerNotifyRecord(new CustomerNotifyRecord("C456", "Jane Smith", "20240602"));
        return req;
    }

    private FileOperationRequest customerCloseRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(CUSTOMER_NOTIFY_FILE);
        req.setOperationType("CLOSE");
        return req;
    }

    private FileOperationRequest reportOpenRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(NOTIFY_REPORT_FILE);
        req.setOperationType("OPEN");
        return req;
    }

    private FileOperationRequest reportWriteRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(NOTIFY_REPORT_FILE);
        req.setOperationType("WRITE");
        req.setNotifyReportRecord(new NotifyReportRecord("R789", "ReportLine", "20240603"));
        return req;
    }

    private FileOperationRequest reportCloseRequest() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(NOTIFY_REPORT_FILE);
        req.setOperationType("CLOSE");
        return req;
    }

    // ----------- TESTS ------------

    @Test
    void testProcessFileOperation_OpenAgentFile_Success() {
        // Arrange
        FileOperationRequest req = agentOpenRequest();

        // Act
        OperationStatus status = service.processFileOperation(req);

        // Assert
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteAgentFile_Success() throws Exception {
        // Arrange
        service.processFileOperation(agentOpenRequest());
        FileOperationRequest req = agentWriteRequest();

        // Act
        OperationStatus status = service.processFileOperation(req);

        // Assert
        assertEquals("00", status.getStatusCode());

        // Verify file contents
        List<String> lines = Files.readAllLines(agentTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).startsWith("A123"));
    }

    @Test
    void testProcessFileOperation_CloseAgentFile_Success() {
        // Arrange
        service.processFileOperation(agentOpenRequest());

        // Act
        OperationStatus status = service.processFileOperation(agentCloseRequest());

        // Assert
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_OpenCustomerFile_Success() {
        FileOperationRequest req = customerOpenRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteCustomerFile_Success() throws Exception {
        service.processFileOperation(customerOpenRequest());
        FileOperationRequest req = customerWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
        List<String> lines = Files.readAllLines(customerTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).startsWith("C456"));
    }

    @Test
    void testProcessFileOperation_CloseCustomerFile_Success() {
        service.processFileOperation(customerOpenRequest());
        OperationStatus status = service.processFileOperation(customerCloseRequest());
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_OpenReportFile_Success() {
        FileOperationRequest req = reportOpenRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteReportFile_Success() throws Exception {
        service.processFileOperation(reportOpenRequest());
        FileOperationRequest req = reportWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
        List<String> lines = Files.readAllLines(reportTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).startsWith("R789"));
    }

    @Test
    void testProcessFileOperation_CloseReportFile_Success() {
        service.processFileOperation(reportOpenRequest());
        OperationStatus status = service.processFileOperation(reportCloseRequest());
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_InvalidOperationType_Returns99() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(AGENT_NOTIFY_FILE);
        req.setOperationType("INVALID");
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_NullRequest_ThrowsException() {
        assertThrows(NullPointerException.class, () -> service.processFileOperation(null));
    }

    @Test
    void testProcessFileOperation_WriteAgentFileWithoutOpen_Returns99() {
        FileOperationRequest req = agentWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteAgentFileWithNullRecord_Returns99() {
        service.processFileOperation(agentOpenRequest());
        FileOperationRequest req = agentWriteRequest();
        req.setAgentNotifyRecord(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteCustomerFileWithNullRecord_Returns99() {
        service.processFileOperation(customerOpenRequest());
        FileOperationRequest req = customerWriteRequest();
        req.setCustomerNotifyRecord(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteReportFileWithNullRecord_Returns99() {
        service.processFileOperation(reportOpenRequest());
        FileOperationRequest req = reportWriteRequest();
        req.setNotifyReportRecord(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_OpenUnknownFile_Returns99() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName("UNKNOWN-FILE");
        req.setOperationType("OPEN");
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_CloseUnknownFile_Returns99() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName("UNKNOWN-FILE");
        req.setOperationType("CLOSE");
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteUnknownFile_Returns99() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName("UNKNOWN-FILE");
        req.setOperationType("WRITE");
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_CloseAgentFileTwice_Returns99SecondTime() {
        service.processFileOperation(agentOpenRequest());
        OperationStatus status1 = service.processFileOperation(agentCloseRequest());
        assertEquals("00", status1.getStatusCode());
        OperationStatus status2 = service.processFileOperation(agentCloseRequest());
        assertEquals("99", status2.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteCustomerFileWithoutOpen_Returns99() {
        FileOperationRequest req = customerWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteReportFileWithoutOpen_Returns99() {
        FileOperationRequest req = reportWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_OpenAgentFileTwice_AppendsData() throws Exception {
        // Open, write, close, open again, write again
        service.processFileOperation(agentOpenRequest());
        service.processFileOperation(agentWriteRequest());
        service.processFileOperation(agentCloseRequest());

        service.processFileOperation(agentOpenRequest());
        FileOperationRequest req2 = agentWriteRequest();
        req2.setAgentNotifyRecord(new AgentNotifyRecord("A999", "Second", "20240610"));
        service.processFileOperation(req2);
        service.processFileOperation(agentCloseRequest());

        List<String> lines = Files.readAllLines(agentTempFile, StandardCharsets.UTF_8);
        assertEquals(2, lines.size());
        assertTrue(lines.get(0).startsWith("A123"));
        assertTrue(lines.get(1).startsWith("A999"));
    }

    @Test
    void testProcessFileOperation_WriteAgentFile_BoundaryRecordLength() throws Exception {
        service.processFileOperation(agentOpenRequest());
        // Record with max length fields
        AgentNotifyRecord rec = new AgentNotifyRecord("A12345678901234567890", "John Doe Doe Doe Doe Doe Doe Doe", "99999999");
        FileOperationRequest req = agentWriteRequest();
        req.setAgentNotifyRecord(rec);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
        List<String> lines = Files.readAllLines(agentTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).contains("A12345678901234567890"));
    }

    @Test
    void testProcessFileOperation_WriteCustomerFile_BoundaryRecordLength() throws Exception {
        service.processFileOperation(customerOpenRequest());
        CustomerNotifyRecord rec = new CustomerNotifyRecord("C456789012345678901234", "Jane Smith Smith Smith Smith", "88888888");
        FileOperationRequest req = customerWriteRequest();
        req.setCustomerNotifyRecord(rec);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
        List<String> lines = Files.readAllLines(customerTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).contains("C456789012345678901234"));
    }

    @Test
    void testProcessFileOperation_WriteReportFile_BoundaryRecordLength() throws Exception {
        service.processFileOperation(reportOpenRequest());
        NotifyReportRecord rec = new NotifyReportRecord("R789012345678901234567", "ReportLineLongLongLongLong", "77777777");
        FileOperationRequest req = reportWriteRequest();
        req.setNotifyReportRecord(rec);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("00", status.getStatusCode());
        List<String> lines = Files.readAllLines(reportTempFile, StandardCharsets.UTF_8);
        assertTrue(lines.get(0).contains("R789012345678901234567"));
    }

    @Test
    void testProcessFileOperation_WriteAgentFile_NullFileName_Returns99() {
        FileOperationRequest req = agentWriteRequest();
        req.setFileName(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteAgentFile_NullOperationType_Returns99() {
        FileOperationRequest req = agentWriteRequest();
        req.setOperationType(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    // ----------- MOCKED IO ERROR TESTS ------------

    @Test
    void testProcessFileOperation_OpenAgentFile_IOException_Returns99() throws Exception {
        // Simulate IOException on open
        setPrivateStaticField(NotificationFileDriverService.class, "AGENT_FILE_PATH", "/invalid/path/agent.txt");
        FileOperationRequest req = agentOpenRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_WriteAgentFile_IOException_Returns99() throws Exception {
        // Open file normally
        service.processFileOperation(agentOpenRequest());
        // Use reflection to inject a mock that throws IOException
        var field = NotificationFileDriverService.class.getDeclaredField("agentFileWriter");
        field.setAccessible(true);
        BufferedWriter mockWriter = mock(BufferedWriter.class);
        doThrow(new IOException("Disk error")).when(mockWriter).write(anyString());
        field.set(service, mockWriter);

        FileOperationRequest req = agentWriteRequest();
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    @Test
    void testProcessFileOperation_CloseAgentFile_IOException_Returns99() throws Exception {
        service.processFileOperation(agentOpenRequest());
        var field = NotificationFileDriverService.class.getDeclaredField("agentFileWriter");
        field.setAccessible(true);
        BufferedWriter mockWriter = mock(BufferedWriter.class);
        doThrow(new IOException("Close error")).when(mockWriter).close();
        field.set(service, mockWriter);

        OperationStatus status = service.processFileOperation(agentCloseRequest());
        assertEquals("99", status.getStatusCode());
    }

    // ----------- COBOL LOGIC PRESERVATION TESTS ------------

    @Test
    void testCobolLogic_FileStatus00OnSuccess() {
        service.processFileOperation(agentOpenRequest());
        OperationStatus status = service.processFileOperation(agentWriteRequest());
        assertEquals("00", status.getStatusCode());
    }

    @Test
    void testCobolLogic_FileStatus99OnError() {
        FileOperationRequest req = agentWriteRequest();
        OperationStatus status = service.processFileOperation(req); // Not opened
        assertEquals("99", status.getStatusCode());
    }

    // ----------- NULL CHECKS AND EDGE CASES ------------

    @Test
    void testProcessFileOperation_WriteAgentFile_NullRequestFields_Returns99() {
        FileOperationRequest req = new FileOperationRequest();
        req.setFileName(null);
        req.setOperationType(null);
        OperationStatus status = service.processFileOperation(req);
        assertEquals("99", status.getStatusCode());
    }

    // ----------- INTEGRATION TEST: MULTIPLE FILES ------------

    @Test
    void testIntegration_MultipleFiles_SequentialOperations() throws Exception {
        // Open all files
        service.processFileOperation(agentOpenRequest());
        service.processFileOperation(customerOpenRequest());
        service.processFileOperation(reportOpenRequest());

        // Write to all files
        service.processFileOperation(agentWriteRequest());
        service.processFileOperation(customerWriteRequest());
        service.processFileOperation(reportWriteRequest());

        // Close all files
        service.processFileOperation(agentCloseRequest());
        service.processFileOperation(customerCloseRequest());
        service.processFileOperation(reportCloseRequest());

        // Verify contents
        List<String> agentLines = Files.readAllLines(agentTempFile, StandardCharsets.UTF_8);
        List<String> customerLines = Files.readAllLines(customerTempFile, StandardCharsets.UTF_8);
        List<String> reportLines = Files.readAllLines(reportTempFile, StandardCharsets.UTF_8);

        assertEquals(1, agentLines.size());
        assertEquals(1, customerLines.size());
        assertEquals(1, reportLines.size());
    }

    // ----------- COMMENTED COMPLEX SCENARIO ------------

    @Test
    void testComplexScenario_WriteMultipleRecordsAndClose_ReopenAndAppend() throws Exception {
        // Open agent file, write two records, close
        service.processFileOperation(agentOpenRequest());
        service.processFileOperation(agentWriteRequest());
        FileOperationRequest req2 = agentWriteRequest();
        req2.setAgentNotifyRecord(new AgentNotifyRecord("A555", "Multi", "20240605"));
        service.processFileOperation(req2);
        service.processFileOperation(agentCloseRequest());

        // Reopen and append another record
        service.processFileOperation(agentOpenRequest());
        FileOperationRequest req3 = agentWriteRequest();
        req3.setAgentNotifyRecord(new AgentNotifyRecord("A666", "Append", "20240606"));
        service.processFileOperation(req3);
        service.processFileOperation(agentCloseRequest());

        // Verify all records are present in order
        List<String> lines = Files.readAllLines(agentTempFile, StandardCharsets.UTF_8);
        assertEquals(3, lines.size());
        assertTrue(lines.get(0).startsWith("A123"));
        assertTrue(lines.get(1).startsWith("A555"));
        assertTrue(lines.get(2).startsWith("A666"));
    }

    // ----------- UTILITY CLASSES FOR TESTS ------------

    // Minimal DTOs for test compilation
    static class FileOperationRequest {
        private String fileName;
        private String operationType;
        private AgentNotifyRecord agentNotifyRecord;
        private CustomerNotifyRecord customerNotifyRecord;
        private NotifyReportRecord notifyReportRecord;
        public String getFileName() { return fileName; }
        public void setFileName(String fileName) { this.fileName = fileName; }
        public String getOperationType() { return operationType; }
        public void setOperationType(String operationType) { this.operationType = operationType; }
        public AgentNotifyRecord getAgentNotifyRecord() { return agentNotifyRecord; }
        public void setAgentNotifyRecord(AgentNotifyRecord agentNotifyRecord) { this.agentNotifyRecord = agentNotifyRecord; }
        public CustomerNotifyRecord getCustomerNotifyRecord() { return customerNotifyRecord; }
        public void setCustomerNotifyRecord(CustomerNotifyRecord customerNotifyRecord) { this.customerNotifyRecord = customerNotifyRecord; }
        public NotifyReportRecord getNotifyReportRecord() { return notifyReportRecord; }
        public void setNotifyReportRecord(NotifyReportRecord notifyReportRecord) { this.notifyReportRecord = notifyReportRecord; }
    }

    static class OperationStatus {
        private final String statusCode;
        public OperationStatus(String statusCode) { this.statusCode = statusCode; }
        public String getStatusCode() { return statusCode; }
    }

    static class AgentNotifyRecord {
        private final String agentId;
        private final String agentName;
        private final String notifyDate;
        public AgentNotifyRecord(String agentId, String agentName, String notifyDate) {
            this.agentId = agentId;
            this.agentName = agentName;
            this.notifyDate = notifyDate;
        }
        public String toFixedLengthString() {
            return String.format("%-20s%-30s%-8s", agentId, agentName, notifyDate);
        }
    }

    static class CustomerNotifyRecord {
        private final String customerId;
        private final String customerName;
        private final String notifyDate;
        public CustomerNotifyRecord(String customerId, String customerName, String notifyDate) {
            this.customerId = customerId;
            this.customerName = customerName;
            this.notifyDate = notifyDate;
        }
        public String toFixedLengthString() {
            return String.format("%-24s%-30s%-8s", customerId, customerName, notifyDate);
        }
    }

    static class NotifyReportRecord {
        private final String reportId;
        private final String reportLine;
        private final String reportDate;
        public NotifyReportRecord(String reportId, String reportLine, String reportDate) {
            this.reportId = reportId;
            this.reportLine = reportLine;
            this.reportDate = reportDate;
        }
        public String toFixedLengthString() {
            return String.format("%-22s%-40s%-8s", reportId, reportLine, reportDate);
        }
    }
}