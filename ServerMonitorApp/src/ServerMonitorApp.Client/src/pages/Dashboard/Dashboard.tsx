import React, { useEffect, useState, useCallback } from 'react';
import { Card, Button, Typography, Tag, Space, Modal, Spin, Empty, Tooltip, Row, Col } from 'antd';
import { LineChartOutlined, DashboardOutlined } from '@ant-design/icons';
import { dashboardService } from '../../services/dashboardService';
import type { DashboardRoomDto, DashboardDeviceDto } from '../../types/dashboard';
import { useSignalR } from '../../hooks/useSignalR';
import DeviceHistoryChart from '../../components/dashboard/DeviceHistoryChart';
import styles from './Dashboard.module.css';

const { Title, Text } = Typography;

const DeviceCard = React.memo(({ 
  device, 
  onOpenChart 
}: { 
  device: DashboardDeviceDto; 
  onOpenChart: (id: string) => void 
}) => {
  const [isFlashing, setIsFlashing] = useState(false);

  useEffect(() => {
    setIsFlashing(true);
    const timer = setTimeout(() => setIsFlashing(false), 1000);
    return () => clearTimeout(timer);
  }, [device.lastSeen]);

  const getStatusConfig = (dev: DashboardDeviceDto) => {
    let status = 'NORMAL';
    
    if (dev.isOffline) {
      status = 'OFFLINE';
    } else {
      const temp = dev.currentTemperature ?? 0;
      const hum = dev.currentHumidity ?? 0;
      
      const isCritical = 
        (dev.criticalTemp != null && temp >= dev.criticalTemp) || 
        (dev.criticalHumidity != null && hum >= dev.criticalHumidity);
        
      const isWarning = 
        (dev.warningTemp != null && temp >= dev.warningTemp) || 
        (dev.warningHumidity != null && hum >= dev.warningHumidity);
        
      if (isCritical) status = 'DANGER';
      else if (isWarning) status = 'WARNING';
    }

    switch (status) {
      case 'NORMAL': return { color: 'success', text: 'Bình thường', bgColor: 'bg-green-50', iconColor: 'text-green-500' };
      case 'WARNING': return { color: 'warning', text: 'Cảnh báo', bgColor: 'bg-yellow-50', iconColor: 'text-yellow-500' };
      case 'DANGER': return { color: 'error', text: 'Nguy hiểm', bgColor: 'bg-red-50', iconColor: 'text-red-500' };
      case 'OFFLINE':
      default: return { color: 'default', text: 'Mất kết nối', bgColor: 'bg-slate-50', iconColor: 'text-slate-400' };
    }
  };

  const statusConfig = getStatusConfig(device);

  return (
    <Card 
      hoverable 
      className={`shadow-sm border-2 transition-all duration-500 h-full flex flex-col ${statusConfig.bgColor} ${
        isFlashing ? 'border-indigo-400 shadow-indigo-200/50 scale-[1.02]' : 'border-transparent'
      }`}
      bodyStyle={{ padding: '16px', display: 'flex', flexDirection: 'column', height: '100%' }}
    >
      <div className="flex justify-between items-start mb-4 gap-2">
        <div className="flex items-center space-x-2 flex-1 overflow-hidden">
          <DashboardOutlined className={`text-xl flex-shrink-0 ${statusConfig.iconColor}`} />
          <Title level={5} className="!mb-0 !mt-0 truncate flex-1 text-base md:text-lg" title={device.name}>
            {device.name}
          </Title>
        </div>
        <Tag color={statusConfig.color} className="m-0 font-medium flex-shrink-0">
          {statusConfig.text}
        </Tag>
      </div>
      
      <Row gutter={16} className="mb-4 flex-1">
        <Col span={12}>
          <div className="bg-white p-3 rounded-lg border border-slate-100 text-center shadow-sm h-full flex flex-col justify-center">
            <Text type="secondary" className="text-[10px] sm:text-xs font-semibold uppercase tracking-wider">Nhiệt độ</Text>
            <div className={`text-lg sm:text-xl font-bold mt-1 ${
              (device.criticalTemp && device.currentTemperature >= device.criticalTemp) ? 'text-red-600' : 
              (device.warningTemp && device.currentTemperature >= device.warningTemp) ? 'text-yellow-600' : 
              'text-slate-800'
            }`}>
              {device.currentTemperature != null ? device.currentTemperature.toFixed(1) : '-- '}°C
            </div>
          </div>
        </Col>
        <Col span={12}>
          <div className="bg-white p-3 rounded-lg border border-slate-100 text-center shadow-sm h-full flex flex-col justify-center">
            <Text type="secondary" className="text-[10px] sm:text-xs font-semibold uppercase tracking-wider">Độ ẩm</Text>
            <div className={`text-lg sm:text-xl font-bold mt-1 ${
               (device.criticalHumidity && device.currentHumidity >= device.criticalHumidity) ? 'text-red-600' : 
               (device.warningHumidity && device.currentHumidity >= device.warningHumidity) ? 'text-yellow-600' : 
               'text-slate-800'
            }`}>
              {device.currentHumidity != null ? device.currentHumidity.toFixed(1) : '-- '}%
            </div>
          </div>
        </Col>
      </Row>

      <div className="flex justify-between items-center pt-3 border-t border-slate-200/60 mt-auto">
        <Tooltip title={device.lastSeen ? new Date(device.lastSeen).toLocaleString('vi-VN') : 'Chưa có dữ liệu'}>
          <Text type="secondary" className="text-xs italic truncate max-w-[120px] sm:max-w-[150px]">
            Cập nhật: {device.lastSeen ? new Date(device.lastSeen).toLocaleTimeString('vi-VN') : '--'}
          </Text>
        </Tooltip>
        <Button 
          type="primary" 
          ghost 
          size="small" 
          icon={<LineChartOutlined />} 
          onClick={() => onOpenChart(device.id)}
        >
          Biểu đồ
        </Button>
      </div>
    </Card>
  );
});

