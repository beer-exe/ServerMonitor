import React, { useState, useEffect } from 'react';
import { dashboardService } from '../../services/dashboardService';
import type { ChartDataPointDto } from '../../types/dashboard';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

import { DatePicker } from 'antd';
import dayjs, { Dayjs } from 'dayjs';

import styles from './DeviceHistoryChart.module.css';

interface DeviceHistoryChartProps {
  deviceId: string;
}

const CustomTooltip = ({ active, payload, label }: any) => {
  if (active && payload && payload.length) {
    const timeStr = label.endsWith('Z') ? label : `${label}Z`;
    const date = new Date(timeStr);
    
    return (
      <div className="bg-white/95 backdrop-blur-md p-3 border border-slate-200 shadow-xl rounded-xl min-w-[150px]">
        <p className="text-slate-500 text-[11px] uppercase font-bold tracking-wider mb-2 border-b border-slate-100 pb-2">
          {date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - {date.toLocaleDateString('vi-VN')}
        </p>
        {payload.map((entry: any, index: number) => (
          <div key={index} className="flex justify-between items-center gap-4 text-sm font-bold mt-1" style={{ color: entry.color }}>
            <div className="flex items-center gap-2">
              <div className="w-2.5 h-2.5 rounded-full shadow-sm" style={{ backgroundColor: entry.color }}></div>
              <span className="font-medium text-slate-700">{entry.name}</span>
            </div>
            <span>{entry.value}{entry.dataKey === 'temperature' ? '°C' : '%'}</span>
          </div>
        ))}
      </div>
    );
  }
  return null;
};

const DeviceHistoryChart: React.FC<DeviceHistoryChartProps> = ({ deviceId }) => {
  const [data, setData] = useState<ChartDataPointDto[]>([]);
  const [totalRecords, setTotalRecords] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const [startTime, setStartTime] = useState<Dayjs>(dayjs().subtract(24, 'hour'));
  const [endTime, setEndTime] = useState<Dayjs>(dayjs());
  
  const [pageNumber, setPageNumber] = useState<number>(1);
  const pageSize = 200; 

  const [windowWidth, setWindowWidth] = useState(typeof window !== 'undefined' ? window.innerWidth : 1024);

  useEffect(() => {
    const handleResize = () => setWindowWidth(window.innerWidth);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const isMobile = windowWidth < 768;

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
        <div className={styles.chartWrapper} style={{ height: isMobile ? '280px' : '350px' }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart 
              data={data} 
              margin={{ top: 10, right: 10, left: isMobile ? -30 : 0, bottom: 0 }}
            >
              <defs>
                <linearGradient id="colorTemp" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#ef4444" stopOpacity={0.4}/>
                  <stop offset="95%" stopColor="#ef4444" stopOpacity={0}/>
                </linearGradient>
                <linearGradient id="colorHum" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.4}/>
                  <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                </linearGradient>
              </defs>

              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
              
              <XAxis 
                dataKey="timestamp" 
                tick={{ fontSize: 11, fill: '#64748b' }}
                tickLine={false}
                axisLine={false}
                minTickGap={30}
                tickFormatter={(timeStr) => {
                  const utcTimeStr = timeStr.endsWith('Z') ? timeStr : `${timeStr}Z`;
                  return new Date(utcTimeStr).toLocaleTimeString('vi-VN', { 
                    hour: '2-digit', 
                    minute: '2-digit', 
                    hour12: false 
                  });
                }} 
              />
              
              <YAxis 
                hide={isMobile}
                tick={{ fontSize: 11, fill: '#64748b' }}
                tickLine={false}
                axisLine={false}
              />
              
              <Tooltip content={<CustomTooltip />} />
              
              <Area 
                type="monotone" 
                dataKey="temperature" 
                name="Nhiệt độ" 
                stroke="#ef4444" 
                strokeWidth={3}
                fillOpacity={1} 
                fill="url(#colorTemp)" 
                dot={false}
                activeDot={{ r: 6, strokeWidth: 0, fill: '#ef4444', style: { filter: 'drop-shadow(0px 2px 4px rgba(239, 68, 68, 0.5))' } }}
              />
              <Area 
                type="monotone" 
                dataKey="humidity" 
                name="Độ ẩm" 
                stroke="#3b82f6" 
                strokeWidth={3}
                fillOpacity={1} 
                fill="url(#colorHum)" 
                dot={false}
                activeDot={{ r: 6, strokeWidth: 0, fill: '#3b82f6', style: { filter: 'drop-shadow(0px 2px 4px rgba(59, 130, 246, 0.5))' } }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}

      <div className={styles.paginationWrapper}>
        <div className={styles.pageControls}>
          <button 
            onClick={() => setPageNumber(prev => Math.max(prev - 1, 1))}
            disabled={pageNumber <= 1 || loading}
            className={styles.btnPrev}
          >
            Trang trước
          </button>
          
          <button 
            onClick={() => setPageNumber(prev => prev + 1)}
            disabled={pageNumber >= totalPages || loading} 
            className={styles.btnNext}
          >
            Trang tiếp
          </button>
        </div>
        
        <div className={styles.pageInfo}>
          Trang <span className="text-indigo-600 font-bold">{pageNumber}</span> / {totalPages} 
          <span className={styles.totalRecords}>({totalRecords} bản ghi)</span>
        </div>
      </div>
    </div>
  );
};

export default DeviceHistoryChart;