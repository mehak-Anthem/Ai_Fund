import React from 'react';
import { AdminSidebar } from '../components/AdminSidebar';
import { AdminHeader } from '../components/AdminHeader';
import { KnowledgeGapTable } from '../components/KnowledgeGapTable';
import { useKnowledgeGaps } from '../hooks/useKnowledgeGaps';

export const AdminKnowledgeGaps: React.FC = () => {
  const { data, loading, refetch, updateStatus } = useKnowledgeGaps();

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <AdminSidebar />
      
      <div className="flex-1 ml-64">
        <AdminHeader title="Knowledge Gaps" onRefresh={refetch} refreshing={loading} />
        
        <main className="p-8">
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
