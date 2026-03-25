import { useEffect, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import type { DeviceUpdateDto } from '../types/dashboard';
import { getTokens } from '../utils/token';

export const useSignalR = (url: string) => {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [latestUpdate, setLatestUpdate] = useState<DeviceUpdateDto | null>(null);

  useEffect(() => {
    const { accessToken } = getTokens();
    
    const newConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => accessToken || '',
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, [url]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('SignalR Connected');
          connection.on('ReceiveDeviceUpdate', (update: DeviceUpdateDto) => {
            setLatestUpdate(update);
          });
        })
        .catch(e => console.error('SignalR Connection Error: ', e));
    }
  }, [connection]);

  return { latestUpdate };
};