import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { AdminState, AdminLoginResponse } from '../types/admin.types';

export const useAdminStore = create<AdminState>()(
  persist(
    (set) => ({
      isAuthenticated: false,
      token: null,
      user: null,
      theme: 'light',
      
      login: (token: string, user: AdminLoginResponse['user']) => {
        localStorage.setItem('adminToken', token);
        set({ isAuthenticated: true, token, user });
      },
      
      logout: () => {
        localStorage.removeItem('adminToken');
        set({ isAuthenticated: false, token: null, user: null });
      },
      
      toggleTheme: () => {
        set((state) => {
          const newTheme = state.theme === 'light' ? 'dark' : 'light';
          document.documentElement.classList.toggle('dark', newTheme === 'dark');
          return { theme: newTheme };
        });
      },
    }),
    {
      name: 'admin-storage',
      partialize: (state) => ({
        isAuthenticated: state.isAuthenticated,
        token: state.token,
        user: state.user,
        theme: state.theme,
      }),
    }
  )
);
