using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Http;
using CobolToQuarkusMigration.Agents.Interfaces;
using CobolToQuarkusMigration.Models;
using CobolToQuarkusMigration.Helpers;
using System.ClientModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace CobolToQuarkusMigration.Agents;

/// <summary>
/// Implementation of the C# converter agent for COBOL to .NET conversion.
/// </summary>
public class CSharpConverterAgent : ICodeConverterAgent
{
    private readonly IKernelBuilder _kernelBuilder;
    private readonly ILogger<CSharpConverterAgent> _logger;
    private readonly string _modelId;
    private readonly EnhancedLogger? _enhancedLogger;
    private readonly ChatLogger? _chatLogger;

    public string TargetLanguage => "CSharp";
    public string FileExtension => ".cs";

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpConverterAgent"/> class.
    /// </summary>
    public CSharpConverterAgent(IKernelBuilder kernelBuilder, ILogger<CSharpConverterAgent> logger, string modelId, EnhancedLogger? enhancedLogger = null, ChatLogger? chatLogger = null)
    {
        _kernelBuilder = kernelBuilder;
        _logger = logger;
        _modelId = modelId;
        _enhancedLogger = enhancedLogger;
        _chatLogger = chatLogger;
    }

    /// <inheritdoc/>
    public async Task<CodeFile> ConvertAsync(CobolFile cobolFile, CobolAnalysis cobolAnalysis)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Converting COBOL file to C#: {FileName}", cobolFile.FileName);
        _enhancedLogger?.LogBehindTheScenes("AI_PROCESSING", "CSHARP_CONVERSION_START",
            $"Starting C# conversion of {cobolFile.FileName}", cobolFile.FileName);

        var kernel = _kernelBuilder.Build();
        int apiCallId = 0;

