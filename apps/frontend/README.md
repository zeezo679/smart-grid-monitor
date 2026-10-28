# Smart Grid Monitor — Frontend

The web dashboard for the Smart Grid Monitor application. Provides real-time operational awareness across 100,000 smart meters and 500 distribution transformers in Cairo and Giza.

## Tech Stack

- **Framework**: React 18 with TypeScript and Vite
- **Data Fetching & State**: TanStack Query (React Query) v5
- **Routing**: React Router v7
- **Styling**: Tailwind CSS with custom dark industrial control-room theme
- **Visualization**: Recharts (dual-axis composed demand charts)
- **Icons**: Lucide React
- **Testing**: Vitest + React Testing Library + Jest-DOM

## Architecture

```
apps/frontend/src/
├── app/                  # App shell, root routing, and React Query providers
├── components/
│   ├── layout/          # Sidebar navigation and collapsible AppLayout
│   └── ui/              # Reusable primitives (Skeleton, ErrorCard, EmptyState, StatusBadge)
├── features/
│   └── dashboard/       # Overview Dashboard feature module
│       ├── api/         # Typed HTTP fetchers calling ASP.NET Core endpoints
│       ├── components/  # DashboardHeader, KpiCard, KpiGrid, DemandChart, AlertSummary
│       ├── hooks/       # TanStack Query custom hooks
│       ├── pages/       # OverviewPage container
│       ├── types/       # TypeScript models matching backend DTO contracts
│       └── __tests__/   # Feature unit and integration tests
├── lib/
│   ├── api-client.ts    # Centralized apiFetch wrapper with error handling
│   ├── formatters.ts    # Energy/power unit conversion (kWh→GWh, kW→MW) and formatting
│   ├── query-client.ts  # TanStack Query client configuration
│   └── __tests__/       # Formatter unit tests
├── styles/
│   └── globals.css      # Dark control-room utility classes, scrollbars, and fonts
└── test/
    └── setup.ts         # Testing setup and DOM polyfills
```

## Getting Started

### 1. Environment Setup

Copy `.env.example` to `.env`:

```bash
cp .env.example .env
```

Default configuration:
```ini
VITE_API_BASE_URL=http://localhost:5091
```

### 2. Install Dependencies

```bash
npm install
```

### 3. Run Development Server

```bash
npm run dev
```

The application will be accessible at `http://localhost:5173`.

### 4. Run Tests & Validation

```bash
# Run unit and integration tests
npm test

# Type checking
npx tsc --noEmit

# Production build
npm run build
```

## Backend Integration

The frontend connects exclusively through HTTP to the ASP.NET Core API at `VITE_API_BASE_URL`:
- `GET /api/dashboard/summary?from=YYYY-MM-DD&to=YYYY-MM-DD` — Key metrics and alert counts
- `GET /api/dashboard/demand?from=YYYY-MM-DD&to=YYYY-MM-DD` — Time-series daily energy and peak demand

Default date range defaults to `2026-08-01` to `2026-08-02` to align with the backend's mock provider fixtures.
