import { type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { AlertTriangle, RefreshCw } from 'lucide-react';

/* ------------------------------------------------------------------ */
/*  Skeleton                                                          */
/* ------------------------------------------------------------------ */
export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('skeleton', className)} />;
}

/* ------------------------------------------------------------------ */
/*  ErrorCard                                                         */
/* ------------------------------------------------------------------ */
interface ErrorCardProps {
  title?: string;
  message: string;
  onRetry?: () => void;
  className?: string;
}

export function ErrorCard({ title = 'Something went wrong', message, onRetry, className }: ErrorCardProps) {
  return (
    <div
      className={cn(
        'card flex flex-col items-center justify-center gap-3 p-8 text-center',
        className,
      )}
      role="alert"
    >
      <AlertTriangle className="h-8 w-8 text-status-overload" aria-hidden="true" />
      <h3 className="text-sm font-semibold text-gray-200">{title}</h3>
      <p className="text-xs text-gray-400 max-w-md">{message}</p>
      {onRetry && (
        <button onClick={onRetry} className="btn-ghost mt-2" type="button">
          <RefreshCw className="h-3.5 w-3.5" aria-hidden="true" />
          Retry
        </button>
      )}
    </div>
  );
}

/* ------------------------------------------------------------------ */
/*  EmptyState                                                        */
/* ------------------------------------------------------------------ */
interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  className?: string;
}

export function EmptyState({ icon, title, description, className }: EmptyStateProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center gap-2 p-8 text-center', className)}>
      {icon && <div className="text-gray-500">{icon}</div>}
      <h3 className="text-sm font-medium text-gray-300">{title}</h3>
      {description && <p className="text-xs text-gray-500 max-w-sm">{description}</p>}
    </div>
  );
}

/* ------------------------------------------------------------------ */
/*  StatusBadge                                                       */
/* ------------------------------------------------------------------ */
type BadgeVariant = 'healthy' | 'theft' | 'overload' | 'linedrop' | 'neutral';

const badgeStyles: Record<BadgeVariant, string> = {
  healthy: 'bg-status-healthy/15 text-status-healthy border-status-healthy/30',
  theft: 'bg-status-theft/15 text-status-theft border-status-theft/30',
  overload: 'bg-status-overload/15 text-status-overload border-status-overload/30',
  linedrop: 'bg-status-linedrop/15 text-status-linedrop border-status-linedrop/30',
  neutral: 'bg-gray-500/15 text-gray-400 border-gray-500/30',
};

interface StatusBadgeProps {
  variant: BadgeVariant;
  children: ReactNode;
  className?: string;
}

export function StatusBadge({ variant, children, className }: StatusBadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 px-2.5 py-0.5 text-xs font-medium rounded-full border',
        badgeStyles[variant],
        className,
      )}
    >
      <span
        className={cn('h-1.5 w-1.5 rounded-full', {
          'bg-status-healthy': variant === 'healthy',
          'bg-status-theft': variant === 'theft',
          'bg-status-overload': variant === 'overload',
          'bg-status-linedrop': variant === 'linedrop',
          'bg-gray-400': variant === 'neutral',
        })}
        aria-hidden="true"
      />
      {children}
    </span>
  );
}
