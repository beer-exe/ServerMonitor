import React, { useState, useEffect, useMemo } from 'react';
import { Table, Button, Modal, Form, Select, Space, Tag, message, Checkbox, Input } from 'antd';
import { EditOutlined, DeleteOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import { accessService } from '../../services/accessService';
import { userService } from '../../services/userService';
import { roomService } from '../../services/roomService';
import type { UserItem, Room, UserRoomAccessDto } from '../../types';
import styles from './AccessControl.module.css';

const { Option } = Select;

const AccessControl: React.FC = () => {
  const [users, setUsers] = useState<UserItem[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  
  const [accessList, setAccessList] = useState<UserRoomAccessDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  const [editingRecord, setEditingRecord] = useState<UserRoomAccessDto | null>(null);
  const [form] = Form.useForm();

  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        const [usersData, roomsData] = await Promise.all([
          userService.getUsers(),
          roomService.getRooms()
        ]);
        setUsers(usersData);
        setRooms(roomsData);
      } catch (error) {
        message.error('Lỗi khi tải dữ liệu hệ thống!');
      }
    };
    fetchInitialData();
  }, []);

  const fetchAccessList = async (userId: string) => {
    setIsLoading(true);
    try {
      const data = await accessService.getRoomsByUser(userId);
      setAccessList(data);
    } catch (error) {
      message.error('Lỗi khi tải danh sách quyền của nhân viên!');
      setAccessList([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleUserChange = (value: string) => {
    setSelectedUserId(value);
    fetchAccessList(value);
  };

  const availableRooms = useMemo(() => {
    return rooms.filter(room => !accessList.some(access => access.roomId === room.id));
  }, [rooms, accessList]);

  const openModal = (record?: UserRoomAccessDto) => {
    if (record) {
      setEditingRecord(record);
      form.setFieldsValue({
        roomId: record.roomId,
        receiveAlerts: record.receiveAlerts
      });
    } else {
      setEditingRecord(null);
      form.resetFields();
      form.setFieldsValue({ receiveAlerts: true });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleSubmit = async (values: any) => {
    if (!selectedUserId) return;

    try {
      const payload = {
        userId: selectedUserId,
        roomId: values.roomId,
        receiveAlerts: values.receiveAlerts
      };

      if (editingRecord) {
        await accessService.updateAccess(payload);
        message.success('Cập nhật quyền thành công!');
      } else {
        await accessService.assignAccess(payload);
        message.success('Cấp quyền truy cập phòng thành công!');
      }
      closeModal();
      fetchAccessList(selectedUserId);
    } catch (error: any) {
      message.error(error.response?.data?.message || 'Lỗi khi lưu dữ liệu phân quyền!');
    }
  };

  const handleRevoke = (roomId: string) => {
    if (!selectedUserId) return;

    Modal.confirm({
      title: 'Xác nhận thu hồi quyền',
      content: 'Bạn có chắc chắn muốn thu hồi quyền giám sát phòng này của nhân viên?',
      okText: 'Thu hồi',
      okType: 'danger',
      cancelText: 'Hủy',
      onOk: async () => {
        try {
          await accessService.revokeAccess(selectedUserId, roomId);
          message.success('Thu hồi quyền thành công!');
          fetchAccessList(selectedUserId);
        } catch (error: any) {
          message.error(error.response?.data?.message || 'Lỗi khi thu hồi quyền!');
        }
      }
    });
  };

const columns = [
    {
      title: 'Tên Phòng Máy',
      dataIndex: 'roomName',
      key: 'roomName',
      className: 'font-medium text-slate-900',
    },
    {
      title: 'Nhận cảnh báo',
      dataIndex: 'receiveAlerts',
      key: 'receiveAlerts',
      align: 'center' as const,
      render: (receive: boolean) => (
        <Tag color={receive ? 'green' : 'red'} className="font-semibold">
          {receive ? 'Đang bật' : 'Đã tắt'}
        </Tag>
      ),
    },
    {
      title: 'Ngày cập nhật',
      dataIndex: 'updatedAt',
      key: 'updatedAt',
      render: (date: string) => (date ? new Date(date).toLocaleDateString('vi-VN') : '--'),
    },
    {
      title: 'Thao tác',
      key: 'action',
      align: 'right' as const,
      render: (_: any, record: UserRoomAccessDto) => (
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
            onClick={() => handleRevoke(record.roomId)}
          >
            Thu hồi
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Phân quyền Giám sát</h2>
        {selectedUserId && (
          <Button 
            type="primary" 
            icon={<SafetyCertificateOutlined />} 
            onClick={() => openModal()} 
            size="large" 
            className="bg-emerald-600 hover:bg-emerald-700 w-full md:w-auto"
          >
            Cấp quyền phòng mới
          </Button>
        )}
      </div>

      <div className={styles.selectUserContainer}>
        <span className={styles.selectLabel}>Chọn Nhân viên:</span>
        <Select
          showSearch
          placeholder="-- Vui lòng chọn nhân viên để quản lý quyền --"
          optionFilterProp="children"
          onChange={handleUserChange}
          className="w-full md:w-96"
          size="large"
          value={selectedUserId}
        >
          {users.map((user) => (
            <Option key={user.id} value={user.id}>
              {user.username} - {user.email}
            </Option>
          ))}
        </Select>
      </div>

      {selectedUserId ? (
        <>
          <div className="hidden md:block">
            <Table 
              columns={columns} 
              dataSource={accessList} 
              rowKey="roomId" 
              loading={isLoading} 
              pagination={false} 
              locale={{ emptyText: 'Nhân viên này chưa được cấp quyền giám sát phòng nào.' }}
              className="shadow-sm border border-slate-200 rounded-lg overflow-hidden" 
            />
          </div>

          <div className="md:hidden flex flex-col gap-4">
            {isLoading ? (
               <div className="text-center py-8 text-slate-500">Đang tải dữ liệu...</div>
            ) : accessList.length > 0 ? (
              accessList.map((record) => (
                <div key={record.roomId} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex flex-col gap-3">
                  <div className="flex justify-between items-start gap-2">
                    <div>
                      <div className="text-xs text-slate-500 mb-1">Tên Phòng Máy</div>
                      <div className="font-semibold text-slate-900 text-base">{record.roomName}</div>
                    </div>
                    <div>
                      <Tag color={record.receiveAlerts ? 'green' : 'red'} className="m-0 font-semibold">
                        {record.receiveAlerts ? 'Đang bật' : 'Đã tắt'}
                      </Tag>
                    </div>
                  </div>
                  
                  <div className="flex justify-between items-center pt-3 border-t border-slate-100 mt-1">
                    <div className="text-sm text-slate-500">
                      Cập nhật: <span className="text-slate-800 font-medium">{record.updatedAt ? new Date(record.updatedAt).toLocaleDateString('vi-VN') : '--'}</span>
                    </div>
                    <Space size="small">
                      <Button 
                        type="text" 
                        className="text-indigo-600 px-2" 
                        icon={<EditOutlined />} 
                        onClick={() => openModal(record)}
                      />
                      <Button 
                        type="text" 
                        danger 
                        className="px-2"
                        icon={<DeleteOutlined />} 
                        onClick={() => handleRevoke(record.roomId)}
                      />
                    </Space>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-slate-500 bg-white border border-slate-200 rounded-lg">
                Nhân viên này chưa được cấp quyền giám sát phòng nào.
              </div>
            )}
          </div>
        </>
      ) : (
        <div className="text-center py-12 text-slate-500 bg-white border border-slate-200 rounded-lg px-4">
          Vui lòng chọn một nhân viên từ danh sách để xem và quản lý quyền truy cập.
        </div>
      )}

      <Modal
        title={editingRecord ? 'Chỉnh sửa quyền truy cập' : 'Cấp quyền truy cập mới'}
        open={isModalOpen}
        onCancel={closeModal}
        footer={null}
        destroyOnClose
        style={{ top: 20 }}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} className="mt-4">
          {editingRecord ? (
            <Form.Item label="Phòng Máy" className="mb-4">
              <div className="p-3 bg-slate-100 border border-slate-200 rounded-lg text-slate-700 font-medium break-words">
                {editingRecord.roomName}
              </div>
              <Form.Item name="roomId" hidden><Input /></Form.Item> 
            </Form.Item>
          ) : (
            <Form.Item 
              name="roomId" 
              label="Chọn Phòng Máy" 
              rules={[{ required: true, message: 'Vui lòng chọn phòng máy!' }]}
            >
              <Select placeholder="Chọn phòng..." size="large" showSearch optionFilterProp="children" className="w-full">
                {availableRooms.map((room) => (
                  <Option key={room.id} value={room.id}>
                    {room.name}
                  </Option>
                ))}
              </Select>
            </Form.Item>
          )}

          <Form.Item name="receiveAlerts" valuePropName="checked">
            <Checkbox className="text-slate-700 font-medium text-base">
              Nhận thông báo khẩn cấp (Receive Alerts)
            </Checkbox>
          </Form.Item>

          <Form.Item className="mb-0 text-right mt-6">
            <Space className="w-full justify-end md:w-auto">
              <Button onClick={closeModal}>Hủy</Button>
              <Button type="primary" htmlType="submit" className="bg-emerald-600 hover:bg-emerald-700">
                Lưu quyền
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};
export default AccessControl;