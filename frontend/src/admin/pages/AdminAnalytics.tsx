import React from 'react';
import { AdminSidebar } from '../components/AdminSidebar';
import { AdminHeader } from '../components/AdminHeader';
import { ChartCard } from '../components/ChartCard';
import { useAnalytics } from '../hooks/useAnalytics';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';

export const AdminAnalytics: React.FC = () => {
  const { data, loading, refetch } = useAnalytics(true, 30000);

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <AdminSidebar />
      
      <div className="flex-1 ml-64">
        <AdminHeader title="Analytics" onRefresh={refetch} refreshing={loading} />
        
        <main className="p-8 space-y-6">
          <ChartCard title="Queries Over Time" loading={loading}>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={data?.queriesOverTime || []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="date" stroke="#9CA3AF" />
                <YAxis stroke="#9CA3AF" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1F2937',
                    border: 'none',
                    borderRadius: '8px',
                    color: '#fff',
                  }}
                />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="value"
                  stroke="#14B8A6"
                  strokeWidth={2}
                  dot={{ fill: '#14B8A6', r: 4 }}
                  name="Queries"
                />
              </LineChart>
            </ResponsiveContainer>
          </ChartCard>

          <ChartCard title="AI Confidence Trend" loading={loading}>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={data?.confidenceTrend || []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="date" stroke="#9CA3AF" />
                <YAxis stroke="#9CA3AF" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1F2937',
                    border: 'none',
                    borderRadius: '8px',
                    color: '#fff',
                  }}
                />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="value"
                  stroke="#10B981"
                  strokeWidth={2}
                  dot={{ fill: '#10B981', r: 4 }}
                  name="Confidence"
                />
              </LineChart>
            </ResponsiveContainer>
          </ChartCard>

          <ChartCard title="Category Usage" loading={loading}>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={data?.categoryUsage || []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="category" stroke="#9CA3AF" />
                <YAxis stroke="#9CA3AF" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1F2937',
                    border: 'none',
                    borderRadius: '8px',
                    color: '#fff',
                  }}
                />
                <Legend />
                <Bar dataKey="count" fill="#14B8A6" radius={[8, 8, 0, 0]} name="Count" />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>
        </main>
      </div>
    </div>
  );
};
