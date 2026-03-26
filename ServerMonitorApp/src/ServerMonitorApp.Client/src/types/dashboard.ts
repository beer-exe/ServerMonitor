export interface DashboardDeviceDto {
  id: string;
  name: string;
  currentTemperature: number;
  currentHumidity: number;
  lastSeen: string;
  status: 'Normal' | 'Warning' | 'Danger' | 'Offline';
}

export interface DashboardRoomDto {
  id: string;
  name: string;
  description: string;
  devices: DashboardDeviceDto[];
}

export interface ChartDataPointDto {
  timestamp: string;
  temperature: number;
  humidity: number;
}

export interface DeviceUpdateDto {
  deviceId: string;
  temperature: number;
  humidity: number;
  timestamp: string;
  status: 'Normal' | 'Warning' | 'Danger' | 'Offline';
}