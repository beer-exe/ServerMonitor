import React, { useEffect, useState, useCallback } from 'react';
import { dashboardService } from '../services/dashboardService';
import type { DashboardRoomDto, DashboardDeviceDto } from '../types/dashboard';
import { useSignalR } from '../hooks/useSignalR';
import DeviceHistoryChart from '../components/dashboard/DeviceHistoryChart';

// --- COMPONENT THẺ THIẾT BỊ (Được Memoize) ---
const DeviceCard = React.memo(({ 
  device, 
  onOpenChart 
}: { 
  device: DashboardDeviceDto; 
  onOpenChart: (id: string) => void 
}) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Normal': return 'bg-green-100 border-green-500 dark:bg-green-900/30 dark:border-green-500';
      case 'Warning': return 'bg-yellow-100 border-yellow-500 dark:bg-yellow-900/30 dark:border-yellow-500';
      case 'Danger': return 'bg-red-100 border-red-500 dark:bg-red-900/30 dark:border-red-500';
      case 'Offline':
      default: return 'bg-gray-100 border-gray-400 dark:bg-gray-800 dark:border-gray-600 opacity-70';
    }
  };

  return (
    <div className={`p-4 border-l-4 rounded-r-lg shadow-sm transition-all duration-300 ${getStatusColor(device.status)}`}>
      <h3 className="font-bold text-lg dark:text-gray-100">{device.name}</h3>
      <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">{device.macAddress}</p>
      
      <div className="flex justify-between items-center mb-4">
        <div className="text-center">
          <p className="text-xs uppercase text-gray-500 dark:text-gray-400">Nhiệt độ</p>
          <p className="text-2xl font-bold text-red-600 dark:text-red-400">
            {device.currentTemperature != null ? device.currentTemperature.toFixed(1) : '-- '}°C
          </p>
        </div>
        <div className="text-center">
          <p className="text-xs uppercase text-gray-500 dark:text-gray-400">Độ ẩm</p>
          <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">
            {device.currentHumidity != null ? device.currentHumidity.toFixed(1) : '-- '}%
          </p>
        </div>
      </div>

      <div className="flex justify-between items-center mt-2">
        <span className="text-xs text-gray-500 dark:text-gray-400">
          Cập nhật: {new Date(device.lastSeen).toLocaleTimeString()}
        </span>
        <button 
          onClick={() => onOpenChart(device.id)}
          className="text-sm px-3 py-1 bg-indigo-600 text-white rounded hover:bg-indigo-700 transition"
        >
          Xem biểu đồ
        </button>
      </div>
    </div>
  );
});

// --- COMPONENT DASHBOARD CHÍNH ---
const Dashboard: React.FC = () => {
  const [rooms, setRooms] = useState<DashboardRoomDto[]>([]);
  const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);
  
  // Thay đổi URL /hubs/monitor phù hợp với domain backend thực tế
  const { latestUpdate } = useSignalR(import.meta.env.VITE_API_URL + '/hubs/monitor');

  useEffect(() => {
    dashboardService.getDashboard()
      .then(data => {
        setRooms(Array.isArray(data) ? data : []);
      })
      .catch(console.error);
  }, []);

  // Xử lý cập nhật Real-time
  useEffect(() => {
    if (!latestUpdate) return;

    setRooms(prevRooms => prevRooms.map(room => ({
      ...room,
      devices: room.devices.map(device => 
        device.id === latestUpdate.deviceId 
          ? { 
              ...device, 
              currentTemperature: latestUpdate.temperature,
              currentHumidity: latestUpdate.humidity,
              lastSeen: latestUpdate.timestamp,
              status: latestUpdate.status
            } 
          : device
      )
    })));
  }, [latestUpdate]);

  const handleOpenChart = useCallback((id: string) => {
    setSelectedDeviceId(id);
  }, []);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      <h1 className="text-3xl font-bold text-gray-800 dark:text-white mb-8">Dashboard Giám Sát</h1>

      {rooms.map(room => (
        <div key={room.id} className="mb-10">
          <h2 className="text-2xl font-semibold text-gray-700 dark:text-gray-200 mb-2 border-b border-gray-300 dark:border-gray-700 pb-2">
            {room.name}
          </h2>
          <p className="text-gray-500 dark:text-gray-400 mb-4">{room.description}</p>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {room.devices.map(device => (
              <DeviceCard 
                key={device.id} 
                device={device} 
                onOpenChart={handleOpenChart} 
              />
            ))}
          </div>
        </div>
      ))}

      {/* Modal Biểu đồ */}
      {selectedDeviceId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg w-full max-w-4xl p-6 relative shadow-2xl">
            <button 
              onClick={() => setSelectedDeviceId(null)}
              className="absolute top-4 right-4 text-gray-500 hover:text-gray-800 dark:text-gray-400 dark:hover:text-white text-xl font-bold"
            >
              &times;
            </button>
            <h3 className="text-xl font-bold mb-4 dark:text-white">Lịch sử thiết bị</h3>
            <DeviceHistoryChart deviceId={selectedDeviceId} />
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;