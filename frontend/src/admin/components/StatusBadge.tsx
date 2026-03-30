import React from 'react';

interface StatusBadgeProps {
  status: 'New' | 'Reviewing' | 'Resolved';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  const styles = {
    New: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    Reviewing: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
    Resolved: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  };

  return (
    <span
      className={`px-3 py-1 rounded-full text-xs font-semibold ${styles[status]}`}
    >
      {status}
    </span>
  );
};
