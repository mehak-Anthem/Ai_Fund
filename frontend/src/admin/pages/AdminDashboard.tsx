import React from 'react';
import { AdminSidebar } from '../components/AdminSidebar';
import { AdminHeader } from '../components/AdminHeader';
import { StatCard } from '../components/StatCard';
import { useDashboardData } from '../hooks/useDashboardData';
import { MessageSquare, TrendingUp, AlertCircle, Users } from 'lucide-react';
import { motion } from 'framer-motion';

export const AdminDashboard: React.FC = () => {
  const { data, loading, refetch } = useDashboardData(true, 30000);

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <AdminSidebar />
      
      <div className="flex-1 ml-64">
        <AdminHeader title="Dashboard" onRefresh={refetch} refreshing={loading} />
        
        <main className="p-8">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <StatCard
              title="Total Queries"
              value={data?.totalQueries || 0}
              icon={MessageSquare}
              loading={loading}
            />
            <StatCard
              title="Avg AI Confidence"
              value={data?.avgConfidence ? (data.avgConfidence * 100).toFixed(1) : 0}
              suffix="%"
              icon={TrendingUp}
              loading={loading}
            />
            <StatCard
              title="Unanswered Queries"
              value={data?.unanswered || 0}
              icon={AlertCircle}
              loading={loading}
            />
            <StatCard
              title="Active Users"
              value={data?.activeUsers || 0}
              icon={Users}
              loading={loading}
            />
          </div>

          {data?.aiPerformanceScore !== undefined && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-200 dark:border-gray-700"
            >
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                AI Performance Score
              </h3>
              <div className="flex items-center space-x-4">
                <div className="flex-1">
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-gradient-to-r from-teal-500 to-emerald-600 transition-all duration-500"
                      style={{ width: `${data.aiPerformanceScore}%` }}
                    ></div>
                  </div>
                </div>
                <span className="text-2xl font-bold text-gray-900 dark:text-white">
                  {data.aiPerformanceScore}%
                </span>
              </div>
            </motion.div>
          )}
        </main>
      </div>
    </div>
  );
};
