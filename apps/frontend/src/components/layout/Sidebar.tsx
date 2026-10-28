import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import {
  LayoutDashboard,
  Map,
  Bell,
  Cpu,
  Target,
  ChevronLeft,
  ChevronRight,
  Zap,
} from 'lucide-react';

interface NavItem {
  label: string;
  path: string;
  icon: React.ElementType;
  enabled: boolean;
}

const navigation: NavItem[] = [
  { label: 'Overview', path: '/', icon: LayoutDashboard, enabled: true },
  { label: 'Alert Map', path: '/alerts/map', icon: Map, enabled: true },
  { label: 'Alert Log', path: '/alert-log', icon: Bell, enabled: false },
  { label: 'Transformers', path: '/transformers', icon: Cpu, enabled: false },
  { label: 'Accuracy', path: '/accuracy', icon: Target, enabled: false },
];

export function Sidebar() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();

  return (
    <aside
      className={cn(
        'fixed inset-y-0 left-0 z-30 flex flex-col border-r border-grid-border bg-grid-surface transition-all duration-200',
        collapsed ? 'w-16' : 'w-56',
      )}
    >
      {/* Brand */}
      <div className="flex h-14 items-center gap-2.5 border-b border-grid-border px-4">
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-brand">
          <Zap className="h-4 w-4 text-white" aria-hidden="true" />
        </div>
        {!collapsed && (
          <span className="text-sm font-semibold tracking-tight text-gray-100 whitespace-nowrap">
            Smart Grid Monitor
          </span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-0.5 overflow-y-auto px-2 py-3" aria-label="Main navigation">
        {navigation.map((item) => {
          const Icon = item.icon;
          const isActive =
            location.pathname === item.path ||
            (item.path === '/alerts/map' &&
              (location.pathname === '/alert-map' || location.pathname.startsWith('/alerts/map')));

          if (!item.enabled) {
            return (
              <div
                key={item.path}
                className={cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-600 cursor-not-allowed',
                  collapsed && 'justify-center px-0',
                )}
                title={collapsed ? `${item.label} (Coming soon)` : undefined}
                aria-disabled="true"
              >
                <Icon className="h-4.5 w-4.5 shrink-0" aria-hidden="true" />
                {!collapsed && (
                  <>
                    <span>{item.label}</span>
                    <span className="ml-auto text-[10px] uppercase tracking-wider text-gray-700">
                      Soon
                    </span>
                  </>
                )}
              </div>
            );
          }

          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={cn(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-150',
                isActive
                  ? 'bg-brand/10 text-brand-light border border-brand/20'
                  : 'text-gray-400 hover:text-gray-200 hover:bg-grid-panel border border-transparent',
                collapsed && 'justify-center px-0',
              )}
              title={collapsed ? item.label : undefined}
              aria-current={isActive ? 'page' : undefined}
            >
              <Icon className="h-4.5 w-4.5 shrink-0" aria-hidden="true" />
              {!collapsed && <span>{item.label}</span>}
            </NavLink>
          );
        })}
      </nav>

      {/* Collapse toggle */}
      <div className="border-t border-grid-border p-2">
        <button
          onClick={() => setCollapsed(!collapsed)}
          className="btn-ghost w-full justify-center"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          type="button"
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" aria-hidden="true" />
          ) : (
            <>
              <ChevronLeft className="h-4 w-4" aria-hidden="true" />
              <span className="text-xs">Collapse</span>
            </>
          )}
        </button>
      </div>
    </aside>
  );
}
