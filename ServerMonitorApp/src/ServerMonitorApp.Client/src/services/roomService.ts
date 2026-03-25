import api from './api';
import type { Room, Response } from '../types';

export const roomService = {
  getRooms: async () => {
    const response = await api.get<Response<Room[]>>('/rooms');
    return response.data.data;
  },
  getRoomById: async (id: string) => {
    const response = await api.get<Response<Room>>(`/rooms/${id}`);
    return response.data.data;
  },
  createRoom: async (data: Omit<Room, 'id'>) => {
    const response = await api.post<Response<string>>('/rooms', data);
    return response.data.data;
  },
  updateRoom: async (id: string, data: Omit<Room, 'id'>) => {
    const response = await api.put<Response<boolean>>(`/rooms/${id}`, data);
    return response.data.data;
  },
  deleteRoom: async (id: string) => {
    const response = await api.delete<Response<boolean>>(`/rooms/${id}`);
    return response.data.data;
  }
};