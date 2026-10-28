# Smart Grid Monitor — API & Data Contract

## 1. Application Purpose

The Smart Grid Monitor web application exposes analytical results from an existing
data engineering pipeline that simulates electricity consumption for **100,000 meters**
connected to **500 transformers**.

The backend is a read-only analytics API. It does **not** re-implement detection
algorithms. All anomaly detection is performed by the dbt models in Snowflake.

---

## 2. Backend Project Structure

```
apps/backend/
├── SmartGrid.sln                           # Solution file
├── SmartGrid.slnx                          # .NET 10 solution (XML format)
│
├── src/
│   ├── SmartGrid.Api/                      # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   └── HealthController.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── SmartGrid.Api.csproj
│   │
│   ├── SmartGrid.Application/             # DTOs, query models, repository interfaces
│   │   ├── Abstractions/
│   │   │   ├── Persistence/               # Repository interfaces
│   │   │   └── Time/                      # IDateTimeProvider
│   │   ├── Common/
│   │   │   └── Models/                    # DateRange, PageRequest, PagedResult<T>
│   │   ├── Dashboard/
│   │   │   └── Models/                    # DashboardOverviewDto, DailyDemandPointDto
│   │   ├── Alerts/
│   │   │   ├── Models/                    # AlertDto, AlertSummaryByTypeDto, EventTypes
│   │   │   └── Queries/                   # AlertSearchCriteria
│   │   ├── Transformers/
│   │   │   ├── Models/                    # TransformerListItemDto, TransformerDetailsDto, etc.
│   │   │   └── Queries/                   # Search, Peak, Reading criteria
│   │   ├── Accuracy/
│   │   │   └── Models/                    # AccuracyByTypeDto, AccuracyReportDto
│   │   └── SmartGrid.Application.csproj
│   │
│   └── SmartGrid.Infrastructure/          # Will contain mock & Snowflake implementations
│       └── SmartGrid.Infrastructure.csproj
│
├── tests/
│   └── SmartGrid.Application.Tests/       # Unit tests for shared models & validation
│       └── SmartGrid.Application.Tests.csproj
│
└── API_CONTRACT.md                        # This document
```

---

## 3. Project Reference Rules

```
SmartGrid.Api
    ├── SmartGrid.Application
    └── SmartGrid.Infrastructure

SmartGrid.Infrastructure
    └── SmartGrid.Application

SmartGrid.Application
    └── (no project dependencies)

SmartGrid.Application.Tests
    └── SmartGrid.Application
```

**The Application project must never reference:**

- Snowflake drivers or SDKs
- AWS SDKs
- Entity Framework Core
- ASP.NET Core controller types
- Infrastructure implementation classes

---

## 4. Repository Interfaces

All interfaces live in `SmartGrid.Application.Abstractions.Persistence`.

| Interface                    | Responsibility                                     |
|------------------------------|---------------------------------------------------|
| `IDashboardReadRepository`   | Dashboard overview, daily demand series            |
| `IAlertReadRepository`       | Alert search, get-by-id, summary by type           |
| `ITransformerReadRepository` | Transformer search, details, peaks, readings       |
| `IAccuracyReadRepository`    | Detection accuracy report                          |

### Conventions

- All methods are asynchronous (`Task<T>`) and end with `Async`.
- All accept `CancellationToken`.
- Return Application DTOs — never raw database readers or SQL.
- Use nullable return types when a record may not exist (e.g. `GetByIdAsync`).
- No HTTP-specific types. No Snowflake-specific types.
- These are read-only repositories. No save/update/delete methods.

---

## 5. DTOs and Key Fields

### 5.1 Common Models

| Type             | Fields                               | Notes                               |
|------------------|--------------------------------------|-------------------------------------|
| `DateRange`      | `Start`, `End` (both `DateOnly`)     | Both inclusive. Start ≤ End enforced.|
| `PageRequest`    | `Page` (1-indexed), `PageSize`       | Max page size = 100. Validated.     |
| `PagedResult<T>` | `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages` | `Empty()` factory for empty results.|

### 5.2 Dashboard

