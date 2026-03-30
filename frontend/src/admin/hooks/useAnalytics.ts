import { useState, useEffect } from 'react';
import { adminApi } from '../services/adminApi';
import { AnalyticsData } from '../types/admin.types';
import toast from 'react-hot-toast';

export const useAnalytics = (autoRefresh = true, interval = 30000) => {
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async (showLoading = true) => {
    try {
      if (showLoading) setLoading(true);
      const analytics = await adminApi.getAnalytics();
      setData(analytics);
      setError(null);
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || 'Failed to fetch analytics data';
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();

    if (autoRefresh) {
      const intervalId = setInterval(() => {
        fetchData(false);
      }, interval);

      return () => clearInterval(intervalId);
    }
  }, [autoRefresh, interval]);

  return { data, loading, error, refetch: fetchData };
};
