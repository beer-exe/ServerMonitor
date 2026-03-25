import React, { useState, useEffect } from 'react';
import { roomService } from '../../services/roomService';
import type { Room } from '../../types';
import styles from './Rooms.module.css';

const Rooms: React.FC = () => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({ name: '', location: '' });

  const fetchRooms = async () => {
    setIsLoading(true);
    try {
      const data = await roomService.getRooms();
      setRooms(data);
    } catch (error) {
      alert('Lỗi khi tải danh sách phòng!');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchRooms(); }, []);

  const openModal = (room?: Room) => {
    if (room) {
      setEditingId(room.id);
      setFormData({ name: room.name, location: room.location });
    } else {
      setEditingId(null);
      setFormData({ name: '', location: '' });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => setIsModalOpen(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) await roomService.updateRoom(editingId, formData);
      else await roomService.createRoom(formData);
      alert('Lưu thành công!');
      closeModal();
      fetchRooms();
    } catch (error) {
      alert('Lỗi khi lưu dữ liệu!');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Bạn có chắc muốn xóa phòng này?')) {
      try {
        await roomService.deleteRoom(id);
        alert('Xóa thành công!');
        fetchRooms();
      } catch (error) {
        alert('Lỗi khi xóa phòng!');
      }
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Quản lý Phòng Server</h2>
        <button onClick={() => openModal()} className={styles.addBtn}>+ Thêm phòng mới</button>
      </div>

      <div className={styles.tableWrapper}>
        <div className={styles.tableResponsive}>
          <table className={styles.table}>
            <thead className={styles.thead}>
              <tr>
                <th className={styles.th}>Tên phòng</th>
                <th className={styles.th}>Vị trí</th>
                <th className={styles.thRight}>Thao tác</th>
              </tr>
            </thead>
            <tbody className={styles.tbody}>
              {isLoading ? (
                <tr><td colSpan={3} className="px-6 py-8 text-center text-slate-500">Đang tải dữ liệu...</td></tr>
              ) : rooms.map((room) => (
                <tr key={room.id} className={styles.tr}>
                  <td className={`${styles.td} ${styles.tdText}`}>{room.name}</td>
                  <td className={`${styles.td} ${styles.tdSubText}`}>{room.location}</td>
                  <td className={styles.tdActions}>
                    <button onClick={() => openModal(room)} className={styles.editBtn}>Sửa</button>
                    <button onClick={() => handleDelete(room.id)} className={styles.deleteBtn}>Xóa</button>
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
              <h3 className={styles.modalTitle}>{editingId ? 'Sửa phòng' : 'Thêm phòng mới'}</h3>
              <button onClick={closeModal} className={styles.closeBtn}>&times;</button>
            </div>
            <form onSubmit={handleSubmit} className={styles.form}>
              <div>
                <label className={styles.label}>Tên phòng *</label>
                <input type="text" required value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} className={styles.input} />
              </div>
              <div>
                <label className={styles.label}>Vị trí *</label>
                <input type="text" required value={formData.location} onChange={(e) => setFormData({...formData, location: e.target.value})} className={styles.input} />
              </div>
              <div className={styles.formActions}>
                <button type="button" onClick={closeModal} className={styles.cancelBtn}>Hủy</button>
                <button type="submit" className={styles.submitBtn}>Lưu lại</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Rooms;