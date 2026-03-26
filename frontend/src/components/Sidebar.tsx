import React from 'react';

interface SidebarItemProps {
  icon: React.ReactNode;
  active?: boolean;
  onClick?: () => void;
  label: string;
}

const SidebarItem: React.FC<SidebarItemProps> = ({ icon, active, onClick, label }) => (
  <button
    onClick={onClick}
    className={`p-3 rounded-2xl transition-all duration-300 relative group mb-4 ${
      active 
        ? 'bg-indigo-500/10 text-indigo-500' 
        : 'text-text-secondary hover:bg-bg-secondary hover:text-text-primary'
    }`}
    title={label}
  >
    <div className="relative z-10 w-6 h-6 flex items-center justify-center">
      {icon}
    </div>
    {active && (
      <div className="absolute inset-0 bg-indigo-500/20 blur-xl rounded-2xl -z-0 scale-150 animate-pulse" />
    )}
  </button>
);

interface SidebarProps {
  currentView: 'chat' | 'dashboard';
  onViewChange: (view: 'chat' | 'dashboard') => void;
  isAdmin: boolean;
}

const Sidebar: React.FC<SidebarProps> = ({ currentView, onViewChange, isAdmin }) => {
  return (
    <aside className="fixed left-6 top-1/2 -translate-y-1/2 z-50 flex flex-col items-center py-6 px-3 glass-panel rounded-3xl h-fit max-h-[80vh] shadow-2xl">
      <div className="mb-8 p-1">
        <div className="w-10 h-10 rounded-2xl bg-indigo-600 flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-indigo-500/20">
          F
        </div>
      </div>

      <nav className="flex flex-col">
        <SidebarItem
          label="Chat"
          active={currentView === 'chat'}
          onClick={() => onViewChange('chat')}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6">
              <path strokeLinecap="round" strokeLinejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 01.865-.501 48.172 48.172 0 003.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0012 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018z" />
            </svg>
          }
        />

        {isAdmin && (
          <SidebarItem
            label="Dashboard"
            active={currentView === 'dashboard'}
            onClick={() => onViewChange('dashboard')}
            icon={
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6">
                <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 3v11.25A2.25 2.25 0 006 16.5h2.25M3.75 3h-1.5m1.5 0h16.5m0 0h1.5m-1.5 0v11.25A2.25 2.25 0 0118 16.5h-2.25m-7.5 0h7.5m-7.5 0l-1 3m8.5-3l1 3m0 0l.5 1.5m-.5-1.5h-10.5m10.5 0l.5 1.5m-1.5-1.5h-7.5M12 10.5V12m3-4.5V12m-6-1.5V12" />
              </svg>
            }
          />
        )}

        <SidebarItem
          label="Settings"
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.11.091.205.204.281.33.25.41.173.948-.19 1.27l1.296 2.247a1.125 1.125 0 01-1.37.49l-1.217-.456c-.354-.133-.751-.072-1.076.124a6.57 6.57 0 01-.22.127c-.332.183-.582.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.49l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.127.332-.184.582-.496.645-.869L9.594 3.94z" />
              <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          }
        />
      </nav>

      <div className="mt-auto pt-6 border-t border-border-primary/50 w-full flex flex-col items-center gap-4">
        <div className="w-8 h-8 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden ring-2 ring-indigo-500/20">
          <img src="https://api.dicebear.com/7.x/avataaars/svg?seed=Admin" alt="Profile" />
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;
