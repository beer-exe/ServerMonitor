import api from './api';
import type { DashboardRoomDto, ChartDataPointDto } from '../types/dashboard';

export interface PaginatedResult<T> {
  items: T[];
  totalRecords: number;
}

export const dashboardService = {
  getDashboard: async (): Promise<DashboardRoomDto[]> => {
    const response = await api.get('/dashboard');
    const actualData = response.data.data || response.data.Data || response.data;
    return actualData;
  },

getHistoricalData: async (
    deviceId: string,
    startTime: string,
    endTime: string,
    pageNumber: number = 1,
    pageSize: number = 50
  ): Promise<PaginatedResult<ChartDataPointDto>> => {
    const response = await api.get(`/dashboard/history/devices/${deviceId}`, {
      params: { startTime, endTime, pageNumber, pageSize },
    });
    
    const items = response.data.data || response.data.Data || [];
    
    const totalRecords = response.data.totalRecords || response.data.TotalRecords || 0;

    return { items, totalRecords };
  },
};