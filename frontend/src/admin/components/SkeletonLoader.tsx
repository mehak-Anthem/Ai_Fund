import React from 'react';

export const SkeletonLoader: React.FC = () => {
  return (
    <div className="animate-pulse space-y-4">
      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-5/6"></div>
    </div>
  );
};

export const TableSkeletonLoader: React.FC = () => {
  return (
    <div className="animate-pulse space-y-3">
      {[...Array(5)].map((_, i) => (
        <div key={i} className="flex space-x-4">
          <div className="h-12 bg-gray-200 dark:bg-gray-700 rounded flex-1"></div>
          <div className="h-12 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
          <div className="h-12 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
        </div>
      ))}
    </div>
  );
};
