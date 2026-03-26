import React, { useState, useEffect, useCallback } from 'react';
import { alertService } from '../../services/alertService';
import type { AlertDto } from '../../types';
import styles from './Alerts.module.css';

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
  const [resolutionNote, setResolutionNote] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      console.error('Lỗi khi tải danh sách cảnh báo:', error);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize, severityFilter, statusFilter]);

  useEffect(() => {
    fetchAlerts();
  }, [fetchAlerts]);

  const totalPages = Math.ceil(totalRecords / pageSize);

  const getSeverityBadge = (severity: string) => {
    switch (severity?.toUpperCase()) {
      case 'CRITICAL': return <span className={styles.badgeCritical}>CRITICAL</span>;
      case 'WARNING': return <span className={styles.badgeWarning}>WARNING</span>;
      case 'OFFLINE': return <span className={styles.badgeOffline}>OFFLINE</span>;
      default: return <span className={styles.badgeOffline}>{severity}</span>;
    }
  };

  const openResolveModal = (id: number) => {
    setResolvingAlertId(id);
    setResolutionNote('');
    setIsModalOpen(true);
  };

  const handleResolveSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resolvingAlertId) return;

    setIsSubmitting(true);
    try {
      await alertService.resolveAlert(resolvingAlertId, resolutionNote);
      setIsModalOpen(false);
      fetchAlerts(); // Tải lại danh sách
    } catch (error: any) {
      alert(error.response?.data?.message || 'Có lỗi xảy ra khi xử lý cảnh báo!');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Lịch sử Cảnh báo & Sự cố</h2>
      </div>

      <div className={styles.filterCard}>
        <div className={styles.filterGroup}>
          <label className={styles.filterLabel}>Trạng thái</label>
          <select 
            value={statusFilter} 
            onChange={(e) => { setStatusFilter(e.target.value); setPageNumber(1); }}
            className={styles.filterSelect}
          >
            <option value="">Tất cả</option>
            <option value="unresolved">Chưa xử lý</option>
            <option value="resolved">Đã xử lý</option>
          </select>
        </div>

        <div className={styles.filterGroup}>
          <label className={styles.filterLabel}>Mức độ</label>
          <select 
            value={severityFilter} 
            onChange={(e) => { setSeverityFilter(e.target.value); setPageNumber(1); }}
            className={styles.filterSelect}
          >
            <option value="">Tất cả</option>
            <option value="CRITICAL">Critical (Nguy hiểm)</option>
            <option value="WARNING">Warning (Cảnh báo)</option>
            <option value="OFFLINE">Offline (Mất kết nối)</option>
          </select>
        </div>
      </div>

      <div className={styles.tableWrapper}>
        <div className={styles.tableResponsive}>
          <table className={styles.table}>
            <thead className={styles.thead}>
              <tr>
                <th className={styles.th}>Thời gian</th>
                <th className={styles.th}>Phòng / Thiết bị</th>
                <th className={styles.thCenter}>Mức độ</th>
                <th className={styles.th}>Nội dung</th>
                <th className={styles.thCenter}>Trạng thái</th>
                <th className={styles.thRight}>Thao tác</th>
              </tr>
            </thead>
            <tbody className={styles.tbody}>
              {isLoading ? (
                <tr><td colSpan={6} className="px-6 py-8 text-center text-slate-500">Đang tải dữ liệu...</td></tr>
              ) : alerts.length === 0 ? (
                <tr><td colSpan={6} className="px-6 py-8 text-center text-slate-500">Không tìm thấy cảnh báo nào.</td></tr>
              ) : alerts.map((alert) => (
                <tr key={alert.id} className={`${styles.tr} ${alert.isResolved ? styles.trResolved : ''}`}>
                  <td className={styles.td}>
                    <div className={styles.textMain}>{new Date(alert.createdAt).toLocaleDateString('vi-VN')}</div>
                    <div className={styles.textSub}>{new Date(alert.createdAt).toLocaleTimeString('vi-VN')}</div>
                  </td>
                  <td className={styles.td}>
                    <div className={styles.textMain}>{alert.roomName || 'Chưa rõ phòng'}</div>
                    <div className={styles.textSub}>{alert.deviceName || 'Hệ thống'}</div>
                  </td>
                  <td className={styles.tdCenter}>
                    {getSeverityBadge(alert.severity)}
                  </td>
                  <td className={styles.tdMessage} title={alert.message}>
                    {alert.message}
                  </td>
                  <td className={styles.tdCenter}>
                    <span className={alert.isResolved ? styles.badgeResolved : styles.badgeUnresolved}>
                      {alert.isResolved ? 'Đã xử lý' : 'Chưa xử lý'}
                    </span>
                  </td>
                  <td className={styles.tdRight}>
                    {!alert.isResolved && (
                      <button onClick={() => openResolveModal(alert.id)} className={styles.resolveBtn}>
                        Xử lý
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className={styles.paginationWrapper}>
          <span className={styles.pageInfo}>
            Hiển thị trang <span className="font-semibold">{pageNumber}</span> trên <span className="font-semibold">{totalPages || 1}</span> ({totalRecords} kết quả)
          </span>
          <div className={styles.pageControls}>
            <button 
              className={styles.pageBtn} 
              disabled={pageNumber <= 1 || isLoading}
              onClick={() => setPageNumber(prev => prev - 1)}
            >
              Trước
            </button>
            <button 
              className={styles.pageBtn} 
              disabled={pageNumber >= totalPages || isLoading}
              onClick={() => setPageNumber(prev => prev + 1)}
            >
              Sau
            </button>
          </div>
        </div>
      </div>

      {isModalOpen && (
        <div className={styles.modalOverlay}>
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h3 className={styles.modalTitle}>Xử lý Sự cố</h3>
              <button onClick={() => setIsModalOpen(false)} className={styles.closeBtn}>&times;</button>
            </div>
            <form onSubmit={handleResolveSubmit} className={styles.form}>
              <div>
                <label className={styles.label}>Ghi chú khắc phục *</label>
                <textarea 
                  required 
                  value={resolutionNote} 
                  onChange={(e) => setResolutionNote(e.target.value)} 
                  className={styles.textarea}
                  placeholder="Mô tả các bước đã thực hiện để khắc phục sự cố..."
                />
              </div>
              <div className={styles.formActions}>
                <button type="button" onClick={() => setIsModalOpen(false)} className={styles.cancelBtn} disabled={isSubmitting}>Hủy</button>
                <button type="submit" className={styles.submitBtn} disabled={isSubmitting}>
                  {isSubmitting ? 'Đang lưu...' : 'Xác nhận xử lý'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Alerts;