import { Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from '@/components/layout/AppLayout';
import { OverviewPage } from '@/features/dashboard/pages/OverviewPage';
import { AlertMapPage } from '@/features/map/pages/AlertMapPage';

export function AppRouter() {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<OverviewPage />} />
        <Route path="/alerts/map" element={<AlertMapPage />} />
        {/* Support legacy alias */}
        <Route path="/alert-map" element={<Navigate to="/alerts/map" replace />} />
        {/* Future pages — redirect to overview for now */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppLayout>
  );
}
