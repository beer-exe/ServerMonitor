import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import styles from './Header.module.css';

const Header: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <header className={styles.header}>
      <div className={styles.title}>
      </div>
      <div className={styles.userInfo}>
        <span className={styles.greeting}>
          Xin chào, <span className={styles.userName}>{user?.Username}</span>
        </span>
        <button onClick={logout} className={styles.logoutBtn}>
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="2">
            <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
          </svg>
          Đăng xuất
        </button>
      </div>
    </header>
  );
};

export default Header;