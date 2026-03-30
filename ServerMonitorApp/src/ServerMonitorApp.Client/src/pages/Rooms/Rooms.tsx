import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Space, message, Pagination } from 'antd';
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

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

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
          setCurrentPage(1); // Reset về trang đầu khi xóa
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

  const paginatedRooms = rooms.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Phòng Server</h2>
        {user?.Role === 'ADMIN' && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()} size="large" className="bg-indigo-600 w-full md:w-auto">
            Thêm phòng mới
          </Button>
        )}
      </div>

      <div className="hidden md:block">
        <Table 
          columns={columns} 
          dataSource={rooms} 
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
        ) : rooms.length > 0 ? (
          <>
            {paginatedRooms.map((room) => (
              <div key={room.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex flex-col gap-3">
                <div className="flex-1">
                  <div className="font-bold text-slate-900 text-lg">{room.name}</div>
                  <div className="text-sm text-slate-600 mt-1">
                    <span className="font-medium text-slate-500 mr-1">Vị trí:</span>
                    {room.location}
                  </div>
                </div>
                
                {user?.Role === 'ADMIN' && (
                  <div className="flex justify-end items-center pt-3 border-t border-slate-100 mt-1">
                    <Space size="small">
                      <Button type="text" className="text-indigo-600 px-2" icon={<EditOutlined />} onClick={() => openModal(room)}>Sửa</Button>
                      <Button type="text" danger className="px-2" icon={<DeleteOutlined />} onClick={() => handleDelete(room.id)}>Xóa</Button>
                    </Space>
                  </div>
                )}
              </div>
            ))}
            
            {rooms.length > pageSize && (
              <div className="flex justify-center mt-2 pb-4">
                <Pagination
                  simple
                  current={currentPage}
                  pageSize={pageSize}
                  total={rooms.length}
                  onChange={(page) => setCurrentPage(page)}
                />
              </div>
            )}
          </>
        ) : (
          <div className="text-center py-8 text-slate-500 bg-white border border-slate-200 rounded-lg">
            Không có dữ liệu phòng.
          </div>
        )}
      </div>

      <Modal
        title={editingId ? 'Sửa phòng' : 'Thêm phòng mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
        style={{ top: 20 }}
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
            <Space className="w-full justify-end md:w-auto">
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