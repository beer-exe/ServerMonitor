export interface Response<T> {
  succeeded: boolean;
  message: string;
  data: T;
  errors: string[] | null;
}

export interface PagedResponse<T> extends Response<T> {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
}

export interface Room {
  id: string;
  name: string;
  location: string;
  description?: string;
}

export interface Device {
  id: string;
  name: string;
  roomId: string | null;
  roomName?: string;
  isActive: boolean;
  temperatureWarningThreshold: number;
  temperatureCriticalThreshold: number;
  humidityWarningThreshold: number;
  humidityCriticalThreshold: number;
}

export interface AlertDto {
  id: number;
  roomId: string | null;
  roomName: string | null;
  deviceId: string | null;
  deviceName: string | null;
  message: string;
  severity: string;
  isResolved: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UserItem {
  id: string;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  updatedAt: string;
}