import { useState, useEffect } from 'react';
import { adminApi } from '../services/adminApi';
import { KnowledgeGap, KnowledgeGapUpdateRequest } from '../types/admin.types';
import toast from 'react-hot-toast';

export const useKnowledgeGaps = () => {
  const [data, setData] = useState<KnowledgeGap[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    try {
      setLoading(true);
      const gaps = await adminApi.getKnowledgeGaps();
      setData(gaps);
      setError(null);
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || 'Failed to fetch knowledge gaps';
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const updateStatus = async (id: string, status: KnowledgeGapUpdateRequest['status']) => {
    try {
      const updated = await adminApi.updateKnowledgeGapStatus(id, { status });
      setData((prev) => prev.map((gap) => (gap.id === id ? updated : gap)));
      toast.success('Status updated successfully');
    } catch (err: any) {
      const errorMsg = err.response?.data?.message || 'Failed to update status';
      toast.error(errorMsg);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  return { data, loading, error, refetch: fetchData, updateStatus };
};
