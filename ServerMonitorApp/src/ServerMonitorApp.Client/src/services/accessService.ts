import api from './api';
import type { Response, UserRoomAccessDto } from '../types';

export const accessService = {
  getRoomsByUser: async (userId: string) => {
    const response = await api.get<Response<UserRoomAccessDto[]>>(`/access/users/${userId}/rooms`);
    return response.data.data;
  },

  assignAccess: async (data: { userId: string; roomId: string; receiveAlerts: boolean }) => {
    const response = await api.post<Response<string>>('/access', data);
    return response.data.data;
  },

  updateAccess: async (data: { userId: string; roomId: string; receiveAlerts: boolean }) => {
    const response = await api.put<Response<string>>('/access', data);
    return response.data.data;
  },

  revokeAccess: async (userId: string, roomId: string) => {
    const response = await api.delete<Response<string>>(`/access/users/${userId}/rooms/${roomId}`);
    return response.data.data;
  }
};