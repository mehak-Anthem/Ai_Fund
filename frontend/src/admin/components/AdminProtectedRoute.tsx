import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAdminStore } from '../store/adminStore';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export const AdminProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated } = useAdminStore();

  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />;
  }

  return <>{children}</>;
};
