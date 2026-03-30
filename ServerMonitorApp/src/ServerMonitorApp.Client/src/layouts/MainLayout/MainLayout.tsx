import React, { useEffect, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { notification } from 'antd';
import Sidebar from '../Sidebar/Sidebar';
import Header from '../Header/Header';
import { useSignalR } from '../../hooks/useSignalR';
import styles from './MainLayout.module.css';

const MainLayout: React.FC = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
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
        duration: isCritical ? 0 : 5,
        style: {
          borderLeft: `4px solid ${isCritical ? '#ef4444' : '#f59e0b'}`,
          backgroundColor: isCritical ? '#fef2f2' : '#fffbeb',
        }
      });
    }
  }, [latestAlert]);

  const toggleSidebar = () => setIsSidebarOpen(!isSidebarOpen);

  return (
    <div className={styles.layoutWrapper}>
      <Sidebar isOpen={isSidebarOpen} setIsOpen={setIsSidebarOpen} />
      
      <div className={styles.contentWrapper}>
        <Header toggleSidebar={toggleSidebar} />
        <main className={styles.mainArea}>
          <Outlet />
        </main>
      </div>

      {isSidebarOpen && (
        <div 
          /* Đổi z-10 thành z-40 */
          className="fixed inset-0 bg-slate-900/50 z-40 md:hidden transition-opacity"
          onClick={() => setIsSidebarOpen(false)}
        />
      )}
    </div>
  );
};

export default MainLayout;