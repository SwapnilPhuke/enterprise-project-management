import { useState, useEffect, useCallback } from 'react';
import { projectsApi } from '../services/projectsApi';

export function useDashboard() {
  const [stats,   setStats]   = useState(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState('');

  const fetchStats = useCallback(() => {
    setLoading(true);
    projectsApi.getStats()
      .then(res => setStats(res.data))
      .catch(() => setError('Could not load stats. Make sure the API is running.'))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { fetchStats(); }, [fetchStats]);

  return { stats, loading, error, refresh: fetchStats };
}