| DTO                    | Key Fields                                                    | Source Model(s)                       |
|------------------------|---------------------------------------------------------------|---------------------------------------|
| `DailyDemandPointDto`  | `DemandDate`, `TotalDailyKwh`, `PeakNetworkKw`               | `kpi_daily_demand`                    |
| `DashboardOverviewDto` | `PeriodStart`, `PeriodEnd`, `TotalEnergyKwh`, `PeakNetworkKw`, `TotalAlerts`, `TheftTransformers`, `OverloadTransformers`, `LinedropTransformers`, `AffectedTransformers`, `LatestReadingTimestamp` | `kpi_daily_demand` + `anomaly_flags` + `dim_transformers` + `fact_transformer_readings` |

**Important:** `PeakNetworkKw` is the **maximum** recorded combined network power
across all time slots in the period — not the sum of daily peaks.

All values on `DashboardOverviewDto` are nullable because the warehouse may have no data.

### 5.3 Alerts

| DTO                     | Key Fields                                                    | Source Model(s)                    |
|-------------------------|---------------------------------------------------------------|------------------------------------|
| `AlertDto`              | `AnomalyId`, `EventType`, `EntityId`, `TransformerId`, `District`, `Governorate`, `StartTimestamp`, `EndTimestamp` | `anomaly_flags` JOIN `dim_transformers` |
| `AlertSummaryByTypeDto` | `EventType`, `TotalAlerts`, `AffectedTransformers`            | Aggregation over `anomaly_flags`   |

**Alert event types:** `theft`, `overload`, `line_drop` (case-sensitive, defined in `EventTypes`).

**Query:** `AlertSearchCriteria` — optional `Period`, `EventType`, `District`, `TransformerId` + required `Pagination`.

### 5.4 Transformers

| DTO                       | Key Fields                                                  | Source Model(s)                 |
|---------------------------|-------------------------------------------------------------|---------------------------------|
| `TransformerListItemDto`  | `TransformerId`, `District`, `Governorate`, `FeederId`, `SubstationId`, `CapacityKw`, `Latitude`, `Longitude` | `dim_transformers` |
| `TransformerDetailsDto`   | Same as list + extensible for detail page                   | `dim_transformers`              |
| `TransformerPeakDto`      | `TransformerId`, `District`, `CapacityKw`, `PeakKwRecorded`, `PeakLoadPercentage` | `kpi_transformer_peak` |
| `TransformerReadingDto`   | `TransformerId`, `ReadingTimestamp`, `ActivePowerKw`, `EnergyKwh`, `Voltage` | `fact_transformer_readings` |

**Queries:**

- `TransformerSearchCriteria` — optional `District`, `Governorate`, `FeederId`, `SubstationId` + required `Pagination`.
- `TransformerPeakCriteria` — optional `District` + required `Pagination`.
- `TransformerReadingCriteria` — required `TransformerId`, `Period`, `Pagination`.

### 5.5 Accuracy

| DTO                  | Key Fields                                                              | Source Model(s)      |
|----------------------|-------------------------------------------------------------------------|----------------------|
| `AccuracyByTypeDto`  | `EventType`, `TruePositives`, `FalsePositives`, `FalseNegatives`, `Precision`, `Recall` | `accuracy_summary` |
| `AccuracyReportDto`  | `ByType`, `TotalTruePositives`, `TotalFalsePositives`, `TotalFalseNegatives`, `OverallPrecision`, `OverallRecall` | Computed from `accuracy_summary` |

---

## 6. Date Filtering Behavior

| Repository Method           | Date Filter Behavior                                          |
|-----------------------------|---------------------------------------------------------------|
| Dashboard overview          | Required `DateRange`. Filters `kpi_daily_demand.demand_date` and `anomaly_flags.end_ts`. |
| Daily demand                | Required `DateRange`. Filters `kpi_daily_demand.demand_date`. |
| Alert search                | Optional `DateRange`. Filters on `anomaly_flags.end_ts`.      |
| Alert summary by type       | Required `DateRange`. Filters on `anomaly_flags.end_ts`.      |
| Transformer search          | No date filter (dimension table).                             |
| Transformer peaks           | **No date filter.** The source model has no date column.      |
| Transformer readings        | Required `DateRange`. Filters on `fact_transformer_readings.reading_ts`. |
| Accuracy report             | No date filter (global report).                               |

