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
  macAddress: string;
  roomId: string | null;
  roomName?: string;
  isActive: boolean;
  temperatureWarningThreshold: number;
  temperatureCriticalThreshold: number;
  humidityWarningThreshold: number;
  humidityCriticalThreshold: number;
}