import React from 'react';
import Sidebar from './Sidebar';
import InsightPanel from './InsightPanel';
import ThemeToggle from './ThemeToggle';

interface LayoutProps {
  children: React.ReactNode;
  currentView: 'chat' | 'dashboard';
  onViewChange: (view: 'chat' | 'dashboard') => void;
  isAdmin: boolean;
  username: string;
}

const Layout: React.FC<LayoutProps> = ({ children, currentView, onViewChange, isAdmin, username }) => {
  return (
    <div className="flex h-screen w-full bg-bg-primary overflow-hidden font-['Inter'] transition-colors duration-300">
      {/* Sidebar - Positioned fixed/absolute by itself */}
      <Sidebar currentView={currentView} onViewChange={onViewChange} isAdmin={isAdmin} />

      {/* Main Content Area */}
      <main className="flex-1 flex flex-col relative ml-24 h-full overflow-hidden bg-whiteboard">
        {/* Top Navigation / Header */}
        <header className="h-20 flex items-center justify-between px-10 z-30">
          <div className="flex items-center gap-3">
            <h2 className="text-lg font-bold text-text-primary capitalize">
              {currentView === 'chat' ? 'Intelligent Assistant' : 'Admin Control Center'}
            </h2>
          </div>
          
          <div className="flex items-center gap-4">
            <div className="hidden md:flex flex-col items-end mr-2">
              <span className="text-xs font-bold text-text-primary">{username}</span>
              <span className="text-[10px] text-text-muted">{isAdmin ? 'Administrator' : 'Premium Member'}</span>
            </div>
            <ThemeToggle />
          </div>
        </header>

        {/* Dynamic Content */}
        <div className="flex-1 overflow-hidden relative">
          {children}
        </div>
      </main>

      {/* Insight Panel - Right Side */}
      <InsightPanel />
    </div>
  );
};

export default Layout;