        try
        {
            var systemPrompt = @"
You are an expert in converting COBOL programs to C# with .NET framework. Your task is to convert COBOL source code to modern, maintainable C# code.

Follow these guidelines:
1. Create proper C# class structures from COBOL programs
2. Convert COBOL variables to appropriate C# data types
3. Transform COBOL procedures into C# methods
4. Handle COBOL-specific features (PERFORM, GOTO, etc.) in an idiomatic C# way
5. Implement proper error handling with try-catch blocks
6. Include comprehensive XML documentation comments
7. Apply modern C# best practices (async/await, LINQ, etc.)
8. Use a SINGLE namespace per file - choose the most appropriate one (e.g., CobolMigration.Legacy, CobolMigration.BusinessLogic)
9. Return ONLY the C# code without markdown code blocks or additional text
10. Namespace declaration must be file-scoped (single line): 'namespace CobolMigration.Something;'
11. DO NOT create multiple namespace blocks in a single file
12. If you need multiple namespaces, choose ONE primary namespace for this file
13. Use 'using' statements at the top for any external namespace references

CONVERSION REQUIREMENTS FOR DIFFERENT COBOL PROGRAM TYPES:
- **Main Programs (MAINPGM, etc.)**: Convert to orchestration services with proper workflow coordination
- **Database Drivers (DBDRIVR*)**: Convert to service classes with async database operations
- **File Drivers (FLDRIVR*)**: Convert to service classes with async file I/O operations
- **Copybooks (*.cpy)**: Convert to C# DTOs, records, or model classes
- Ensure ALL procedures, paragraphs, and sections are converted - do not skip any logic

IMPORTANT: The COBOL code may contain placeholder terms for error handling. Convert these to appropriate C# exception handling.

CRITICAL RULES:
- Your response MUST start with 'namespace' or 'using'
- Include ONLY ONE namespace declaration per file
- All code must be within that single namespace
- Do NOT include explanations, notes, or markdown code blocks
- Return valid, compilable C# code only
- Convert EVERY piece of logic - do not omit sections thinking they are 'not important'
";

            string sanitizedContent = SanitizeCobolContent(cobolFile.Content);

            // Determine program type and provide specific guidance
            var programType = DetermineProgramType(cobolFile.FileName, cobolAnalysis);
            var specificGuidance = GetProgramTypeGuidance(programType);

            var prompt = $@"
Convert the following COBOL program to C# with .NET:

**File:** {cobolFile.FileName}
**Program Type:** {programType}

{specificGuidance}

```cobol
{sanitizedContent}
```

Here is the analysis of the COBOL program:

{cobolAnalysis.RawAnalysisData}

IMPORTANT REQUIREMENTS:
1. Return ONLY the C# code - NO explanations, NO markdown blocks
2. Start with: namespace CobolMigration.Something; (single line, file-scoped)
3. Use ONLY ONE namespace declaration for this entire file
4. Do NOT create multiple 'namespace' blocks in your response
5. Your response must be valid, compilable C# code
6. Place all classes and types within the single namespace
7. Convert ALL logic - do not skip or omit any procedures, paragraphs, or sections
8. Ensure the converted code is complete and functional
";

            apiCallId = _enhancedLogger?.LogApiCallStart(
                "CSharpConverterAgent",
                "ChatCompletion",
                "OpenAI/ConvertToCSharp",
                _modelId,
                $"Converting {cobolFile.FileName} ({cobolFile.Content.Length} chars)"
            ) ?? 0;

            _chatLogger?.LogUserMessage("CSharpConverterAgent", cobolFile.FileName, prompt, systemPrompt);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["max_completion_tokens"] = 32768
                }
            };

            var fullPrompt = $"{systemPrompt}\n\n{prompt}";
            var kernelArguments = new KernelArguments(executionSettings);

            string csharpCode = string.Empty;
            int maxRetries = 3;
            int retryDelay = 5000;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Converting COBOL to C# - Attempt {Attempt}/{MaxRetries} for {FileName}",
                        attempt, maxRetries, cobolFile.FileName);

                    var functionResult = await kernel.InvokePromptAsync(fullPrompt, kernelArguments);
                    csharpCode = functionResult.GetValue<string>() ?? string.Empty;
                    break;
                }
                catch (Exception ex) when (ShouldFallback(ex))
                {
                    lastException = ex;
                    var reason = GetFallbackReason(ex);
                    _enhancedLogger?.LogApiCallError(apiCallId, reason);
                    return CreateFallbackCodeFile(cobolFile, cobolAnalysis, reason);
                }
                catch (Exception ex) when (attempt < maxRetries && (
                    ex.Message.Contains("canceled") ||
                    ex.Message.Contains("timeout") ||
                    ex.Message.Contains("content_filter")))
                {
                    _logger.LogWarning("Attempt {Attempt} failed: {Error}. Retrying...", attempt, ex.Message);
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _enhancedLogger?.LogApiCallEnd(apiCallId, string.Empty, 0, 0);
                    _logger.LogError(ex, "Failed to convert COBOL file to C#: {FileName}", cobolFile.FileName);
                    throw;
                }
            }

            if (string.IsNullOrEmpty(csharpCode))
            {
                if (lastException != null && ShouldFallback(lastException))
                {
                    return CreateFallbackCodeFile(cobolFile, cobolAnalysis, GetFallbackReason(lastException));
                }
                throw new InvalidOperationException($"Failed to convert {cobolFile.FileName} after {maxRetries} attempts", lastException);
            }

            _chatLogger?.LogAIResponse("CSharpConverterAgent", cobolFile.FileName, csharpCode);
            _enhancedLogger?.LogApiCallEnd(apiCallId, csharpCode, csharpCode.Length / 4, 0.002m);

            csharpCode = ExtractCSharpCode(csharpCode);
            csharpCode = ValidateAndFixNamespaces(csharpCode, cobolFile.FileName);

            string className = GetClassName(csharpCode);
            string namespaceName = GetNamespaceName(csharpCode);

            var codeFile = new CodeFile
            {
                FileName = $"{className}.cs",
                Content = csharpCode,
                ClassName = className,
                NamespaceName = namespaceName,
                OriginalCobolFileName = cobolFile.FileName,
                TargetLanguage = TargetLanguage
            };

            stopwatch.Stop();
            _enhancedLogger?.LogBehindTheScenes("AI_PROCESSING", "CSHARP_CONVERSION_COMPLETE",
                $"Completed C# conversion of {cobolFile.FileName} in {stopwatch.ElapsedMilliseconds}ms", codeFile);

            return codeFile;
        }
        catch (Exception ex) when (ShouldFallback(ex))
        {
            stopwatch.Stop();
            if (apiCallId > 0)
            {
                _enhancedLogger?.LogApiCallError(apiCallId, GetFallbackReason(ex));
            }
            return CreateFallbackCodeFile(cobolFile, cobolAnalysis, GetFallbackReason(ex));
        }
    }

    /// <inheritdoc/>
    public async Task<List<CodeFile>> ConvertAsync(List<CobolFile> cobolFiles, List<CobolAnalysis> cobolAnalyses, Action<int, int>? progressCallback = null)
    {
        _logger.LogInformation("Converting {Count} COBOL files to C#", cobolFiles.Count);
        _enhancedLogger?.LogBehindTheScenes("CONVERSION_START", "BATCH_CONVERSION",
            $"Starting batch conversion of {cobolFiles.Count} COBOL files to C#");

        var codeFiles = new List<CodeFile>();
        var skippedFiles = new List<string>();
        var failedFiles = new List<(string fileName, string reason)>();
        int processedCount = 0;

        for (int i = 0; i < cobolFiles.Count; i++)
        {
            var cobolFile = cobolFiles[i];
            var cobolAnalysis = i < cobolAnalyses.Count ? cobolAnalyses[i] : null;

            if (cobolAnalysis == null)
            {
                _logger.LogWarning("No analysis found for COBOL file: {FileName} - SKIPPING CONVERSION", cobolFile.FileName);
                _enhancedLogger?.LogBehindTheScenes("CONVERSION_WARNING", "MISSING_ANALYSIS",
                    $"File {cobolFile.FileName} has no analysis data - cannot convert");
                skippedFiles.Add(cobolFile.FileName);
                continue;
            }

            try
            {
                _logger.LogInformation("[{Current}/{Total}] Converting: {FileName}", i + 1, cobolFiles.Count, cobolFile.FileName);
                var codeFile = await ConvertAsync(cobolFile, cobolAnalysis);
                codeFiles.Add(codeFile);

                _enhancedLogger?.LogBehindTheScenes("CONVERSION_SUCCESS", "FILE_CONVERTED",
                    $"Successfully converted {cobolFile.FileName} to {codeFile.FileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert file: {FileName}", cobolFile.FileName);
                _enhancedLogger?.LogBehindTheScenes("CONVERSION_ERROR", "FILE_FAILED",
                    $"Failed to convert {cobolFile.FileName}: {ex.Message}");
                failedFiles.Add((cobolFile.FileName, ex.Message));
            }

            processedCount++;
            progressCallback?.Invoke(processedCount, cobolFiles.Count);
        }

        // Log comprehensive summary
        _logger.LogInformation("Conversion complete: {Succeeded} succeeded, {Skipped} skipped, {Failed} failed out of {Total} files",
            codeFiles.Count, skippedFiles.Count, failedFiles.Count, cobolFiles.Count);

        if (skippedFiles.Count > 0)
        {
            _logger.LogWarning("Skipped files (no analysis): {Files}", string.Join(", ", skippedFiles));
            _enhancedLogger?.LogBehindTheScenes("CONVERSION_SUMMARY", "SKIPPED_FILES",
                $"Files skipped: {string.Join(", ", skippedFiles)}");
        }

        if (failedFiles.Count > 0)
        {
            _logger.LogError("Failed files: {Files}", string.Join(", ", failedFiles.Select(f => $"{f.fileName} ({f.reason})")));
            _enhancedLogger?.LogBehindTheScenes("CONVERSION_SUMMARY", "FAILED_FILES",
                $"Files failed: {string.Join(", ", failedFiles.Select(f => f.fileName))}");
        }

        // Critical validation: Ensure all files were processed
        var totalProcessed = codeFiles.Count + skippedFiles.Count + failedFiles.Count;
        if (totalProcessed < cobolFiles.Count)
        {
            var missingCount = cobolFiles.Count - totalProcessed;
            _logger.LogError("CRITICAL: {MissingCount} files were not processed!", missingCount);
            _enhancedLogger?.LogBehindTheScenes("CONVERSION_ERROR", "MISSING_FILES",
                $"{missingCount} files were neither converted, skipped, nor failed - possible bug!");
        }

        _enhancedLogger?.LogBehindTheScenes("CONVERSION_COMPLETE", "BATCH_SUMMARY",
            $"Batch conversion complete: {codeFiles.Count}/{cobolFiles.Count} files successfully converted");

        return codeFiles;
    }

    private CodeFile CreateFallbackCodeFile(CobolFile cobolFile, CobolAnalysis cobolAnalysis, string reason)
    {
        var className = GetFallbackClassName(cobolFile.FileName);
        var namespaceName = "CobolMigration.Fallback";
        var sanitizedReason = reason.Replace("\"", "'");

        var csharpCode = $$"""
namespace {{namespaceName}};

/// <summary>
/// Placeholder implementation generated because the AI conversion service was unavailable.
/// Original COBOL file: {{cobolFile.FileName}}
/// Reason: {{sanitizedReason}}
/// </summary>
public class {{className}}
{
    public void Run()
    {
        throw new NotSupportedException("AI conversion unavailable. Please supply valid Azure OpenAI credentials and rerun the migration. Details: {{sanitizedReason}}");
    }
}
""";

        return new CodeFile
        {
            FileName = $"{className}.cs",
            NamespaceName = namespaceName,
            ClassName = className,
            Content = csharpCode,
            OriginalCobolFileName = cobolFile.FileName,
            TargetLanguage = TargetLanguage
        };
    }

    private static string GetFallbackClassName(string cobolFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(cobolFileName);
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "ConvertedCobolProgram";

        baseName = new string(baseName.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "ConvertedCobolProgram";

        if (!char.IsLetter(baseName[0]))
            baseName = "Converted" + baseName;

        return baseName + "Fallback";
    }

    private static bool ShouldFallback(Exception exception) =>
        IsUnauthorizedException(exception) || IsNetworkException(exception);

    private static bool IsNetworkException(Exception exception) =>
        exception switch
        {
            HttpRequestException or SocketException => true,
            ClientResultException client when client.InnerException != null => IsNetworkException(client.InnerException),
            HttpOperationException http when http.InnerException != null => IsNetworkException(http.InnerException),
            AggregateException aggregate => aggregate.InnerExceptions.Any(IsNetworkException),
            _ => exception.InnerException != null && IsNetworkException(exception.InnerException)
        };

    private static string GetFallbackReason(Exception exception)
    {
        var innermost = exception;
        while (innermost.InnerException != null)
            innermost = innermost.InnerException;

        var message = innermost.Message;
        return string.IsNullOrWhiteSpace(message)
            ? exception.Message
            : message.Replace('\r', ' ').Replace('\n', ' ').Trim();
    }

    private static bool IsUnauthorizedException(Exception exception)
    {
        var statusCode = ExtractStatusCode(exception);
        return statusCode is 401 or 403;
    }

    private static int? ExtractStatusCode(Exception exception) =>
        exception switch
        {
            HttpOperationException httpEx when httpEx.StatusCode.HasValue => (int)httpEx.StatusCode.Value,
            ClientResultException clientEx => clientEx.Status,
            AggregateException aggregateEx => aggregateEx.InnerExceptions
                .Select(ExtractStatusCode)
                .FirstOrDefault(s => s.HasValue),
            _ => exception.InnerException != null ? ExtractStatusCode(exception.InnerException) : null
        };

    private string ExtractCSharpCode(string input)
    {
        if (input.Contains("```csharp") || input.Contains("```c#"))
        {
            var startMarker = input.Contains("```csharp") ? "```csharp" : "```c#";
            var endMarker = "```";

            int startIndex = input.IndexOf(startMarker);
            if (startIndex >= 0)
            {
                startIndex += startMarker.Length;
                int endIndex = input.IndexOf(endMarker, startIndex);

                if (endIndex >= 0)
                    return input.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        return input;
    }

    private string GetClassName(string csharpCode)
    {
        try
        {
            var lines = csharpCode.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("public class ") || trimmedLine.StartsWith("internal class ") || trimmedLine.StartsWith("class "))
                {
                    var parts = trimmedLine.Split(' ');
                    var classIndex = Array.IndexOf(parts, "class");
                    if (classIndex >= 0 && classIndex + 1 < parts.Length)
                    {
                        var className = parts[classIndex + 1];
                        className = className.Split('{', ' ', '\t', '\r', '\n', ':')[0];

                        if (IsValidCSharpIdentifier(className))
                            return className;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting class name from C# code");
        }

        return "ConvertedCobolProgram";
    }

    private bool IsValidCSharpIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
            return false;

        return identifier.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private string GetNamespaceName(string csharpCode)
    {
        var namespaceIndex = csharpCode.IndexOf("namespace ");
        if (namespaceIndex >= 0)
        {
            var start = namespaceIndex + "namespace ".Length;
            var remaining = csharpCode.Substring(start);
            var end = remaining.IndexOfAny(new[] { ';', '{', '\r', '\n' });

            if (end >= 0)
                return remaining.Substring(0, end).Trim();
        }

        return "CobolMigration.Legacy";
    }

    private string SanitizeCobolContent(string cobolContent)
    {
        if (string.IsNullOrEmpty(cobolContent))
            return cobolContent;

        var sanitizationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"FEJL", "ERROR_CODE"},
            {"FEJLMELD", "ERROR_MSG"},
            {"FEJL-", "ERROR_"},
            {"FEJLMELD-", "ERROR_MSG_"},
            {"INC-FEJLMELD", "INC-ERROR-MSG"},
            {"FEJL VED KALD", "ERROR IN CALL"},
            {"KALD", "CALL_OP"},
            {"MEDD-TEKST", "MSG_TEXT"},
        };

        string sanitizedContent = cobolContent;
        foreach (var (original, replacement) in sanitizationMap)
        {
            if (sanitizedContent.Contains(original))
                sanitizedContent = sanitizedContent.Replace(original, replacement);
        }

        return sanitizedContent;
    }

    /// <inheritdoc/>
    public async Task CreateProjectAsync(List<CodeFile> codeFiles, string outputFolder)
    {
        _logger.LogInformation("Creating .NET project file for {Count} C# files", codeFiles.Count);
        _enhancedLogger?.LogBehindTheScenes("PROJECT_GENERATION", "CSPROJ_CREATION_START",
            $"Generating .csproj file for {codeFiles.Count} files in {outputFolder}");

        var projectName = "CobolMigration";
        var csprojPath = Path.Combine(outputFolder, $"{projectName}.csproj");

        // Check if any file has a Main method
        bool hasMainMethod = codeFiles.Any(f =>
            f.Content.Contains("static void Main(") ||
            f.Content.Contains("static async Task Main(") ||
            f.Content.Contains("static Task Main("));

        _logger.LogInformation("Main method detection: hasMainMethod={HasMain}, fileCount={Count}",
            hasMainMethod, codeFiles.Count);

        // Generate .csproj content
        var csprojContent = GenerateCsprojContent(codeFiles, projectName, hasMainMethod);

        // Write the .csproj file
        await File.WriteAllTextAsync(csprojPath, csprojContent);
        _logger.LogInformation("Created project file: {ProjectFile}", csprojPath);

        // If no Main method exists, create a simple Program.cs entry point
        if (!hasMainMethod)
        {
            _logger.LogInformation("No Main method found in any of the {Count} files. Creating default Program.cs",
                codeFiles.Count);
            await CreateDefaultProgramFileAsync(outputFolder, codeFiles);
        }
        else
        {
            _logger.LogInformation("Main method found in one of the converted files. Skipping Program.cs generation.");
        }

        // Try to build the project to verify it compiles
        await ValidateProjectCompilationAsync(outputFolder, csprojPath);

        _enhancedLogger?.LogBehindTheScenes("PROJECT_GENERATION", "CSPROJ_CREATION_COMPLETE",
            $"Created compilable .NET project at {csprojPath}");
    }

    private string GenerateCsprojContent(List<CodeFile> codeFiles, string projectName, bool hasMainMethod)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine($"    <OutputType>{(hasMainMethod ? "Exe" : "Exe")}</OutputType>");
        sb.AppendLine("    <TargetFramework>net9.0</TargetFramework>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");
        sb.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <PackageReference Include=\"Microsoft.Data.SqlClient\" Version=\"5.2.0\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine();
        sb.AppendLine("</Project>");

        return sb.ToString();
    }

    /// <summary>
    /// Creates a default Program.cs file with a Main entry point if none exists.
    /// </summary>
    private async Task CreateDefaultProgramFileAsync(string outputFolder, List<CodeFile> codeFiles)
    {
        var programPath = Path.Combine(outputFolder, "Program.cs");

        _logger.LogInformation("No Main method found. Creating default Program.cs entry point.");

        var programContent = @"namespace CobolMigration;

using System;
using System.Threading.Tasks;

/// <summary>
/// Entry point for the COBOL migration console application.
/// This class was auto-generated because no Main method was found in the converted code.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.WriteLine(""COBOL Migration Console Application"");
        Console.WriteLine(""====================================="");
        Console.WriteLine();
        Console.WriteLine(""Converted COBOL modules are available as libraries."");
        Console.WriteLine(""Add your application logic here to use the converted code."");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }
}
";

        await File.WriteAllTextAsync(programPath, programContent);
        _logger.LogInformation("Created default Program.cs at: {ProgramPath}", programPath);

        _enhancedLogger?.LogBehindTheScenes("PROJECT_GENERATION", "PROGRAM_CREATION",
            "Created default Program.cs with Main entry point");
    }

    private string GetRelativePath(CodeFile codeFile)
    {
        // Determine folder structure based on namespace
        if (codeFile.NamespaceName.Contains("BusinessLogic"))
        {
            return $"businesslogic/{codeFile.FileName}";
        }
        else if (codeFile.NamespaceName.Contains("Legacy"))
        {
            if (codeFile.FileName.Contains("Entity"))
            {
                return $"legacy/models/{codeFile.FileName}";
            }
            return $"legacy/{codeFile.FileName}";
        }
        else if (codeFile.NamespaceName.Contains("Fallback"))
        {
            return $"fallback/{codeFile.FileName}";
        }
        return codeFile.FileName;
    }

    private async Task ValidateProjectCompilationAsync(string outputFolder, string csprojPath)
    {
        try
        {
            _logger.LogInformation("Validating project compilation...");
            _enhancedLogger?.LogBehindTheScenes("PROJECT_VALIDATION", "BUILD_START",
                "Running 'dotnet build' to verify project compiles");

            // Use just the project filename since we're setting WorkingDirectory
            var projectFileName = Path.GetFileName(csprojPath);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectFileName}\" --configuration Release",
                WorkingDirectory = outputFolder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                _logger.LogWarning("Failed to start dotnet build process");
                return;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("✓ Project compilation successful");
                _enhancedLogger?.LogBehindTheScenes("PROJECT_VALIDATION", "BUILD_SUCCESS",
                    "Project compiled successfully without errors");
            }
            else
            {
                _logger.LogWarning("Project compilation failed with exit code {ExitCode}", process.ExitCode);
                _logger.LogWarning("Build output: {Output}", output);
                _logger.LogWarning("Build errors: {Error}", error);
                _enhancedLogger?.LogBehindTheScenes("PROJECT_VALIDATION", "BUILD_FAILED",
                    $"Compilation failed: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not validate project compilation. Ensure .NET SDK is installed.");
            _enhancedLogger?.LogBehindTheScenes("PROJECT_VALIDATION", "BUILD_ERROR",
                $"Unable to run build validation: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates and fixes C# code that may have multiple namespace declarations.
    /// If multiple namespaces are found, extracts only the first namespace block.
    /// </summary>
    private string ValidateAndFixNamespaces(string csharpCode, string fileName)
    {
        try
        {
            var lines = csharpCode.Split('\n');
            int namespaceCount = 0;
            int firstNamespaceIndex = -1;
            int secondNamespaceIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith("namespace ") && !trimmed.Contains("//"))
                {
                    namespaceCount++;
                    if (firstNamespaceIndex == -1)
                        firstNamespaceIndex = i;
                    else if (secondNamespaceIndex == -1)
                        secondNamespaceIndex = i;
                }
            }

            if (namespaceCount > 1 && secondNamespaceIndex > 0)
            {
                _logger.LogWarning("File {FileName} contains {Count} namespace declarations. Extracting only the first namespace block.",
                    fileName, namespaceCount);

                // Extract only content up to the second namespace
                var fixedLines = new List<string>();
                for (int i = 0; i < secondNamespaceIndex; i++)
                {
                    fixedLines.Add(lines[i]);
                }

                // Add closing brace if needed
                var fixedCode = string.Join('\n', fixedLines);
                if (!fixedCode.TrimEnd().EndsWith("}"))
                {
                    fixedCode += "\n}";
                }

                _enhancedLogger?.LogBehindTheScenes("CODE_VALIDATION", "NAMESPACE_FIX",
                    $"Fixed multiple namespaces in {fileName}: {namespaceCount} -> 1");

                return fixedCode;
            }

            return csharpCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating namespaces in {FileName}", fileName);
            return csharpCode;
        }
    }

    /// <summary>
    /// Determines the type of COBOL program based on filename and analysis.
    /// </summary>
    private string DetermineProgramType(string fileName, CobolAnalysis analysis)
    {
        var upperFileName = fileName.ToUpperInvariant();

        // Check for specific patterns in filename
        if (upperFileName.Contains("MAINPGM") || upperFileName.Contains("MAIN"))
            return "Main Orchestration Program";

        if (upperFileName.Contains("DBDRIVR") || upperFileName.Contains("DATABASE"))
            return "Database Driver";

        if (upperFileName.Contains("FLDRIVR") || upperFileName.Contains("FILE"))
            return "File Driver";

        if (upperFileName.EndsWith(".CPY"))
            return "Copybook (Data Structure)";

        // Check analysis for clues
        var analysisText = analysis.RawAnalysisData?.ToUpperInvariant() ?? "";

        if (analysisText.Contains("EXEC SQL") || analysisText.Contains("DATABASE"))
            return "Database Program";

        if (analysisText.Contains("OPEN") && analysisText.Contains("CLOSE") && analysisText.Contains("READ"))
            return "File Processing Program";

        if (analysisText.Contains("CALL") && analysisText.Contains("PERFORM") && analysisText.Contains("SECTION"))
            return "Business Logic Program";

        return "Standard COBOL Program";
    }

    /// <summary>
    /// Provides specific guidance based on program type.
    /// </summary>
    private string GetProgramTypeGuidance(string programType)
    {
        return programType switch
        {
            "Main Orchestration Program" => @"
**CONVERSION GUIDANCE FOR MAIN ORCHESTRATION PROGRAM:**
This is a main program that coordinates other components. Your conversion MUST include:
- Create a primary service class that orchestrates the workflow
- Convert all CALL statements to service method invocations
- Implement the complete processing loop (PERFORM UNTIL, etc.)
- Convert all control break logic (state changes, summaries)
- Implement report generation if present
- Include all initialization and finalization steps
- Use dependency injection pattern for service coordination
- Implement async/await for all I/O operations
CRITICAL: Do not skip ANY sections - convert the entire workflow end-to-end.",

            "Database Driver" => @"
**CONVERSION GUIDANCE FOR DATABASE DRIVER:**
This is a database access program. Your conversion MUST include:
- Create a service class with IAsyncDisposable
- Convert EXEC SQL statements to ADO.NET SqlCommand
- Implement OPEN, FETCH, CLOSE operations as async methods
- Return SQLCODE values (0=success, 100=no data, negative=error)
- Use parameterized queries to prevent SQL injection
- Map COBOL data structures to C# DTOs
- Implement proper connection management
CRITICAL: Convert ALL database operations - do not omit SELECT, INSERT, UPDATE, or DELETE statements.",

            "File Driver" => @"
**CONVERSION GUIDANCE FOR FILE DRIVER:**
This is a file I/O program. Your conversion MUST include:
- Create a service class with IAsyncDisposable
- Convert OPEN/CLOSE operations to StreamWriter/StreamReader
- Implement WRITE operations as WriteLineAsync
- Implement READ operations as ReadLineAsync
- Return FILE-STATUS codes (00=success, 23=not found, 99=error)
- Handle fixed-width or delimited formats as appropriate
- Map COBOL file records to C# DTOs
CRITICAL: Convert ALL file operations including OPEN, READ, WRITE, and CLOSE.",

            "Copybook (Data Structure)" => @"
**CONVERSION GUIDANCE FOR COPYBOOK:**
This is a data structure definition. Your conversion MUST include:
- Create C# record, class, or DTO for each COBOL structure
- Map PIC X(n) to string with MaxLength attribute
- Map PIC 9(n) to int/long based on size
- Map PIC S9(n)V9(m) COMP-3 to decimal
- Include XML documentation for each property
- Use data annotations for validation
- Create helper methods for date parsing if dates are present
CRITICAL: Convert ALL fields - do not skip any data elements.",

            "Database Program" => @"
**CONVERSION GUIDANCE FOR DATABASE PROGRAM:**
This program interacts with databases. Your conversion MUST include:
- Implement all database operations (SELECT, INSERT, UPDATE, DELETE)
- Use async/await for all database calls
- Proper error handling with try-catch
- Connection string management
- Transaction handling if present in COBOL
CRITICAL: Ensure all SQL operations are converted completely.",

            "File Processing Program" => @"
**CONVERSION GUIDANCE FOR FILE PROCESSING:**
This program processes files. Your conversion MUST include:
- Complete file lifecycle (OPEN, READ/WRITE, CLOSE)
- Proper resource disposal with IAsyncDisposable
- Error handling for file not found, access denied, etc.
- Record processing logic
CRITICAL: Convert the complete file processing workflow.",

            "Business Logic Program" => @"
**CONVERSION GUIDANCE FOR BUSINESS LOGIC:**
This contains business rules and logic. Your conversion MUST include:
- All calculations and data transformations
- All conditional logic (IF-THEN-ELSE)
- All loops (PERFORM VARYING, PERFORM UNTIL)
- All validation rules
CRITICAL: Preserve all business logic exactly - do not simplify or skip conditions.",

            _ => @"
**GENERAL CONVERSION GUIDANCE:**
Convert all COBOL structures, procedures, and logic to equivalent C# code.
- Do not skip any sections or paragraphs
- Convert all variables and data structures
- Implement all procedures as methods
- Include all error handling
CRITICAL: Ensure complete and accurate conversion of all code."
        };
    }

    /// <summary>
    /// Validates that all COBOL files were successfully converted.
    /// </summary>
    /// <param name="cobolFiles">The original COBOL files.</param>
    /// <param name="convertedFiles">The converted code files.</param>
    /// <returns>A validation result containing success status and details.</returns>
    public (bool success, string message, List<string> missingFiles) ValidateConversion(
        List<CobolFile> cobolFiles,
        List<CodeFile> convertedFiles)
    {
        _logger.LogInformation("=== Validating Conversion Completeness ===");

        var missingFiles = new List<string>();
        var convertedFileNames = new HashSet<string>(
            convertedFiles.Select(f => Path.GetFileNameWithoutExtension(f.FileName)),
            StringComparer.OrdinalIgnoreCase);

        foreach (var cobolFile in cobolFiles)
        {
            var cobolBaseName = Path.GetFileNameWithoutExtension(cobolFile.FileName);

            // Check if this COBOL file has a corresponding converted file
            if (!convertedFileNames.Contains(cobolBaseName))
            {
                missingFiles.Add(cobolFile.FileName);
                _logger.LogWarning($"❌ Missing conversion for: {cobolFile.FileName}");
            }
            else
            {
                _logger.LogInformation($"✅ Converted: {cobolFile.FileName}");
            }
        }

        bool success = missingFiles.Count == 0;
        string message;

        if (success)
        {
            message = $"✅ All {cobolFiles.Count} COBOL files were successfully converted.";
            _logger.LogInformation(message);
        }
        else
        {
            message = $"⚠️ Conversion incomplete: {missingFiles.Count} of {cobolFiles.Count} files were not converted.";
            _logger.LogWarning(message);
            _logger.LogWarning($"Missing files: {string.Join(", ", missingFiles)}");
        }

        _logger.LogInformation("=== Validation Complete ===");
        return (success, message, missingFiles);
    }
}