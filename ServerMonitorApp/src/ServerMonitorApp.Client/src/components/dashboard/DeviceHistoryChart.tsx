import React, { useState, useEffect } from 'react';
import { dashboardService } from '../../services/dashboardService';
import type { ChartDataPointDto } from '../../types/dashboard';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

// IMPORT ANT DESIGN
import { DatePicker } from 'antd';
import dayjs, { Dayjs } from 'dayjs';

interface DeviceHistoryChartProps {
  deviceId: string;
}

const DeviceHistoryChart: React.FC<DeviceHistoryChartProps> = ({ deviceId }) => {
  const [data, setData] = useState<ChartDataPointDto[]>([]);
  const [totalRecords, setTotalRecords] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Dùng dayjs cho Ant Design
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

      // Chuyển Dayjs object về chuẩn ISO string (UTC) cho Backend C#
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
    <div className="bg-white p-4 rounded-lg shadow-md">
      <h3 className="text-lg font-bold mb-4">Lịch sử hoạt động của thiết bị</h3>

      {/* SỬ DỤNG BỘ CHỌN THỜI GIAN CỦA ANT DESIGN */}
      <div className="flex gap-4 mb-4 items-center">
        <div className="flex flex-col">
          <label className="text-sm font-medium mb-1">Từ thời điểm:</label>
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
            className="h-[42px] w-[220px]"
            allowClear={false}
          />
        </div>
        
        <div className="flex flex-col">
          <label className="text-sm font-medium mb-1">Đến thời điểm:</label>
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
            className="h-[42px] w-[220px]"
            allowClear={false}
          />
        </div>
      </div>

      {loading && <p className="text-blue-500 font-medium">Đang tải dữ liệu biểu đồ...</p>}
      {error && <p className="text-red-500 font-medium">{error}</p>}
      {!loading && !error && data.length === 0 && (
        <p className="text-gray-500 italic">Không có dữ liệu cảm biến trong khoảng thời gian này.</p>
      )}

      {!loading && data.length > 0 && (
        <div className="h-64 w-full mt-4">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={data}>
              <CartesianGrid strokeDasharray="3 3" />
              
              {/* TRỤC X: Tự động cộng 7 tiếng nhờ thêm đuôi 'Z' */}
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
              
              {/* TOOLTIP: Tự động cộng 7 tiếng nhờ thêm đuôi 'Z' */}
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

      {/* Phân trang */}
      <div className="flex justify-between items-center mt-6">
        <button 
          onClick={() => setPageNumber(prev => Math.max(prev - 1, 1))}
          disabled={pageNumber <= 1 || loading}
          className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50 hover:bg-gray-300 transition-colors"
        >
          Trang trước
        </button>
        
        <span className="font-medium text-gray-700">
          Trang {pageNumber} / {totalPages} 
          <span className="text-sm text-gray-500 ml-2">({totalRecords} bản ghi)</span>
        </span>

        <button 
          onClick={() => setPageNumber(prev => prev + 1)}
          disabled={pageNumber >= totalPages || loading} 
          className="px-4 py-2 bg-blue-500 text-white rounded disabled:opacity-50 hover:bg-blue-600 transition-colors"
        >
          Trang tiếp
        </button>
      </div>
    </div>
  );
};

export default DeviceHistoryChart;