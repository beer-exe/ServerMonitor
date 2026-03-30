import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Select, Switch, InputNumber, Space, Tag, message, Row, Col, Pagination } from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import { deviceService } from '../../services/deviceService';
import { roomService } from '../../services/roomService';
import type { Device, Room } from '../../types';
import styles from './Devices.module.css';
import { useAuth } from '../../contexts/AuthContext';

const { Option } = Select;

const Devices: React.FC = () => {
  const { user } = useAuth();
  const [devices, setDevices] = useState<Device[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  
  const [form] = Form.useForm();

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [devicesData, roomsData] = await Promise.all([
        deviceService.getDevices(),
        roomService.getRooms()
      ]);
      setDevices(devicesData);
      setRooms(roomsData);
    } catch (error) {
      message.error('Lỗi khi tải dữ liệu thiết bị!');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const openModal = (device?: Device) => {
    if (device) {
      setEditingId(device.id);
      form.setFieldsValue({
        ...device,
        roomId: device.roomId || undefined,
      });
    } else {
      setEditingId(null);
      form.resetFields();
      form.setFieldsValue({
        isActive: true,
        temperatureWarningThreshold: 30,
        temperatureCriticalThreshold: 40,
        humidityWarningThreshold: 60,
        humidityCriticalThreshold: 80
      });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleSubmit = async (values: any) => {
    try {
      const payload = { 
        ...values, 
        roomId: values.roomId || null 
      };

      if (editingId) await deviceService.updateDevice(editingId, payload);
      else await deviceService.createDevice(payload);
      
      message.success('Lưu thiết bị thành công!');
      closeModal();
      fetchData();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Lỗi khi lưu thiết bị!');
    }
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Xóa thiết bị',
      content: 'Bạn có chắc chắn muốn xóa thiết bị này khỏi hệ thống?',
      okText: 'Xóa',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await deviceService.deleteDevice(id);
          message.success('Xóa thiết bị thành công!');
          fetchData();
          setCurrentPage(1);
        } catch (error: any) {
          message.error(error.response?.data?.message || 'Lỗi khi xóa thiết bị!');
        }
      }
    });
  };

  const columns = [
    {
      title: 'Thiết bị',
      dataIndex: 'name',
      key: 'name',
      className: 'font-medium text-slate-900',
    },
    {
      title: 'Phòng Server',
      dataIndex: 'roomName',
      key: 'roomName',
      render: (text: string) => text ? <span className="text-slate-600">{text}</span> : <span className="text-slate-400 italic">Chưa gán</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      align: 'center' as const,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'success' : 'error'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Cảnh báo (Nhiệt/Ẩm)',
      key: 'thresholds',
      align: 'center' as const,
      render: (_: any, record: Device) => (
        <span className="text-sm">
          <span className="text-orange-500 font-medium">{record.temperatureWarningThreshold}°C</span>
          {' / '}
          <span className="text-blue-500 font-medium">{record.humidityWarningThreshold}%</span>
        </span>
      ),
    },
    ...(user?.Role === 'ADMIN' ? [{
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: any, record: Device) => (
        <Space size="small">
          <Button type="text" className="text-indigo-600 hover:text-indigo-800" icon={<EditOutlined />} onClick={() => openModal(record)}>Sửa</Button>
          <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDelete(record.id)}>Xóa</Button>
        </Space>
      ),
    }] : []),
  ];

  const paginatedDevices = devices.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Thiết bị IoT</h2>
        {user?.Role === 'ADMIN' && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()} size="large" className="bg-indigo-600 w-full md:w-auto">
            Thêm thiết bị
          </Button>
        )}
      </div>

      <div className="hidden md:block">
        <Table 
          columns={columns} 
          dataSource={devices} 
          rowKey="id" 
          loading={isLoading}
          pagination={{ 
            current: currentPage,
            pageSize: pageSize,
            onChange: (page) => setCurrentPage(page)
          }}
          className="shadow-sm border border-slate-200 rounded-lg overflow-hidden"
        />
      </div>

      <div className="md:hidden flex flex-col gap-4">
        {isLoading ? (
          <div className="text-center py-8 text-slate-500">Đang tải dữ liệu...</div>
        ) : devices.length > 0 ? (
          <>
            {paginatedDevices.map((device) => (
              <div key={device.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex flex-col gap-3">
                <div className="flex justify-between items-start gap-2">
                  <div className="flex-1 overflow-hidden">
                    <div className="font-bold text-slate-900 text-lg truncate">{device.name}</div>
                    <div className="text-sm mt-1 truncate">
                      {device.roomName ? <span className="text-slate-600">{device.roomName}</span> : <span className="text-slate-400 italic">Chưa gán</span>}
                    </div>
                  </div>
                  <div>
                    <Tag color={device.isActive ? 'success' : 'error'} className="m-0 font-semibold">
                      {device.isActive ? 'Active' : 'Inactive'}
                    </Tag>
                  </div>
                </div>
                
                <div className="bg-slate-50 p-3 rounded border border-slate-100 text-sm flex justify-between items-center">
                   <span className="text-slate-500">Ngưỡng (Nhiệt/Ẩm):</span>
                   <span>
                     <span className="text-orange-500 font-medium">{device.temperatureWarningThreshold}°C</span>
                     {' / '}
                     <span className="text-blue-500 font-medium">{device.humidityWarningThreshold}%</span>
                   </span>
                </div>
                
                {user?.Role === 'ADMIN' && (
                  <div className="flex justify-end items-center pt-3 border-t border-slate-100 mt-1">
                    <Space size="small">
                      <Button type="text" className="text-indigo-600 px-2" icon={<EditOutlined />} onClick={() => openModal(device)} />
                      <Button type="text" danger className="px-2" icon={<DeleteOutlined />} onClick={() => handleDelete(device.id)} />
                    </Space>
                  </div>
                )}
              </div>
            ))}
            
            {devices.length > pageSize && (
              <div className="flex justify-center mt-2 pb-4">
                <Pagination
                  simple
                  current={currentPage}
                  pageSize={pageSize}
                  total={devices.length}
                  onChange={(page) => setCurrentPage(page)}
                />
              </div>
            )}
          </>
        ) : (
          <div className="text-center py-8 text-slate-500 bg-white border border-slate-200 rounded-lg">
            Không có dữ liệu thiết bị.
          </div>
        )}
      </div>

      <Modal
        title={editingId ? 'Sửa Thiết bị' : 'Thêm Thiết bị mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        width={700}
        destroyOnClose
        style={{ top: 20 }}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="mt-4">
          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <Form.Item name="name" label="Tên thiết bị" rules={[{ required: true, message: 'Vui lòng nhập tên thiết bị!' }]}>
                <Input placeholder="Nhập tên thiết bị..." size="large" />
              </Form.Item>
              
              <Form.Item name="roomId" label="Phòng Server">
                <Select placeholder="-- Chưa gán phòng --" size="large" allowClear>
                  {rooms.map(r => <Option key={r.id} value={r.id}>{r.name}</Option>)}
                </Select>
              </Form.Item>

              <Form.Item name="isActive" label="Trạng thái hoạt động" valuePropName="checked">
                <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
              </Form.Item>
            </Col>

            <Col xs={24} md={12}>
              <div className="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <h4 className="font-semibold text-slate-700 text-sm mb-4 border-b pb-2">Ngưỡng cảnh báo</h4>
                
                <Row gutter={16}>
                  <Col xs={24} sm={12}>
                    <Form.Item name="temperatureWarningThreshold" label={<span className="text-orange-600 text-xs font-medium">Nhiệt độ cảnh báo (°C)</span>} rules={[{ required: true }]}>
                      <InputNumber className="w-full" />
                    </Form.Item>
                  </Col>
                  <Col xs={24} sm={12}>
                    <Form.Item name="temperatureCriticalThreshold" label={<span className="text-red-600 text-xs font-medium">Nhiệt độ nguy hiểm (°C)</span>} rules={[{ required: true }]}>
                      <InputNumber className="w-full" />
                    </Form.Item>
                  </Col>
                </Row>

                <Row gutter={16}>
                  <Col xs={24} sm={12}>
                    <Form.Item name="humidityWarningThreshold" label={<span className="text-blue-500 text-xs font-medium">Độ ẩm cảnh báo (%)</span>} rules={[{ required: true }]}>
                      <InputNumber className="w-full" />
                    </Form.Item>
                  </Col>
                  <Col xs={24} sm={12}>
                    <Form.Item name="humidityCriticalThreshold" label={<span className="text-blue-800 text-xs font-medium">Độ ẩm nguy hiểm (%)</span>} rules={[{ required: true }]}>
                      <InputNumber className="w-full" />
                    </Form.Item>
                  </Col>
                </Row>
              </div>
            </Col>
          </Row>

          <Form.Item className="mb-0 text-right mt-6 pt-4 border-t border-slate-200">
            <Space className="w-full justify-end md:w-auto">
              <Button onClick={closeModal}>Hủy</Button>
              <Button type="primary" htmlType="submit" className="bg-indigo-600">
                Lưu thiết bị
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default Devices;