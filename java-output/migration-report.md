# COBOL to Java Migration Report
Generated: 2025-11-12 14:28:00 UTC
Total Migration Time: 00:45:02.9108143

## 📊 Migration Overview
- **Source Files**: 12 COBOL files
- **Generated Java Files**: 12
- **Dependencies Found**: 11
- **Copybooks Analyzed**: 7
- **Average Dependencies per Program**: 2.2

## 🗂️ Java File Mapping
| COBOL File | Java File | Type |
|------------|-----------|------|
| DBDRIVR1.cbl | Policy | Program |
| DBDRIVR2.cbl | TrackingRecord | Program |
| FLDRIVR1.cbl | AgentRecord | Program |
| FLDRIVR2.cbl | NotificationFileDriverService | Program |
| MAINPGM.cbl | PolicyRecord | Program |
| AGNTNTFY.cpy | AgentNotifyRecord | Copybook |
| CAGENT.cpy | AgentRecord | Copybook |
| CPOLICY.cpy | PolicyRecord | Copybook |
| CUSTNTFY.cpy | CustomerNotifyRecord | Copybook |
| DCOVERAG.cpy | Coverage | Copybook |
| DPOLICY.cpy | Policy | Copybook |
| DTRAKING.cpy | TTracking | Copybook |

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
5. Configure Quarkus application properties for Java

## 📁 Generated Files
- `dependency-map.json` - Complete dependency analysis
- `dependency-diagram.md` - Mermaid dependency visualization
- `migration-conversation-log.md` - AI agent conversation log
- Individual Java files in respective packages
