import React, { useState, useEffect } from 'react';
import { dashboardService } from '../../services/dashboardService';
import type { ChartDataPointDto } from '../../types/dashboard';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

import { DatePicker } from 'antd';
import dayjs, { Dayjs } from 'dayjs';

import styles from './DeviceHistoryChart.module.css';

interface DeviceHistoryChartProps {
  deviceId: string;
}

const DeviceHistoryChart: React.FC<DeviceHistoryChartProps> = ({ deviceId }) => {
  const [data, setData] = useState<ChartDataPointDto[]>([]);
  const [totalRecords, setTotalRecords] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const [startTime, setStartTime] = useState<Dayjs>(dayjs().subtract(24, 'hour'));
  const [endTime, setEndTime] = useState<Dayjs>(dayjs());
  
  const [pageNumber, setPageNumber] = useState<number>(1);
  const pageSize = 200; 

  useEffect(() => {
    fetchData();
  }, [deviceId, startTime, endTime, pageNumber]); 

  const fetchData = async () => {
    try {
      setLoading(true);
      setError(null);

      const result = await dashboardService.getHistoricalData(
        deviceId,
        startTime.toISOString(),
        endTime.toISOString(),
        pageNumber,
        pageSize
      );
      
      setData(result.items || []);
      setTotalRecords(result.totalRecords || 0);

    } catch (err: any) {
      setError(`Lỗi tải dữ liệu lịch sử: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const totalPages = Math.ceil(totalRecords / pageSize) || 1;

  return (
    <div className={styles.container}>
      <h3 className={styles.title}>Lịch sử hoạt động của thiết bị</h3>

      <div className={styles.filtersWrapper}>
        <div className={styles.filterBlock}>
          <label className={styles.filterLabel}>Từ thời điểm:</label>
          <DatePicker 
            showTime={{ format: 'HH:mm' }} 
            format="DD/MM/YYYY HH:mm"
            value={startTime}
            onChange={(date) => {
              if (date) {
                setStartTime(date);
                setPageNumber(1);
              }
            }}
            className={styles.datePicker}
            allowClear={false}
          />
        </div>
        
        <div className={styles.filterBlock}>
          <label className={styles.filterLabel}>Đến thời điểm:</label>
          <DatePicker 
            showTime={{ format: 'HH:mm' }} 
            format="DD/MM/YYYY HH:mm"
            value={endTime}
            onChange={(date) => {
              if (date) {
                setEndTime(date);
                setPageNumber(1);
              }
            }}
            className={styles.datePicker}
            allowClear={false}
          />
        </div>
      </div>

      {loading && <p className={styles.loadingText}>Đang tải dữ liệu biểu đồ...</p>}
      {error && <p className={styles.errorText}>{error}</p>}
      {!loading && !error && data.length === 0 && (
        <p className={styles.emptyText}>Không có dữ liệu cảm biến trong khoảng thời gian này.</p>
      )}

      {!loading && data.length > 0 && (
        <div className={styles.chartWrapper}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data}>
              <CartesianGrid strokeDasharray="3 3" />
              
              <XAxis 
                dataKey="timestamp" 
                tickFormatter={(timeStr) => {
                  const utcTimeStr = timeStr.endsWith('Z') ? timeStr : `${timeStr}Z`;
                  return new Date(utcTimeStr).toLocaleTimeString('vi-VN', { 
                    hour: '2-digit', 
                    minute: '2-digit', 
                    hour12: false 
                  });
                }} 
              />
              <YAxis />
              
              <Tooltip 
                labelFormatter={(label) => {
                  const utcTimeStr = label.endsWith('Z') ? label : `${label}Z`;
                  return new Date(utcTimeStr).toLocaleString('vi-VN', { 
                    hour12: false 
                  });
                }} 
              />
              
              <Line type="monotone" dataKey="temperature" name="Nhiệt độ (°C)" stroke="#ef4444" activeDot={{ r: 8 }} />
              <Line type="monotone" dataKey="humidity" name="Độ ẩm (%)" stroke="#3b82f6" activeDot={{ r: 8 }} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      <div className={styles.paginationWrapper}>
        <button 
          onClick={() => setPageNumber(prev => Math.max(prev - 1, 1))}
          disabled={pageNumber <= 1 || loading}
          className={styles.btnPrev}
        >
          Trang trước
        </button>
        
        <span className={styles.pageInfo}>
          Trang {pageNumber} / {totalPages} 
          <span className={styles.totalRecords}>({totalRecords} bản ghi)</span>
        </span>

        <button 
          onClick={() => setPageNumber(prev => prev + 1)}
          disabled={pageNumber >= totalPages || loading} 
          className={styles.btnNext}
        >
          Trang tiếp
        </button>
      </div>
    </div>
  );
};

export default DeviceHistoryChart;