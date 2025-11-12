# COBOL to CSharp Migration Report
Generated: 2025-11-12 14:28:00 UTC
Total Migration Time: 00:45:02.9291686

## 📊 Migration Overview
- **Source Files**: 12 COBOL files
- **Generated C# Files**: 12
- **Dependencies Found**: 11
- **Copybooks Analyzed**: 7
- **Average Dependencies per Program**: 2.2

## 🗂️ C# File Mapping
| COBOL File | C# File | Type |
|------------|---------|------|
| DBDRIVR1.cbl | PolicyDriver | Program |
| DBDRIVR2.cbl | TrackingRepository | Program |
| FLDRIVR1.cbl | AgentFileInput | Program |
| FLDRIVR2.cbl | NotificationFileService | Program |
| MAINPGM.cbl | PolicyExpiryBatchProcessor | Program |
| AGNTNTFY.cpy | for | Copybook |
| CAGENT.cpy | AgentService | Copybook |
| CPOLICY.cpy | PolicyRecord | Copybook |
| CUSTNTFY.cpy | containing | Copybook |
| DCOVERAG.cpy | CoverageRepository | Copybook |
| DPOLICY.cpy | PolicyRepository | Copybook |
| DTRAKING.cpy | TrackingRecord | Copybook |

## 🔗 Dependency Analysis
### Most Used Copybooks
- **SQLCA.cpy**: Used by 2 programs
- **DTRAKING.cpy**: Used by 2 programs
- **CAGENT.cpy**: Used by 2 programs
- **DPOLICY.cpy**: Used by 1 programs
- **DCOVERAG.cpy**: Used by 1 programs
- **CPOLICY.cpy**: Used by 1 programs
- **CUSTNTFY.cpy**: Used by 1 programs
- **AGNTNTFY.cpy**: Used by 1 programs

## 📈 Migration Metrics
- **Files per Minute**: 0.3
- **Average File Size**: 5175 characters
- **Total Lines of Code**: 1,372

## 🚀 Next Steps
1. Review generated files for accuracy
2. Run unit tests (if UnitTestAgent is configured)
3. Check dependency diagram for architecture insights
4. Validate business logic in converted code
6. Configure appsettings.json and dependency injection for C#

## 📁 Generated Files
- `dependency-map.json` - Complete dependency analysis
- `dependency-diagram.md` - Mermaid dependency visualization
- `migration-conversation-log.md` - AI agent conversation log
- Individual C# files in respective namespaces
