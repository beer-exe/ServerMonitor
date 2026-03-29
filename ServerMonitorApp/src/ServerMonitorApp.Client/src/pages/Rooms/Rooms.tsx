import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Space, message } from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import { roomService } from '../../services/roomService';
import type { Room } from '../../types';
import styles from './Rooms.module.css';
import { useAuth } from '../../contexts/AuthContext';

const Rooms: React.FC = () => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { user } = useAuth();
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  
  const [form] = Form.useForm();

  const fetchRooms = async () => {
    setIsLoading(true);
    try {
      const data = await roomService.getRooms();
      setRooms(data);
    } catch (error) {
      message.error('Lỗi khi tải danh sách phòng!');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchRooms(); }, []);

  const openModal = (room?: Room) => {
    if (room) {
      setEditingId(room.id);
      form.setFieldsValue({ name: room.name, location: room.location });
    } else {
      setEditingId(null);
      form.resetFields();
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleSubmit = async (values: { name: string, location: string }) => {
    try {
      if (editingId) await roomService.updateRoom(editingId, values);
      else await roomService.createRoom(values);
      
      message.success('Lưu phòng thành công!');
      closeModal();
      fetchRooms();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Lỗi khi lưu dữ liệu!');
    }
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Xác nhận xóa',
      content: 'Bạn có chắc chắn muốn xóa phòng này không?',
      okText: 'Xóa',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await roomService.deleteRoom(id);
          message.success('Xóa phòng thành công!');
          fetchRooms();
        } catch (error: any) {
          message.error(error.response?.data?.message || 'Lỗi khi xóa phòng!');
        }
      }
    });
  };

  const columns = [
    {
      title: 'Tên phòng',
      dataIndex: 'name',
      key: 'name',
      className: 'font-medium text-slate-900',
    },
    {
      title: 'Vị trí',
      dataIndex: 'location',
      key: 'location',
      className: 'text-slate-600',
    },
    ...(user?.Role === 'ADMIN' ? [{
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: any, record: Room) => (
        <Space size="middle">
          <Button type="text" className="text-indigo-600 hover:text-indigo-800" icon={<EditOutlined />} onClick={() => openModal(record)}>Sửa</Button>
          <Button type="text" danger icon={<DeleteOutlined />} onClick={() => handleDelete(record.id)}>Xóa</Button>
        </Space>
      ),
    }] : []),
  ];

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Phòng Server</h2>
        {user?.Role === 'ADMIN' && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()} size="large" className="bg-indigo-600">
            Thêm phòng mới
          </Button>
        )}
      </div>

      <Table 
        columns={columns} 
        dataSource={rooms} 
        rowKey="id" 
        loading={isLoading}
        pagination={{ pageSize: 10 }}
        className="shadow-sm border border-slate-200 rounded-lg overflow-hidden"
      />

      <Modal
        title={editingId ? 'Sửa phòng' : 'Thêm phòng mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="mt-4">
          <Form.Item 
            name="name" 
            label="Tên phòng" 
            rules={[{ required: true, message: 'Vui lòng nhập tên phòng!' }]}
          >
            <Input placeholder="Nhập tên phòng..." size="large" />
          </Form.Item>
          
          <Form.Item 
            name="location" 
            label="Vị trí" 
            rules={[{ required: true, message: 'Vui lòng nhập vị trí phòng!' }]}
          >
            <Input placeholder="Nhập vị trí (VD: Tầng 1, Tòa A)..." size="large" />
          </Form.Item>

          <Form.Item className="mb-0 text-right mt-6">
            <Space>
              <Button onClick={closeModal}>Hủy</Button>
              <Button type="primary" htmlType="submit" className="bg-indigo-600">
                Lưu lại
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default Rooms;