---

## 7. Pagination Conventions

- Pages are **1-indexed**.
- Minimum page size: 1. Maximum page size: **100**.
- Invalid values are rejected with `ArgumentOutOfRangeException` in `PageRequest`.
- `PagedResult<T>.TotalPages` is computed as ⌈TotalCount / PageSize⌉.
- `PagedResult<T>.Empty()` creates an empty result with zero items and zero total count.

---

## 8. Supported Alert Types

Exactly three event types, as defined in the dbt schema:

| Event Type   | dbt Source           | Description                                            |
|--------------|----------------------|--------------------------------------------------------|
| `theft`      | `check_theft.sql`    | >11% energy loss in rolling 4h window, ≥95% meter readings present |
| `overload`   | `check_overload.sql` | Load ratio ≥ 85% for 2 consecutive readings (30 min)  |
| `line_drop`  | `check_line_drop.sql`| Voltage < 207V for 4 consecutive readings (1 hour)    |

Event type values are **case-sensitive** and match the lowercase strings in the warehouse.

---

## 9. Units

| Quantity     | Unit    | Used In                                            |
|--------------|---------|----------------------------------------------------|
| Power        | kW      | `DailyDemandPointDto.PeakNetworkKw`, `TransformerPeakDto.PeakKwRecorded`, `TransformerReadingDto.ActivePowerKw`, `TransformerListItemDto.CapacityKw` |
| Energy       | kWh     | `DailyDemandPointDto.TotalDailyKwh`, `TransformerReadingDto.EnergyKwh`, `DashboardOverviewDto.TotalEnergyKwh` |
| Voltage      | V       | `TransformerReadingDto.Voltage`                     |
| Load         | %       | `TransformerPeakDto.PeakLoadPercentage`             |
| Precision    | ratio   | `AccuracyByTypeDto.Precision` (0.0–1.0)            |
| Recall       | ratio   | `AccuracyByTypeDto.Recall` (0.0–1.0)               |

No unit conversions are performed in the Application layer.

---

## 10. Warehouse Model → DTO Mapping

| Warehouse Model               | Snowflake Columns                                          | C# DTO                     |
|-------------------------------|------------------------------------------------------------|-----------------------------|
| `kpi_daily_demand`            | `demand_date`, `total_daily_kwh`, `peak_network_kw`        | `DailyDemandPointDto`       |
| `kpi_transformer_peak`        | `transformer_id`, `district`, `capacity_kw`, `peak_kw_recorded`, `peak_load_percentage` | `TransformerPeakDto` |
| `dim_transformers`            | `transformer_id`, `feeder_id`, `substation_id`, `district`, `governorate`, `capacity_kw`, `lat`, `lon` | `TransformerListItemDto`, `TransformerDetailsDto` |
| `fact_transformer_readings`   | `transformer_id`, `reading_ts`, `kw`, `kwh`, `voltage`    | `TransformerReadingDto`     |
| `anomaly_flags`               | `anomaly_id`, `event_type`, `entity_id`, `transformer_id`, `start_ts`, `end_ts` | `AlertDto` (partial)   |
| `accuracy_summary`            | `event_type`, `true_positives`, `false_positives`, `false_negatives`, `precision`, `recall` | `AccuracyByTypeDto` |

---

## 11. Required Joins

### Alerts ↔ Transformer Metadata

The `anomaly_flags` model does **not** contain `district` or `governorate`.
These fields must be obtained by joining:

```sql
anomaly_flags.transformer_id → dim_transformers.transformer_id
```

The Snowflake/mock repository implementation is responsible for performing this join
and populating `AlertDto.District` and `AlertDto.Governorate`.

### Dashboard Overview

The dashboard overview aggregates from multiple tables:

- Energy/power: `kpi_daily_demand` filtered by `demand_date`
- Alert counts: `anomaly_flags` filtered by `end_ts`, with district via `dim_transformers`
- Freshness: `MAX(reading_ts)` from `fact_transformer_readings`

---

