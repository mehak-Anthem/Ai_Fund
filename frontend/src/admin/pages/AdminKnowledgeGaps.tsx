import React, { useEffect, useState } from 'react';
import { AdminSidebar } from '../components/AdminSidebar';
import { AdminHeader } from '../components/AdminHeader';
import { KnowledgeGapTable } from '../components/KnowledgeGapTable';
import { useKnowledgeGaps } from '../hooks/useKnowledgeGaps';
import { adminApi } from '../services/adminApi';
import { QdrantStatus } from '../types/admin.types';
import { Database, RefreshCw } from 'lucide-react';
import toast from 'react-hot-toast';

export const AdminKnowledgeGaps: React.FC = () => {
  const { data, loading, refetch, updateStatus } = useKnowledgeGaps();
  const [qdrantStatus, setQdrantStatus] = useState<QdrantStatus | null>(null);
  const [syncing, setSyncing] = useState(false);
  const [statusLoading, setStatusLoading] = useState(true);

  const fetchQdrantStatus = async () => {
    try {
      setStatusLoading(true);
      const status = await adminApi.getQdrantStatus();
      setQdrantStatus(status);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to load Qdrant status');
    } finally {
      setStatusLoading(false);
    }
  };

  const handleRefresh = async () => {
    await Promise.all([refetch(), fetchQdrantStatus()]);
  };

  const handleSync = async () => {
    try {
      setSyncing(true);
      const response = await adminApi.syncKnowledgeToQdrant();
      toast.success(response.message || 'Knowledge synced to Qdrant');
      await fetchQdrantStatus();
    } catch (err: any) {
      toast.error(err.response?.data?.error || err.response?.data?.message || 'Sync failed');
    } finally {
      setSyncing(false);
    }
  };

  useEffect(() => {
    fetchQdrantStatus();
  }, []);

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <AdminSidebar />
      
      <div className="flex-1 ml-64">
        <AdminHeader title="Knowledge Gaps" onRefresh={handleRefresh} refreshing={loading || statusLoading} />
        
        <main className="p-8">
          <div className="mb-6 grid grid-cols-1 xl:grid-cols-[minmax(0,1fr)_auto] gap-4">
            <div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg border border-gray-200 dark:border-gray-700 p-6">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Qdrant Collection</p>
                  <h2 className="mt-2 text-xl font-semibold text-gray-900 dark:text-white">
                    {qdrantStatus?.collectionName || 'ai_fund_knowledge'}
                  </h2>
                  <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                    {statusLoading
                      ? 'Checking vector store status...'
                      : qdrantStatus?.collectionExists
                      ? 'Vector search is connected and ready.'
                      : 'Collection is not initialized yet.'}
                  </p>
                </div>

                <div className="flex items-center gap-2 rounded-full px-3 py-1.5 bg-gray-100 dark:bg-gray-700">
                  <Database className="w-4 h-4 text-teal-600 dark:text-teal-400" />
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    {statusLoading ? 'Checking' : qdrantStatus?.status || 'Unknown'}
                  </span>
                </div>
              </div>
            </div>

            <button
              onClick={handleSync}
              disabled={syncing}
              className="inline-flex items-center justify-center gap-2 px-5 py-3 rounded-xl bg-gradient-to-r from-teal-500 to-emerald-600 text-white font-semibold shadow-lg hover:shadow-xl disabled:opacity-60 disabled:cursor-not-allowed"
            >
              <RefreshCw className={`w-4 h-4 ${syncing ? 'animate-spin' : ''}`} />
              {syncing ? 'Syncing...' : 'Sync To Qdrant'}
            </button>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-lg border border-gray-200 dark:border-gray-700 p-6">
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                Manage Knowledge Gaps
              </h2>
              <p className="text-gray-600 dark:text-gray-400">
                Track and resolve low-confidence queries to improve AI performance
              </p>
            </div>

            <KnowledgeGapTable data={data} loading={loading} onStatusUpdate={updateStatus} />
          </div>
        </main>
      </div>
    </div>
  );
};