const Dashboard: React.FC = () => {
  const [rooms, setRooms] = useState<DashboardRoomDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);
  
  const { latestUpdate } = useSignalR(import.meta.env.VITE_API_URL + '/hubs/monitor');

  useEffect(() => {
    setIsLoading(true);
    dashboardService.getDashboard()
      .then(data => {
        setRooms(Array.isArray(data) ? data : []);
      })
      .catch(console.error)
      .finally(() => setIsLoading(false));
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
              isOffline: false
            } 
          : device
      )
    })));
  }, [latestUpdate]);

  const handleOpenChart = useCallback((id: string) => {
    setSelectedDeviceId(id);
  }, []);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Spin size="large" tip="Đang tải dữ liệu giám sát..." />
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {/* Thay đổi Header responsive */}
      <div className="mb-4 md:mb-6 flex flex-col md:flex-row justify-between items-start md:items-center">
        <div>
          <Title level={2} className="!mb-1 text-2xl md:text-3xl">Dashboard Giám Sát</Title>
          <Text type="secondary" className="text-sm md:text-base">Theo dõi trạng thái môi trường các phòng Server theo thời gian thực</Text>
        </div>
      </div>

      {rooms.length === 0 ? (
        <div className="bg-white p-6 md:p-10 rounded-xl shadow-sm border border-slate-200 mt-6 text-center">
          <Empty description={<span className="text-slate-500 text-base md:text-lg">Hệ thống chưa có phòng hoặc thiết bị nào được thiết lập.</span>} />
        </div>
      ) : (
        rooms.map(room => (
          <div key={room.id} className="mb-6 md:mb-8 bg-white p-4 md:p-5 rounded-xl shadow-sm border border-slate-200">
            <div className="mb-4 pb-3 border-b border-slate-100">
              <Title level={4} className="!mb-1 text-indigo-800 text-lg md:text-xl">{room.name}</Title>
              {room.description && <Text type="secondary" className="text-sm">{room.description}</Text>}
            </div>
            
            {room.devices.length === 0 ? (
              <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Phòng này chưa có thiết bị" />
            ) : (
              <Row gutter={[16, 16]}>
                {room.devices.map(device => (
                  <Col xs={24} sm={12} lg={8} xl={6} key={device.id}>
                    <DeviceCard 
                      device={device} 
                      onOpenChart={handleOpenChart} 
                    />
                  </Col>
                ))}
              </Row>
            )}
          </div>
        ))
      )}

      <Modal
        title={
          <Space>
            <LineChartOutlined className="text-indigo-600" />
            <span className="font-bold text-sm md:text-base">Lịch sử thông số thiết bị</span>
          </Space>
        }
        open={!!selectedDeviceId}
        onCancel={() => setSelectedDeviceId(null)}
        footer={null}
        width={800}
        destroyOnClose
        style={{ top: 20 }}
        className={styles.chartModal}
      >
        {selectedDeviceId && (
          <div className="pt-2 md:pt-4 overflow-x-auto overflow-y-hidden w-full custom-scrollbar">
            <div className="min-w-[600px] md:min-w-full pb-2 pr-2">
              <DeviceHistoryChart deviceId={selectedDeviceId} />
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default Dashboard;