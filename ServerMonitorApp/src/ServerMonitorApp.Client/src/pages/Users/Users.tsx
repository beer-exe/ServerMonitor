import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Select, Space, Tag, message, Pagination } from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import { userService } from '../../services/userService';
import type { UserItem } from '../../types';
import styles from './Users.module.css';

const { Option } = Select;

const Users: React.FC = () => {
  const [users, setUsers] = useState<UserItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form] = Form.useForm();

  // Thêm state quản lý phân trang đồng bộ cho cả PC và Mobile
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  const fetchUsers = async () => {
    setIsLoading(true);
    try {
      const data = await userService.getUsers();
      setUsers(data);
    } catch (error) {
      message.error('Lỗi khi tải danh sách nhân viên!');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const openModal = (user?: UserItem) => {
    if (user) {
      setEditingId(user.id);
      form.setFieldsValue({
        username: user.username,
        email: user.email,
        role: user.role,
      });
    } else {
      setEditingId(null);
      form.resetFields();
      form.setFieldsValue({ role: 'USER' });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleSubmit = async (values: any) => {
    try {
      if (editingId) {
        const updatePayload = { email: values.email, role: values.role };
        await userService.updateUser(editingId, updatePayload);
        message.success('Cập nhật tài khoản thành công!');
      } else {
        await userService.createUser(values);
        message.success('Thêm tài khoản mới thành công!');
      }
      closeModal();
      fetchUsers();
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Lỗi khi lưu dữ liệu!');
    }
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Xác nhận xóa',
      content: 'Bạn có chắc chắn muốn xóa tài khoản này không? Hành động này không thể hoàn tác.',
      okText: 'Xóa',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await userService.deleteUser(id);
          message.success('Xóa tài khoản thành công!');
          fetchUsers();
          // Reset về trang 1 nếu xóa hết user ở trang hiện tại
          setCurrentPage(1); 
        } catch (error: any) {
          message.error(error.response?.data?.message || 'Lỗi khi xóa tài khoản!');
        }
      }
    });
  };

  const columns = [
    {
      title: 'Username',
      dataIndex: 'username',
      key: 'username',
      className: 'font-medium text-slate-900',
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      className: 'text-slate-600',
    },
    {
      title: 'Vai trò',
      dataIndex: 'role',
      key: 'role',
      align: 'center' as const,
      render: (role: string) => (
        <Tag color={role === 'ADMIN' ? 'purple' : 'blue'} className="font-semibold">
          {role}
        </Tag>
      ),
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => (date ? new Date(date).toLocaleDateString('vi-VN') : '--'),
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: any, record: UserItem) => (
        <Space size="middle">
          <Button 
            type="text" 
            className="text-indigo-600 hover:text-indigo-800" 
            icon={<EditOutlined />} 
            onClick={() => openModal(record)}
          >
            Sửa
          </Button>
          <Button 
            type="text" 
            danger 
            icon={<DeleteOutlined />} 
            onClick={() => handleDelete(record.id)}
          >
            Xóa
          </Button>
        </Space>
      ),
    },
  ];

  // Tính toán dữ liệu hiển thị cho Mobile dựa theo Pagination
  const paginatedUsers = users.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Người dùng</h2>
        <Button 
          type="primary" 
          icon={<PlusOutlined />} 
          onClick={() => openModal()} 
          size="large" 
          className="bg-indigo-600 w-full md:w-auto"
        >
          Thêm tài khoản
        </Button>
      </div>

      {/* Giao diện PC (Giữ nguyên Table 100%) */}
      <div className="hidden md:block">
        <Table 
          columns={columns} 
          dataSource={users} 
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

      {/* Giao diện Mobile (Có phân trang đơn giản) */}
      <div className="md:hidden flex flex-col gap-4">
        {isLoading ? (
          <div className="text-center py-8 text-slate-500">Đang tải dữ liệu...</div>
        ) : users.length > 0 ? (
          <>
            {paginatedUsers.map((user) => (
              <div key={user.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex flex-col gap-3">
                <div className="flex justify-between items-start gap-2">
                  <div className="flex-1 overflow-hidden">
                    <div className="font-bold text-slate-900 text-lg truncate">{user.username}</div>
                    <div className="text-sm text-slate-500 truncate">{user.email}</div>
                  </div>
                  <div>
                    <Tag color={user.role === 'ADMIN' ? 'purple' : 'blue'} className="m-0 font-semibold">
                      {user.role}
                    </Tag>
                  </div>
                </div>
                
                <div className="flex justify-between items-center pt-3 border-t border-slate-100 mt-1">
                  <div className="text-sm text-slate-500">
                    Ngày tạo: <span className="text-slate-800 font-medium">{user.createdAt ? new Date(user.createdAt).toLocaleDateString('vi-VN') : '--'}</span>
                  </div>
                  <Space size="small">
                    <Button 
                      type="text" 
                      className="text-indigo-600 px-2" 
                      icon={<EditOutlined />} 
                      onClick={() => openModal(user)}
                    />
                    <Button 
                      type="text" 
                      danger 
                      className="px-2"
                      icon={<DeleteOutlined />} 
                      onClick={() => handleDelete(user.id)}
                    />
                  </Space>
                </div>
              </div>
            ))}
            
            {/* Component Phân trang dành riêng cho Mobile (Dùng giao diện simple) */}
            {users.length > pageSize && (
              <div className="flex justify-center mt-2 pb-4">
                <Pagination
                  simple
                  current={currentPage}
                  pageSize={pageSize}
                  total={users.length}
                  onChange={(page) => setCurrentPage(page)}
                />
              </div>
            )}
          </>
        ) : (
          <div className="text-center py-8 text-slate-500 bg-white border border-slate-200 rounded-lg">
            Không có dữ liệu người dùng.
          </div>
        )}
      </div>

      <Modal
        title={editingId ? 'Sửa thông tin tài khoản' : 'Thêm tài khoản mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
        style={{ top: 20 }}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="mt-4">
          {!editingId && (
            <Form.Item 
              name="username" 
              label="Tên đăng nhập (Username)" 
              rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập!' }]}
            >
              <Input placeholder="Nhập username..." size="large" />
            </Form.Item>
          )}

          {!editingId && (
            <Form.Item 
              name="password" 
              label="Mật khẩu" 
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }, { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự!' }]}
            >
              <Input.Password placeholder="Nhập mật khẩu..." size="large" />
            </Form.Item>
          )}

          <Form.Item 
            name="email" 
            label="Địa chỉ Email" 
            rules={[
              { required: true, message: 'Vui lòng nhập email!' },
              { type: 'email', message: 'Định dạng email không hợp lệ!' }
            ]}
          >
            <Input placeholder="Nhập địa chỉ email..." size="large" />
          </Form.Item>

          <Form.Item 
            name="role" 
            label="Vai trò (Role)" 
            rules={[{ required: true, message: 'Vui lòng chọn vai trò!' }]}
          >
            <Select placeholder="Chọn vai trò" size="large">
              <Option value="USER">USER (Giám sát viên)</Option>
              <Option value="ADMIN">ADMIN (Quản trị viên)</Option>
            </Select>
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

export default Users;