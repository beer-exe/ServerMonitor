import React, { useState, useEffect, useCallback } from 'react';
import { Table, Select, Modal, Form, Input, Button, Tag, Space, message, Typography } from 'antd';
import { CheckCircleOutlined } from '@ant-design/icons';
import { alertService } from '../../services/alertService';
import type { AlertDto } from '../../types';
import styles from './Alerts.module.css';

const { Option } = Select;
const { Text } = Typography;

const Alerts: React.FC = () => {
  const [alerts, setAlerts] = useState<AlertDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(15);
  const [totalRecords, setTotalRecords] = useState(0);
  const [severityFilter, setSeverityFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [resolvingAlertId, setResolvingAlertId] = useState<number | null>(null);
  const [form] = Form.useForm();

  const fetchAlerts = useCallback(async () => {
    setIsLoading(true);
    try {
      const isResolved = statusFilter === 'resolved' ? true : statusFilter === 'unresolved' ? false : undefined;
      const severity = severityFilter !== '' ? severityFilter : undefined;

      const response = await alertService.getAlerts({
        pageNumber,
        pageSize,
        severity,
        isResolved
      });

      const items = response.data || (response as any).Data || [];
      const total = response.totalRecords || (response as any).TotalRecords || 0;

      setAlerts(items);
      setTotalRecords(total);
    } catch (error) {
      message.error('Lỗi khi tải danh sách cảnh báo!');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize, severityFilter, statusFilter]);

  useEffect(() => {
    fetchAlerts();
  }, [fetchAlerts]);

  const handleTableChange = (pagination: any) => {
    setPageNumber(pagination.current);
  };

  const openResolveModal = (id: number) => {
    setResolvingAlertId(id);
    form.resetFields();
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setResolvingAlertId(null);
    form.resetFields();
  };

  const handleResolveSubmit = async (values: { resolutionNote: string }) => {
    if (!resolvingAlertId) return;

    try {
      await alertService.resolveAlert(resolvingAlertId, values.resolutionNote);
      message.success('Đã xử lý cảnh báo thành công!');
      closeModal();
      fetchAlerts(); // Tải lại danh sách
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Có lỗi xảy ra khi xử lý cảnh báo!');
    }
  };

  const columns = [
    {
      title: 'Thời gian',
      key: 'time',
      render: (_: any, record: AlertDto) => (
        <div className="flex flex-col">
          <Text className="font-medium text-slate-900">{new Date(record.createdAt).toLocaleDateString('vi-VN')}</Text>
          <Text type="secondary" className="text-xs">{new Date(record.createdAt).toLocaleTimeString('vi-VN')}</Text>
        </div>
      ),
    },
    {
      title: 'Phòng / Thiết bị',
      key: 'location',
      render: (_: any, record: AlertDto) => (
        <div className="flex flex-col">
          <Text className="font-medium text-slate-900">{record.roomName || 'Hệ thống'}</Text>
          <Text type="secondary" className="text-xs">{record.deviceName || 'Không xác định'}</Text>
        </div>
      ),
    },
    {
      title: 'Mức độ',
      dataIndex: 'severity',
      key: 'severity',
      align: 'center' as const,
      render: (severity: string) => {
        let color = 'default';
        if (severity === 'CRITICAL') color = 'error';
        if (severity === 'WARNING') color = 'warning';
        return <Tag color={color} className="font-bold">{severity}</Tag>;
      },
    },
    {
      title: 'Nội dung',
      dataIndex: 'message',
      key: 'message',
      render: (text: string) => <div className="max-w-md break-words text-sm text-slate-700">{text}</div>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isResolved',
      key: 'isResolved',
      align: 'center' as const,
      render: (isResolved: boolean) => (
        <Tag color={isResolved ? 'success' : 'error'}>
          {isResolved ? 'Đã xử lý' : 'Chưa xử lý'}
        </Tag>
      ),
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: any, record: AlertDto) => (
        !record.isResolved ? (
          <Button 
            type="primary" 
            ghost 
            size="small" 
            icon={<CheckCircleOutlined />}
            onClick={() => openResolveModal(record.id)}
          >
            Xử lý
          </Button>
        ) : null
      ),
    },
  ];

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Lịch sử Cảnh báo & Sự cố</h2>
      </div>

      <div className="bg-white p-4 rounded-xl shadow-sm border border-slate-200 flex flex-wrap gap-4 items-end mb-6">
        <div className="flex flex-col space-y-1">
          <label className="text-sm font-medium text-slate-600">Trạng thái</label>
          <Select 
            value={statusFilter} 
            onChange={(val) => { setStatusFilter(val); setPageNumber(1); }}
            className="w-40"
            size="large"
          >
            <Option value="">Tất cả</Option>
            <Option value="unresolved">Chưa xử lý</Option>
            <Option value="resolved">Đã xử lý</Option>
          </Select>
        </div>

        <div className="flex flex-col space-y-1">
          <label className="text-sm font-medium text-slate-600">Mức độ</label>
          <Select 
            value={severityFilter} 
            onChange={(val) => { setSeverityFilter(val); setPageNumber(1); }}
            className="w-48"
            size="large"
          >
            <Option value="">Tất cả</Option>
            <Option value="CRITICAL">Critical (Nguy hiểm)</Option>
            <Option value="WARNING">Warning (Cảnh báo)</Option>
            <Option value="OFFLINE">Offline (Mất kết nối)</Option>
          </Select>
        </div>
      </div>

      <Table 
        columns={columns} 
        dataSource={alerts} 
        rowKey="id" 
        loading={isLoading}
        onChange={handleTableChange}
        pagination={{ 
          current: pageNumber,
          pageSize: pageSize,
          total: totalRecords,
          showSizeChanger: false, // Tắt chức năng đổi số lượng / trang nếu API không hỗ trợ động
          showTotal: (total, range) => `Hiển thị ${range[0]}-${range[1]} trên tổng số ${total} cảnh báo`
        }}
        rowClassName={(record) => record.isResolved ? 'bg-slate-50 opacity-80' : ''}
        className="shadow-sm border border-slate-200 rounded-lg overflow-hidden bg-white"
        scroll={{ x: 'max-content' }}
      />

      <Modal
        title="Xử lý Sự cố"
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleResolveSubmit} className="mt-4">
          <Form.Item 
            name="resolutionNote" 
            label="Ghi chú khắc phục"
            rules={[{ required: true, message: 'Vui lòng nhập ghi chú khắc phục!' }]}
          >
            <Input.TextArea 
              rows={4} 
              placeholder="Mô tả các bước đã thực hiện để khắc phục sự cố..." 
            />
          </Form.Item>
          
          <Form.Item className="mb-0 text-right mt-6">
            <Space>
              <Button onClick={closeModal}>Hủy</Button>
              <Button type="primary" htmlType="submit" className="bg-indigo-600">
                Xác nhận xử lý
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default Alerts;