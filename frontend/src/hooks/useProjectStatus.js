import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const HUB_URL = process.env.REACT_APP_API_URL
  ? `${process.env.REACT_APP_API_URL.replace('/api', '')}/hubs/project-status`
  : 'http://localhost:5000/hubs/project-status';

/**
 * Connects to the ProjectStatusHub and subscribes to live status updates
 * for the given projectId.
 *
 * @param {number|null} projectId  - Project to watch (or null to skip).
 * @param {Function}    onStatusChanged - Callback called with { projectId, status, updatedAt }.
 * @param {string|null} token       - Bearer JWT token for authenticated connection.
 */
export function useProjectStatus(projectId, onStatusChanged, token) {
  const connectionRef = useRef(null);
  const callbackRef   = useRef(onStatusChanged);

  // Keep callback ref fresh without reconnecting
  useEffect(() => {
    callbackRef.current = onStatusChanged;
  }, [onStatusChanged]);

  const connect = useCallback(async () => {
    if (!projectId || !token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000]) // retry after 0s, 2s, 5s, 10s
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ProjectStatusChanged', (update) => {
      callbackRef.current?.(update);
    });

    connection.onreconnected(() => {
      connection.invoke('JoinProjectGroup', projectId).catch(console.error);
    });

    try {
      await connection.start();
      await connection.invoke('JoinProjectGroup', projectId);
      connectionRef.current = connection;
    } catch (err) {
      console.warn('SignalR connection failed:', err);
    }
  }, [projectId, token]);

  useEffect(() => {
    connect();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [connect]);
}
