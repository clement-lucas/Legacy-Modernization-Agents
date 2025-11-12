# COBOL Dependency Diagram

```mermaid
graph TB
  %% Styles
  classDef program fill:#1f77b4,stroke:#0e3c5e,stroke-width:2px,color:#fff;
  classDef copybook fill:#ffdb58,stroke:#bfa600,stroke-width:2px,color:#333;
  classDef subgraphBg fill:#f5f5f5,stroke:#bbb,stroke-width:1px;

  %% Programs Subgraph
  subgraph COBOL Programs
    direction TB
    DBDRIVR1["DBDRIVR1.cbl"]
    DBDRIVR2["DBDRIVR2.cbl"]
    FLDRIVR1["FLDRIVR1.cbl"]
    FLDRIVR2["FLDRIVR2.cbl"]
    MAINPGM["MAINPGM.cbl"]
  end
  class DBDRIVR1,DBDRIVR2,FLDRIVR1,FLDRIVR2,MAINPGM program;
  class COBOL Programs subgraphBg;

  %% Copybooks Subgraph
  subgraph Copybooks
    direction TB
    SQLCA["SQLCA.cpy"]
    DPOLICY["DPOLICY.cpy"]
    DCOVERAG["DCOVERAG.cpy"]
    DTRAKING["DTRAKING.cpy"]
    CAGENT["CAGENT.cpy"]
    CPOLICY["CPOLICY.cpy"]
    CUSTNTFY["CUSTNTFY.cpy"]
    AGNTNTFY["AGNTNTFY.cpy"]
  end
  class SQLCA,DPOLICY,DCOVERAG,DTRAKING,CAGENT,CPOLICY,CUSTNTFY,AGNTNTFY copybook;
  class Copybooks subgraphBg;

  %% Dependencies
  DBDRIVR1 -->|COPY| SQLCA
  DBDRIVR1 -->|COPY| DPOLICY
  DBDRIVR1 -->|COPY| DCOVERAG
  DBDRIVR1 -->|COPY| DTRAKING

  DBDRIVR2 -->|COPY| SQLCA
  DBDRIVR2 -->|COPY| DTRAKING

  FLDRIVR1 -->|COPY| CAGENT

  MAINPGM -->|COPY| CPOLICY
  MAINPGM -->|COPY| CAGENT
  MAINPGM -->|COPY| CUSTNTFY
  MAINPGM -->|COPY| AGNTNTFY
```