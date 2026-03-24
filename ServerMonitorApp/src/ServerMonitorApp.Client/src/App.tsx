import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './layouts/MainLayout/MainLayout';
import Login from './pages/Login/Login';

const Dashboard = () => <div className="p-4 bg-white shadow rounded-lg">Trang Dashboard Tổng quan</div>;
const Rooms = () => <div className="p-4 bg-white shadow rounded-lg">Danh sách Phòng Server</div>;
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
              <Route path="/devices" element={<div>Quản lý Thiết bị</div>} />
              <Route path="/alerts" element={<div>Lịch sử Cảnh báo</div>} />
              
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