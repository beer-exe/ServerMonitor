import React, { useEffect, useState, useCallback } from 'react';
import { dashboardService } from '../../services/dashboardService';
import type { DashboardRoomDto, DashboardDeviceDto } from '../../types/dashboard';
import { useSignalR } from '../../hooks/useSignalR';
import DeviceHistoryChart from '../../components/dashboard/DeviceHistoryChart';

import styles from './Dashboard.module.css';

const DeviceCard = React.memo(({ 
  device, 
  onOpenChart 
}: { 
  device: DashboardDeviceDto; 
  onOpenChart: (id: string) => void 
}) => {
  const getStatusClass = (status: string) => {
    switch (status) {
      case 'Normal': return styles.statusNormal;
      case 'Warning': return styles.statusWarning;
      case 'Danger': return styles.statusDanger;
      case 'Offline':
      default: return styles.statusOffline;
    }
  };

  return (
    <div className={`${styles.deviceCard} ${getStatusClass(device.status)}`}>
      <h3 className={styles.deviceName}>{device.name}</h3>
      <p className={styles.deviceMac}>{device.macAddress}</p>
      
      <div className={styles.statsRow}>
        <div className={styles.statBox}>
          <p className={styles.statLabel}>Nhiệt độ</p>
          <p className={styles.tempValue}>
            {device.currentTemperature != null ? device.currentTemperature.toFixed(1) : '-- '}°C
          </p>
        </div>
        <div className={styles.statBox}>
          <p className={styles.statLabel}>Độ ẩm</p>
          <p className={styles.humValue}>
            {device.currentHumidity != null ? device.currentHumidity.toFixed(1) : '-- '}%
          </p>
        </div>
      </div>

      <div className={styles.cardFooter}>
        <span className={styles.updateText}>
          Cập nhật: {new Date(device.lastSeen).toLocaleTimeString()}
        </span>
        <button 
          onClick={() => onOpenChart(device.id)}
          className={styles.btnChart}
        >
          Xem biểu đồ
        </button>
      </div>
    </div>
  );
});

const Dashboard: React.FC = () => {
  const [rooms, setRooms] = useState<DashboardRoomDto[]>([]);
  const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);
  
  const { latestUpdate } = useSignalR(import.meta.env.VITE_API_URL + '/hubs/monitor');

  useEffect(() => {
    dashboardService.getDashboard()
      .then(data => {
        setRooms(Array.isArray(data) ? data : []);
      })
      .catch(console.error);
  }, []);

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
    <div className={styles.container}>
      <h1 className={styles.pageTitle}>Dashboard Giám Sát</h1>

      {rooms.map(room => (
        <div key={room.id} className={styles.roomWrapper}>
          <h2 className={styles.roomTitle}>
            {room.name}
          </h2>
          <p className={styles.roomDesc}>{room.description}</p>
          
          <div className={styles.gridContainer}>
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

      {selectedDeviceId && (
        <div className={styles.modalOverlay}>
          <div className={styles.modalContent}>
            <button 
              onClick={() => setSelectedDeviceId(null)}
              className={styles.btnClose}
            >
              &times;
            </button>
            <h3 className={styles.modalTitle}>Lịch sử thiết bị</h3>
            <DeviceHistoryChart deviceId={selectedDeviceId} />
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;