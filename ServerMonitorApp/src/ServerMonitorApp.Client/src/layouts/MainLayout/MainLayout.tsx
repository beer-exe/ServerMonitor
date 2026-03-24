import React from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '../Sidebar/Sidebar';
import Header from '../Header/Header';
import styles from './MainLayout.module.css';

const MainLayout: React.FC = () => {
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