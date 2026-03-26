import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './layouts/MainLayout/MainLayout';
import Login from './pages/Login/Login';
import Rooms from './pages/Rooms/Rooms';
import Devices from './pages/Devices/Devices';
import Dashboard from './pages/Dashboard/Dashboard';
import Alerts from './pages/Alerts/Alerts';

const Unauthorized = () => <div className="text-red-500 font-bold">Bạn không có quyền truy cập trang này.</div>;

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/unauthorized" element={<Unauthorized />} />

          <Route element={<ProtectedRoute />}>
            <Route element={<MainLayout />}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              
              <Route path="/dashboard" element={<Dashboard />} />
              
              <Route path="/rooms" element={<Rooms />} />
              <Route path="/devices" element={<Devices />} />
              
              {/* 2. Thay thế Route /alerts */}
              <Route path="/alerts" element={<Alerts />} />
              
              <Route element={<ProtectedRoute allowedRoles={['ADMIN']} />}>
                <Route path="/users" element={<div>Quản lý Người dùng</div>} />
              </Route>
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;