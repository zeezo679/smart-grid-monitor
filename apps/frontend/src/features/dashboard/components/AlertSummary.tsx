import { ShieldAlert, AlertTriangle, Activity, ArrowRight } from 'lucide-react';
import { StatusBadge, Skeleton } from '@/components/ui';
import { formatInt } from '@/lib/formatters';
import type { DashboardOverview } from '../types';

interface AlertSummaryProps {
  data: DashboardOverview | undefined;
  isLoading: boolean;
}

interface AlertTypeRow {
  label: string;
  description: string;
  count: number | null | undefined;
  variant: 'theft' | 'overload' | 'linedrop';
  icon: React.ElementType;
}

/**
 * Compact alert breakdown panel showing the three alert types
 * and their affected transformer counts from the summary endpoint.
 */
export function AlertSummary({ data, isLoading }: AlertSummaryProps) {
  const rows: AlertTypeRow[] = [
    {
      label: 'Theft',
      description: 'Transformer-level energy loss detected',
      count: data?.theftTransformers,
      variant: 'theft',
      icon: ShieldAlert,
    },
    {
      label: 'Overload',
      description: 'Load ratio ≥ 85% sustained',
      count: data?.overloadTransformers,
      variant: 'overload',
      icon: AlertTriangle,
    },
    {
      label: 'Voltage Drop',
      description: 'Voltage < 207V sustained',
      count: data?.linedropTransformers,
      variant: 'linedrop',
      icon: Activity,
    },
  ];

  return (
    <div className="card p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-sm font-semibold text-gray-200">Alert Breakdown</h2>
        {data?.totalAlerts != null && (
          <StatusBadge variant={data.totalAlerts > 0 ? 'overload' : 'healthy'}>
            {formatInt(data.totalAlerts)} total alert{data.totalAlerts !== 1 ? 's' : ''}
          </StatusBadge>
        )}
      </div>

      <div className="space-y-3">
        {rows.map((row) => {
          const Icon = row.icon;
          return (
            <div
              key={row.variant}
              className="flex items-center gap-3 rounded-lg border border-grid-border bg-grid-surface/50 p-3"
            >
              <div
                className={`shrink-0 rounded-lg p-2 ${
                  row.variant === 'theft'
                    ? 'bg-status-theft/10 text-status-theft'
                    : row.variant === 'overload'
                      ? 'bg-status-overload/10 text-status-overload'
                      : 'bg-status-linedrop/10 text-status-linedrop'
                }`}
              >
                <Icon className="h-4 w-4" aria-hidden="true" />
              </div>

              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-gray-200">{row.label}</span>
                  <StatusBadge variant={row.variant}>
                    {row.variant}
                  </StatusBadge>
                </div>
                <p className="text-xs text-gray-500 mt-0.5">{row.description}</p>
              </div>

              <div className="text-right shrink-0">
                {isLoading ? (
                  <Skeleton className="h-6 w-8" />
                ) : (
                  <span className="text-lg font-semibold text-gray-200 font-mono">
                    {formatInt(row.count)}
                  </span>
                )}
                <p className="text-[10px] text-gray-600 uppercase tracking-wider">
                  transformers
                </p>
              </div>
            </div>
          );
        })}
      </div>

      {/* Link to future Alert Log */}
      <div className="mt-4 flex items-center gap-1.5 text-xs text-gray-600">
        <ArrowRight className="h-3 w-3" aria-hidden="true" />
        <span>Detailed alert log available in a future release</span>
      </div>
    </div>
  );
}
