import api from './api';
import type { Device, Response } from '../types';

export const deviceService = {
  getDevices: async () => {
    const response = await api.get<Response<Device[]>>('/devices');
    return response.data.data;
  },
  getDeviceById: async (id: string) => {
    const response = await api.get<Response<Device>>(`/devices/${id}`);
    return response.data.data;
  },
  createDevice: async (data: Omit<Device, 'id' | 'roomName'>) => {
    const response = await api.post<Response<string>>('/devices', data);
    return response.data.data;
  },
  updateDevice: async (id: string, data: Omit<Device, 'id' | 'roomName'>) => {
    const response = await api.put<Response<boolean>>(`/devices/${id}`, data);
    return response.data.data;
  },
  deleteDevice: async (id: string) => {
    const response = await api.delete<Response<boolean>>(`/devices/${id}`);
    return response.data.data;
  }
};