## 12. Transformer Peaks vs. Date-Filtered Demand

These are two fundamentally different metrics:

| Metric                    | Source                    | Date Filter | Meaning                              |
|---------------------------|---------------------------|-------------|--------------------------------------|
| Daily demand peak         | `kpi_daily_demand`        | ✅ Yes      | Network-wide peak kW on a given day  |
| Transformer historical peak | `kpi_transformer_peak`  | ❌ No       | Highest kW ever recorded for a transformer across **all** available data |

The `kpi_transformer_peak` model has no date column. Querying "peak for the last 7 days"
is not supported by this model. A date-filtered peak would require querying
`fact_transformer_readings` directly.

---

## 13. Accuracy Metric Calculations

Overall precision and recall are computed from **aggregate counts**, never by averaging
per-type percentages.

```
Overall Precision = Total TP / (Total TP + Total FP)
Overall Recall    = Total TP / (Total TP + Total FN)
```

Both are **nullable** — `null` when the denominator is zero (e.g., no flags raised at all).

Per-type precision and recall come directly from `accuracy_summary` and are also nullable
for zero-denominator cases.

---

## 14. Mappings Requiring Live Snowflake Verification

The following have been derived from dbt SQL but need confirmation against the actual
Snowflake schema:

| Item | What to Verify |
|------|---------------|
| Column types | `demand_date` — is it `DATE` or `TIMESTAMP`? C# maps to `DateOnly`. |
| Column types | `capacity_kw` — is it `NUMBER(x,y)` or `FLOAT`? C# maps to `decimal`. |
| Column types | `lat`, `lon` — are they `FLOAT` or `NUMBER`? C# maps to `double`. |
| Column types | `peak_load_percentage` — verify `ROUND(..., 2)` returns `NUMBER(x,2)`. |
| Column types | `kw`, `kwh`, `voltage` — verify numeric types in `transformer_readings` raw source. |
| Column types | `precision`, `recall` in `accuracy_summary` — verify `ROUND(..., 3)` produces `NUMBER(x,3)`. |
| Null handling | Can `kpi_daily_demand` rows have NULL `peak_network_kw`? |
| Timestamp TZ | `reading_ts`, `start_ts`, `end_ts` — `TIMESTAMP_NTZ` or `TIMESTAMP_TZ`? |
| Row counts | `dim_transformers` — confirm exactly 500 rows in current data. |
| Anomaly IDs | Verify format matches pattern `{transformer_id}-{type_code}-{timestamp}`. |

These will be verified by the teammate implementing the Snowflake repository.

---

## 15. Mock & Snowflake Provider Strategy

Both providers implement the same four repository interfaces:

```
IDashboardReadRepository
IAlertReadRepository
ITransformerReadRepository
IAccuracyReadRepository
```

### Mock Provider (Infrastructure)

- Returns deterministic, hardcoded data.
- No external dependencies.
- Used during frontend development and integration testing.
- Respects pagination and date-range filtering with in-memory LINQ.

### Snowflake Provider (Infrastructure)

- Uses the Snowflake .NET driver.
- Executes parameterised SQL against the warehouse models.
- Performs the alert ↔ dim_transformers join server-side.
- Connection string provided via configuration, never committed to source control.

Both will be registered via dependency injection in `Program.cs`:

```csharp
// Example (to be implemented in a later milestone):
// builder.Services.AddScoped<IDashboardReadRepository, MockDashboardRepository>();
// — or —
// builder.Services.AddScoped<IDashboardReadRepository, SnowflakeDashboardRepository>();
```

Provider selection will be controlled by configuration (e.g., `DataProvider:Type` in
`appsettings.json` or environment variables).

---

## 16. Timestamp Convention

All `DateTimeOffset` values at the API boundary represent **UTC** timestamps.

The `reading_ts` column in the warehouse stores the start of each 15-minute slot in UTC.

---

## 17. Health Endpoint

```
GET /api/health
```

Returns:

```json
{
  "status": "healthy",
  "timestamp": "2024-05-15T12:00:00+00:00",
  "service": "SmartGrid.Api"
}
```

This confirms the API process is running. It does **not** verify Snowflake connectivity.
