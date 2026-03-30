import React from 'react';
import { useAdminStore } from '../store/adminStore';
import { User, RefreshCw } from 'lucide-react';

interface AdminHeaderProps {
  title: string;
  onRefresh?: () => void;
  refreshing?: boolean;
}

export const AdminHeader: React.FC<AdminHeaderProps> = ({ title, onRefresh, refreshing }) => {
  const { user } = useAdminStore();

  return (
    <header className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-8 py-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{title}</h1>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Welcome back, {user?.username || 'Admin'}
          </p>
        </div>

        <div className="flex items-center space-x-4">
          {onRefresh && (
            <button
              onClick={onRefresh}
              disabled={refreshing}
              className="p-2 rounded-lg bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors disabled:opacity-50"
            >
              <RefreshCw className={`w-5 h-5 ${refreshing ? 'animate-spin' : ''}`} />
            </button>
          )}

          <div className="flex items-center space-x-2 px-4 py-2 bg-gray-100 dark:bg-gray-700 rounded-lg">
            <User className="w-5 h-5 text-gray-600 dark:text-gray-400" />
            <span className="text-sm font-medium text-gray-900 dark:text-white">
              {user?.username || 'Admin'}
            </span>
          </div>
        </div>
      </div>
    </header>
  );
};
