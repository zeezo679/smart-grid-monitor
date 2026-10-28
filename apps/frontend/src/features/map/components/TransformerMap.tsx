import { useEffect, useMemo } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import L from 'leaflet';
import type { MapMarker, ThreatType } from '../types';

interface TransformerMapProps {
  markers: MapMarker[];
  selectedMarker: MapMarker | null;
  activeTypeFilter?: ThreatType | '';
  onSelectMarker: (marker: MapMarker) => void;
}

// Center of Cairo / Giza distribution network
const DEFAULT_CENTER: [number, number] = [30.0444, 31.2357];
const DEFAULT_ZOOM = 12;

/**
 * Determines marker badge style based on alert profile and active threat filter.
 */
function getMarkerStyle(marker: MapMarker, activeTypeFilter?: ThreatType | '') {
  // If a specific threat filter is active and the transformer has alerts of that type, emphasize it
  if (activeTypeFilter === 'theft' && marker.theftAlertCount > 0) {
    return { bg: '#EF4444', ring: '#F87171', label: marker.theftAlertCount };
  }
  if (activeTypeFilter === 'overload' && marker.overloadAlertCount > 0) {
    return { bg: '#F59E0B', ring: '#FBBF24', label: marker.overloadAlertCount };
  }
  if (activeTypeFilter === 'line_drop' && marker.lineDropAlertCount > 0) {
    return { bg: '#06B6D4', ring: '#22D3EE', label: marker.lineDropAlertCount };
  }

  // Dominant threat priority: Theft > Overload > Line Drop > Healthy
  if (marker.theftAlertCount > 0) {
    return { bg: '#EF4444', ring: '#F87171', label: marker.totalAlertCount };
  }
  if (marker.overloadAlertCount > 0) {
    return { bg: '#F59E0B', ring: '#FBBF24', label: marker.totalAlertCount };
  }
  if (marker.lineDropAlertCount > 0) {
    return { bg: '#06B6D4', ring: '#22D3EE', label: marker.totalAlertCount };
  }

  // No alerts detected
  return { bg: '#10B981', ring: '#34D399', label: 0 };
}

/**
 * Creates custom Leaflet DivIcon for a transformer marker.
 */
function createCustomMarkerIcon(marker: MapMarker, isSelected: boolean, activeTypeFilter?: ThreatType | '') {
  const { bg, ring, label } = getMarkerStyle(marker, activeTypeFilter);
  const size = isSelected ? 34 : 26;
  const pulseClass = isSelected ? 'ring-4 ring-white ring-offset-2 ring-offset-grid-bg animate-pulse' : '';

  const html = `
    <div style="display:flex;align-items:center;justify-content:center;width:${size}px;height:${size}px;border-radius:50%;background-color:${bg};border:2px solid ${ring};box-shadow:0 2px 8px rgba(0,0,0,0.5);cursor:pointer;color:white;font-weight:700;font-size:${isSelected ? '12px' : '10px'};font-family:ui-monospace,monospace;" class="${pulseClass}">
      ${label > 0 ? label : ''}
    </div>
  `;

  return L.divIcon({
    className: 'custom-grid-marker',
    html,
    iconSize: [size, size],
    iconAnchor: [size / 2, size / 2],
    popupAnchor: [0, -size / 2],
  });
}

/**
 * Map helper component to auto-pan when a marker is selected
 * or adjust bounds when markers change.
 */
function MapViewUpdater({
  selectedMarker,
  markers,
}: {
  selectedMarker: MapMarker | null;
  markers: MapMarker[];
}) {
  const map = useMap();

  useEffect(() => {
    if (selectedMarker) {
      map.panTo([selectedMarker.latitude, selectedMarker.longitude], {
        animate: true,
        duration: 0.5,
      });
    }
  }, [selectedMarker, map]);

  useEffect(() => {
    if (!selectedMarker && markers.length > 0) {
      const bounds = L.latLngBounds(markers.map((m) => [m.latitude, m.longitude]));
      if (bounds.isValid()) {
        map.fitBounds(bounds, { padding: [40, 40], maxZoom: 14 });
      }
    }
  }, [markers, map, selectedMarker]);

  return null;
}

export function TransformerMap({
  markers,
  selectedMarker,
  activeTypeFilter,
  onSelectMarker,
}: TransformerMapProps) {
  // Pre-generate icons to avoid recreating on each render
  const markerIcons = useMemo(() => {
    const map = new Map<string, L.DivIcon>();
    markers.forEach((m) => {
      const isSelected = selectedMarker?.transformerId === m.transformerId;
      map.set(m.transformerId, createCustomMarkerIcon(m, isSelected, activeTypeFilter));
    });
    return map;
  }, [markers, selectedMarker, activeTypeFilter]);

  return (
    <div className="relative h-[480px] sm:h-[560px] w-full rounded-lg overflow-hidden border border-grid-border shadow-inner">
      <MapContainer
        center={DEFAULT_CENTER}
        zoom={DEFAULT_ZOOM}
        className="h-full w-full bg-grid-bg"
        scrollWheelZoom={true}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener noreferrer">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          className="map-tiles"
        />

        <MapViewUpdater selectedMarker={selectedMarker} markers={markers} />

        {markers.map((marker) => {
          const icon = markerIcons.get(marker.transformerId);
          if (!icon) return null;

          return (
            <Marker
              key={marker.transformerId}
              position={[marker.latitude, marker.longitude]}
              icon={icon}
              eventHandlers={{
                click: () => onSelectMarker(marker),
              }}
            >
              <Popup className="custom-leaflet-popup">
                <div className="p-1 text-xs">
                  <div className="font-mono font-semibold text-gray-900">
                    {marker.transformerId}
                  </div>
                  <div className="text-gray-600 text-[11px]">
                    {marker.district} • {marker.capacityKw} kW
                  </div>
                  <div className="mt-1 font-medium text-gray-800">
                    {marker.totalAlertCount > 0 ? (
                      <span className="text-red-600 font-semibold">
                        {marker.totalAlertCount} alert window{marker.totalAlertCount !== 1 ? 's' : ''}
                      </span>
                    ) : (
                      <span className="text-emerald-600">No alerts</span>
                    )}
                  </div>
                </div>
              </Popup>
            </Marker>
          );
        })}
      </MapContainer>
    </div>
  );
}
