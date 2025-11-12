// File: src/main/java/com/example/fldriver/NotificationFileDriverService.java

package com.example.fldriver;

import jakarta.enterprise.context.ApplicationScoped;
import jakarta.inject.Inject;
import org.jboss.logging.Logger;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.Objects;

/**
 * Service class that implements the logic of the COBOL FLDRIVR2 program.
 * Handles file operations for agent, customer, and report notification files.
 * 
 * Conversion Notes:
 * - COBOL file operations are mapped to Java NIO file streams.
 * - Error handling uses Quarkus logging and exceptions.
 * - File status codes: "00" = success, "99" = error.
 */
@ApplicationScoped
public class NotificationFileDriverService {

    private static final Logger LOG = Logger.getLogger(NotificationFileDriverService.class);

    // File names as per COBOL SELECT statements
    private static final String AGENT_NOTIFY_FILE = "AGENT-NOTIFY-FILE";
    private static final String CUSTOMER_NOTIFY_FILE = "CUSTOMER-NOTIFY-FILE";
    private static final String NOTIFY_REPORT_FILE = "NOTIFY-REPORT-FILE";

    // Physical file paths (could be injected via Quarkus config)
    private static final String AGENT_FILE_PATH = "AGENTFLE";
    private static final String CUSTOMER_FILE_PATH = "CUSTFLE";
    private static final String REPORT_FILE_PATH = "RPTFLE";

    // File streams (kept open per file type)
    private BufferedWriter agentFileWriter;
    private BufferedWriter customerFileWriter;
    private BufferedWriter reportFileWriter;

    /**
     * Main entry point for file operations.
     * @param request DTO containing operation parameters and record data.
     * @return OperationStatus containing status code and message.
     */
    public OperationStatus processFileOperation(FileOperationRequest request) {
        Objects.requireNonNull(request, "FileOperationRequest must not be null");

        String fileName = request.getFileName();
        String operationType = request.getOperationType();
        String operationStatus = "00"; // Default success

        try {
            switch (operationType.toUpperCase()) {
                case "OPEN":
                    fileOpen(fileName);
                    break;
                case "CLOSE":
                    fileClose(fileName);
                    break;
                case "WRITE":
                    fileWrite(fileName, request);
                    break;
                default:
                    operationStatus = "99";
                    LOG.errorf("Invalid operation type: %s", operationType);
            }
        } catch (FileOperationException e) {
            operationStatus = "99";
            LOG.errorf("ERROR: %s ON FILE %s. FILE STATUS: %s", operationType, fileName, e.getMessage());
        } catch (Exception e) {
            operationStatus = "99";
            LOG.errorf("Unexpected error: %s", e.getMessage());
        }

        return new OperationStatus(operationStatus);
    }

    /**
     * Opens the specified file for output.
     * In Java, opening means creating a BufferedWriter for the file.
     */
    private void fileOpen(String fileName) throws FileOperationException {
        try {
            if (AGENT_NOTIFY_FILE.equals(fileName)) {
                agentFileWriter = Files.newBufferedWriter(Paths.get(AGENT_FILE_PATH), StandardCharsets.UTF_8,
                        StandardOpenOption.CREATE, StandardOpenOption.APPEND);
            } else if (CUSTOMER_NOTIFY_FILE.equals(fileName)) {
                customerFileWriter = Files.newBufferedWriter(Paths.get(CUSTOMER_FILE_PATH), StandardCharsets.UTF_8,
                        StandardOpenOption.CREATE, StandardOpenOption.APPEND);
            } else if (NOTIFY_REPORT_FILE.equals(fileName)) {
                reportFileWriter = Files.newBufferedWriter(Paths.get(REPORT_FILE_PATH), StandardCharsets.UTF_8,
                        StandardOpenOption.CREATE, StandardOpenOption.APPEND);
            } else {
                throw new FileOperationException("Unknown file name: " + fileName);
            }
        } catch (IOException e) {
            throw new FileOperationException("Failed to open file: " + fileName, e);
        }
    }

    /**
     * Closes the specified file.
     * In Java, closing means closing the BufferedWriter.
     */
    private void fileClose(String fileName) throws FileOperationException {
        try {
            if (AGENT_NOTIFY_FILE.equals(fileName) && agentFileWriter != null) {
                agentFileWriter.close();
                agentFileWriter = null;
            } else if (CUSTOMER_NOTIFY_FILE.equals(fileName) && customerFileWriter != null) {
                customerFileWriter.close();
                customerFileWriter = null;
            } else if (NOTIFY_REPORT_FILE.equals(fileName) && reportFileWriter != null) {
                reportFileWriter.close();
                reportFileWriter = null;
            } else {
                throw new FileOperationException("Unknown or unopened file: " + fileName);
            }
        } catch (IOException e) {
            throw new FileOperationException("Failed to close file: " + fileName, e);
        }
    }

    /**
     * Writes a record to the specified file.
     * Maps COBOL MOVE and WRITE to Java serialization and file write.
     */
    private void fileWrite(String fileName, FileOperationRequest request) throws FileOperationException {
        try {
            if (AGENT_NOTIFY_FILE.equals(fileName)) {
                AgentNotifyRecord record = request.getAgentNotifyRecord();
                if (record == null) throw new FileOperationException("AgentNotifyRecord is null");
                if (agentFileWriter == null) throw new FileOperationException("File not open: " + fileName);
                agentFileWriter.write(record.toFixedLengthString());
                agentFileWriter.newLine();
                agentFileWriter.flush();
            } else if (CUSTOMER_NOTIFY_FILE.equals(fileName)) {
                CustomerNotifyRecord record = request.getCustomerNotifyRecord();
                if (record == null) throw new FileOperationException("CustomerNotifyRecord is null");
                if (customerFileWriter == null) throw new FileOperationException("File not open: " + fileName);
                customerFileWriter.write(record.toFixedLengthString());
                customerFileWriter.newLine();
                customerFileWriter.flush();
            } else if (NOTIFY_REPORT_FILE.equals(fileName)) {
                NotifyReportRecord record = request.getNotifyReportRecord();
                if (record == null) throw new FileOperationException("NotifyReportRecord is null");
                if (reportFileWriter == null) throw new FileOperationException("File not open: " + fileName);
                reportFileWriter.write(record.toFixedLengthString());
                reportFileWriter.newLine();
                reportFileWriter.flush();
            } else {
                throw new FileOperationException("Unknown file name: " + fileName);
            }
        } catch (IOException e) {
            throw new FileOperationException("Failed to write to file: " + fileName, e);
        }
    }

    /**
     * Custom exception for file operation errors.
     */
    public static class FileOperationException extends Exception {
        public FileOperationException(String message) { super(message); }
        public FileOperationException(String message, Throwable cause) { super(message, cause); }
    }
}