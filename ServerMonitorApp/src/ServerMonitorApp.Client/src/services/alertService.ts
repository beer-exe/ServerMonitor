import api from './api';
import type { AlertDto, PagedResponse } from '../types';

export interface AlertQueryParams {
  roomId?: string;
  severity?: string;
  isResolved?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export const alertService = {
  getAlerts: async (params: AlertQueryParams): Promise<PagedResponse<AlertDto[]>> => {
    const response = await api.get('/alerts', { params });
    return response.data;
  },

  resolveAlert: async (id: number, resolutionNote: string) => {
    const response = await api.put(`/alerts/${id}/resolve`, { resolutionNote });
    return response.data;
  }
};