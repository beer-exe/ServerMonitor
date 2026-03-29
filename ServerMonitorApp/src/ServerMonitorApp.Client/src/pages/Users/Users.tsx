import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, Select, Space, Tag, message } from 'antd';
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

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Người dùng</h2>
        <Button 
          type="primary" 
          icon={<PlusOutlined />} 
          onClick={() => openModal()} 
          size="large" 
          className="bg-indigo-600"
        >
          Thêm tài khoản
        </Button>
      </div>

      <Table 
        columns={columns} 
        dataSource={users} 
        rowKey="id" 
        loading={isLoading} 
        pagination={{ pageSize: 10 }} 
        className="shadow-sm border border-slate-200 rounded-lg overflow-hidden" 
      />

      <Modal
        title={editingId ? 'Sửa thông tin tài khoản' : 'Thêm tài khoản mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="mt-4">
          {/* Username chỉ được nhập khi thêm mới */}
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

export default Users;