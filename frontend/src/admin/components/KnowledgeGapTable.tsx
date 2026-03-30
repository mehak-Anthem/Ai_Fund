import React, { useState } from 'react';
import { KnowledgeGap } from '../types/admin.types';
import { StatusBadge } from './StatusBadge';
import { Search } from 'lucide-react';
import { TableSkeletonLoader } from './SkeletonLoader';

interface KnowledgeGapTableProps {
  data: KnowledgeGap[];
  loading: boolean;
  onStatusUpdate: (id: string, status: 'New' | 'Reviewing' | 'Resolved') => void;
}

export const KnowledgeGapTable: React.FC<KnowledgeGapTableProps> = ({
  data,
  loading,
  onStatusUpdate,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  const filteredData = data.filter((gap) => {
    const matchesSearch = gap.question.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === 'all' || gap.status === filterStatus;
    return matchesSearch && matchesStatus;
  });

  if (loading) {
    return <TableSkeletonLoader />;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center space-x-4">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search questions..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-teal-500 focus:border-transparent"
          />
        </div>

        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-teal-500 focus:border-transparent"
        >
          <option value="all">All Status</option>
          <option value="New">New</option>
          <option value="Reviewing">Reviewing</option>
          <option value="Resolved">Resolved</option>
        </select>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Question
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Count
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Confidence
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Last Asked
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            {filteredData.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                  No knowledge gaps found
                </td>
              </tr>
            ) : (
              filteredData.map((gap) => (
                <tr key={gap.id} className="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <td className="px-6 py-4 text-sm text-gray-900 dark:text-white max-w-md truncate">
                    {gap.question}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900 dark:text-white">{gap.count}</td>
                  <td className="px-6 py-4 text-sm">
                    <span
                      className={`font-medium ${
                        gap.confidenceScore >= 0.7
                          ? 'text-green-600'
                          : gap.confidenceScore >= 0.4
                          ? 'text-yellow-600'
                          : 'text-red-600'
                      }`}
                    >
                      {(gap.confidenceScore * 100).toFixed(0)}%
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <StatusBadge status={gap.status} />
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                    {new Date(gap.lastAsked).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4">
                    <select
                      value={gap.status}
                      onChange={(e) =>
                        onStatusUpdate(gap.id, e.target.value as 'New' | 'Reviewing' | 'Resolved')
                      }
                      className="text-sm px-3 py-1 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-teal-500"
                    >
                      <option value="New">New</option>
                      <option value="Reviewing">Reviewing</option>
                      <option value="Resolved">Resolved</option>
                    </select>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
