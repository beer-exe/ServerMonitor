import React, { useState, useEffect } from 'react';
import { deviceService } from '../../services/deviceService';
import { roomService } from '../../services/roomService';
import type { Device, Room } from '../../types';
import styles from './Devices.module.css';

const defaultFormState = {
  name: '', macAddress: '', roomId: '', isActive: true,
  temperatureWarningThreshold: 30, temperatureCriticalThreshold: 40,
  humidityWarningThreshold: 60, humidityCriticalThreshold: 80
};

const Devices: React.FC = () => {
  const [devices, setDevices] = useState<Device[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState(defaultFormState);

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [devicesData, roomsData] = await Promise.all([
        deviceService.getDevices(), roomService.getRooms()
      ]);
      setDevices(devicesData);
      setRooms(roomsData);
    } catch (error) {
      alert('Lỗi khi tải dữ liệu thiết bị!');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const openModal = (device?: Device) => {
    if (device) {
      setEditingId(device.id);
      setFormData({
        name: device.name, macAddress: device.macAddress || '', roomId: device.roomId || '', isActive: device.isActive,
        temperatureWarningThreshold: device.temperatureWarningThreshold, temperatureCriticalThreshold: device.temperatureCriticalThreshold,
        humidityWarningThreshold: device.humidityWarningThreshold, humidityCriticalThreshold: device.humidityCriticalThreshold
      });
    } else {
      setEditingId(null);
      setFormData(defaultFormState);
    }
    setIsModalOpen(true);
  };

  const closeModal = () => setIsModalOpen(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = { ...formData, roomId: formData.roomId === '' ? null : formData.roomId };
      if (editingId) await deviceService.updateDevice(editingId, payload);
      else await deviceService.createDevice(payload);
      
      alert('Lưu dữ liệu thành công!');
      closeModal();
      fetchData();
    } catch (error) {
      alert('Lỗi khi lưu thiết bị!');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa thiết bị này?')) {
      try {
        await deviceService.deleteDevice(id);
        alert('Xóa thành công!');
        fetchData();
      } catch (error) {
        alert('Lỗi khi xóa!');
      }
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Thiết bị IoT</h2>
        <button onClick={() => openModal()} className={styles.addBtn}>+ Thêm thiết bị</button>
      </div>

      <div className={styles.tableWrapper}>
        <div className={styles.tableResponsive}>
          <table className={styles.table}>
            <thead className={styles.thead}>
              <tr>
                <th className={styles.th}>Thiết bị</th>
                <th className={styles.th}>Phòng Server</th>
                <th className={styles.thCenter}>Trạng thái</th>
                <th className={styles.thCenter}>Cảnh báo (Nhiệt/Ẩm)</th>
                <th className={styles.thRight}>Thao tác</th>
              </tr>
            </thead>
            <tbody className={styles.tbody}>
              {isLoading ? (
                <tr><td colSpan={5} className="px-6 py-8 text-center text-slate-500">Đang tải dữ liệu...</td></tr>
              ) : devices.map((dev) => (
                <tr key={dev.id} className={styles.tr}>
                  <td className={styles.td}>
                    <div className={styles.textMain}>{dev.name}</div>
                    <div className={styles.textSub}>MAC: {dev.macAddress}</div>
                  </td>
                  <td className={styles.td}>
                    {dev.roomName ? <span className={styles.textRoom}>{dev.roomName}</span> : <span className={styles.textUnassigned}>Chưa gán</span>}
                  </td>
                  <td className={styles.tdCenter}>
                    <span className={dev.isActive ? styles.badgeActive : styles.badgeInactive}>
                      {dev.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className={styles.tdCenter}>
                    <span className="text-sm text-slate-600">
                      <span className="text-orange-600 font-medium">{dev.temperatureWarningThreshold}°C</span> / <span className="text-blue-600 font-medium">{dev.humidityWarningThreshold}%</span>
                    </span>
                  </td>
                  <td className={styles.tdRight}>
                    <button onClick={() => openModal(dev)} className={styles.editBtn}>Sửa</button>
                    <button onClick={() => handleDelete(dev.id)} className={styles.deleteBtn}>Xóa</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {isModalOpen && (
        <div className={styles.modalOverlay}>
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h3 className={styles.modalTitle}>{editingId ? 'Sửa Thiết bị' : 'Thêm Thiết bị mới'}</h3>
              <button onClick={closeModal} className={styles.closeBtn}>&times;</button>
            </div>
            <form onSubmit={handleSubmit} className={styles.form}>
              <div className={styles.formGrid}>
                <div className={styles.colSpace}>
                  <div>
                    <label className={styles.label}>Tên thiết bị *</label>
                    <input type="text" required value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} className={styles.input} />
                  </div>
                  <div>
                    <label className={styles.label}>Địa chỉ MAC *</label>
                    <input type="text" required value={formData.macAddress} onChange={(e) => setFormData({...formData, macAddress: e.target.value})} className={styles.input} placeholder="00:1B:44:11:3A:B7" />
                  </div>
                  <div>
                    <label className={styles.label}>Phòng Server</label>
                    <select value={formData.roomId} onChange={(e) => setFormData({...formData, roomId: e.target.value})} className={styles.input}>
                      <option value="">-- Chưa gán phòng --</option>
                      {rooms.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
                    </select>
                  </div>
                  <div className={styles.checkboxWrapper}>
                    <input type="checkbox" id="isActive" checked={formData.isActive} onChange={(e) => setFormData({...formData, isActive: e.target.checked})} className={styles.checkbox} />
                    <label htmlFor="isActive" className={styles.checkboxLabel}>Thiết bị đang hoạt động (Active)</label>
                  </div>
                </div>

                <div className={styles.thresholdCard}>
                  <h4 className={styles.thresholdTitle}>Ngưỡng cảnh báo</h4>
                  <div className={styles.thresholdGrid}>
                    <div>
                      <label className={styles.labelOrange}>Nhiệt độ cảnh báo (°C)</label>
                      <input type="number" required value={formData.temperatureWarningThreshold} onChange={(e) => setFormData({...formData, temperatureWarningThreshold: Number(e.target.value)})} className={styles.input} />
                    </div>
                    <div>
                      <label className={styles.labelRed}>Nhiệt độ nguy hiểm (°C)</label>
                      <input type="number" required value={formData.temperatureCriticalThreshold} onChange={(e) => setFormData({...formData, temperatureCriticalThreshold: Number(e.target.value)})} className={styles.input} />
                    </div>
                  </div>
                  <div className={styles.thresholdGrid}>
                    <div>
                      <label className={styles.labelBlueLight}>Độ ẩm cảnh báo (%)</label>
                      <input type="number" required value={formData.humidityWarningThreshold} onChange={(e) => setFormData({...formData, humidityWarningThreshold: Number(e.target.value)})} className={styles.input} />
                    </div>
                    <div>
                      <label className={styles.labelBlueDark}>Độ ẩm nguy hiểm (%)</label>
                      <input type="number" required value={formData.humidityCriticalThreshold} onChange={(e) => setFormData({...formData, humidityCriticalThreshold: Number(e.target.value)})} className={styles.input} />
                    </div>
                  </div>
                </div>
              </div>
              
              <div className={styles.formActions}>
                <button type="button" onClick={closeModal} className={styles.cancelBtn}>Hủy</button>
                <button type="submit" className={styles.submitBtn}>Lưu thiết bị</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Devices;