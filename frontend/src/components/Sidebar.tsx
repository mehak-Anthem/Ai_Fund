import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from '../context/AuthContext';

interface SidebarItemProps {
  icon: React.ReactNode;
  active?: boolean;
  onClick?: () => void;
  label: string;
  variant?: 'default' | 'danger';
}

const SidebarItem: React.FC<SidebarItemProps> = ({ icon, active, onClick, label, variant = 'default' }) => (
  <motion.button
    whileHover={{ scale: 1.05, x: 2 }}
    whileTap={{ scale: 0.95 }}
    onClick={onClick}
    className={`p-3.5 rounded-2xl transition-all duration-300 relative group mb-3 w-12 h-12 flex items-center justify-center ${
      active 
        ? 'bg-gradient-to-br from-indigo-500 to-violet-600 text-white shadow-lg shadow-indigo-500/30' 
        : variant === 'danger'
          ? 'text-rose-400 hover:bg-rose-500/10 hover:text-rose-500'
          : 'text-text-secondary hover:bg-bg-secondary hover:text-indigo-500'
    }`}
    title={label}
  >
    <div className="relative z-10 w-5 h-5 flex items-center justify-center">
      {icon}
    </div>
    
    <AnimatePresence>
      {active && (
        <motion.div 
          layoutId="active-glow"
          className="absolute inset-0 bg-indigo-500/20 blur-xl rounded-2xl -z-0 scale-125"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
        />
      )}
    </AnimatePresence>

    {/* Tooltip */}
    <div className="absolute left-full ml-4 px-2.5 py-1.5 bg-bg-primary border border-border-primary rounded-lg text-[10px] font-black uppercase tracking-widest text-text-primary opacity-0 group-hover:opacity-100 pointer-events-none transition-all duration-300 translate-x-1 group-hover:translate-x-0 shadow-xl z-50 whitespace-nowrap">
      {label}
    </div>
  </motion.button>
);

interface SidebarProps {
  currentView: 'chat' | 'dashboard';
  onViewChange: (view: 'chat' | 'dashboard') => void;
  isAdmin: boolean;
}

const Sidebar: React.FC<SidebarProps> = ({ currentView, onViewChange, isAdmin }) => {
  const { logout } = useAuth();

  return (
    <aside className="fixed left-6 top-1/2 -translate-y-1/2 z-50 flex flex-col items-center py-8 px-4 glass-panel rounded-[2.5rem] h-fit max-h-[85vh] border border-white/10 shadow-[0_20px_50px_rgba(0,0,0,0.3)] backdrop-blur-2xl">
      <motion.div 
        whileHover={{ rotate: 5, scale: 1.1 }}
        className="mb-10 relative"
      >
        <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-700 flex items-center justify-center text-white font-black text-xl shadow-2xl shadow-indigo-500/40 relative z-10">
          F
        </div>
        <div className="absolute inset-0 bg-indigo-500 blur-2xl opacity-20 -z-10" />
      </motion.div>

      <nav className="flex flex-col items-center">
        <SidebarItem
          label="Intelligence Chat"
          active={currentView === 'chat'}
          onClick={() => onViewChange('chat')}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 01.865-.501 48.172 48.172 0 003.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0012 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018z" />
            </svg>
          }
        />

        {isAdmin && (
          <SidebarItem
            label="Admin Console"
            active={currentView === 'dashboard'}
            onClick={() => onViewChange('dashboard')}
            icon={
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 3v11.25A2.25 2.25 0 006 16.5h2.25M3.75 3h-1.5m1.5 0h16.5m0 0h1.5m-1.5 0v11.25A2.25 2.25 0 0118 16.5h-2.25m-7.5 0h7.5m-7.5 0l-1 3m8.5-3l1 3m0 0l.5 1.5m-.5-1.5h-10.5m10.5 0l.5 1.5m-1.5-1.5h-7.5M12 10.5V12m3-4.5V12m-6-1.5V12" />
              </svg>
            }
          />
        )}
      </nav>

      <div className="mt-auto flex flex-col items-center pt-6 border-t border-white/5 w-full gap-5">
        <SidebarItem
          label="Sign Out"
          variant="danger"
          onClick={logout}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
            </svg>
          }
        />
      </div>
    </aside>
  );
};
export default Sidebar;
