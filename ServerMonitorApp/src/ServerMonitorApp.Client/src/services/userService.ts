import api from './api';
import type { UserItem, Response } from '../types';

export const userService = {
  getUsers: async () => {
    const response = await api.get<Response<UserItem[]>>('/users');
    return response.data.data;
  },

  getUserById: async (id: string) => {
    const response = await api.get<Response<UserItem>>(`/users/${id}`);
    return response.data.data;
  },

  createUser: async (data: Pick<UserItem, 'username' | 'email' | 'role'> & { password?: string }) => {
    const response = await api.post<Response<string>>('/users', data);
    return response.data.data;
  },

  updateUser: async (id: string, data: Pick<UserItem, 'email' | 'role'>) => {
    const response = await api.put<Response<string>>(`/users/${id}`, data);
    return response.data.data;
  },

  deleteUser: async (id: string) => {
    const response = await api.delete<Response<string>>(`/users/${id}`);
    return response.data.data;
  }
};