import React, { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { notification } from 'antd';
import Sidebar from '../Sidebar/Sidebar';
import Header from '../Header/Header';
import { useSignalR } from '../../hooks/useSignalR';
import styles from './MainLayout.module.css';

const MainLayout: React.FC = () => {
  const { latestAlert } = useSignalR(import.meta.env.VITE_API_URL + '/hubs/monitor');

  useEffect(() => {
    if (latestAlert) {
      const isCritical = latestAlert.severity === 'CRITICAL';
      
      notification.open({
        message: `Cảnh báo hệ thống - ${latestAlert.severity}`,
        description: (
          <div>
            <p className="font-semibold">{latestAlert.roomName} - {latestAlert.deviceName}</p>
            <p className="mt-1 text-sm">{latestAlert.message}</p>
          </div>
        ),
        type: isCritical ? 'error' : 'warning',
        placement: 'topRight',
        duration: isCritical ? 0 : 5, // Cảnh báo Critical sẽ không tự tắt
        style: {
          borderLeft: `4px solid ${isCritical ? '#ef4444' : '#f59e0b'}`,
          backgroundColor: isCritical ? '#fef2f2' : '#fffbeb',
        }
      });
    }
  }, [latestAlert]);

  return (
    <div className={styles.layoutWrapper}>
      <Sidebar />
      <div className={styles.contentWrapper}>
        <Header />
        <main className={styles.mainArea}>
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default MainLayout;