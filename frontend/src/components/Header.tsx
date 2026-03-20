import React from 'react';

interface HeaderProps {
  currentView: 'chat' | 'dashboard';
  onViewChange: (view: 'chat' | 'dashboard') => void;
}

const Header: React.FC<HeaderProps> = ({ currentView, onViewChange }) => {
  return (
    <div className="glass-panel border-b-0">
      <div className="flex items-center justify-between max-w-7xl mx-auto px-6 py-4">
        {/* Logo */}
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl flex items-center justify-center text-xl bg-gradient-to-br from-violet-500 to-cyan-500 shadow-lg shadow-violet-500/20 animate-pulse-slow">
            ✨
          </div>
          <span className="text-xl font-bold tracking-tight text-slate-900">
            Fund<span className="text-gradient">AI</span>
          </span>
        </div>

        {/* Navigation */}
        <div className="flex items-center gap-1.5 bg-slate-100/80 p-1.5 rounded-2xl border border-slate-200">
          <button
            onClick={() => onViewChange('chat')}
            className={`px-5 py-2.5 rounded-xl text-sm font-semibold transition-all duration-300 ${
              currentView === 'chat'
                ? 'bg-white text-slate-900 shadow-sm border border-slate-200/50'
                : 'text-slate-500 hover:text-slate-700 hover:bg-black/5'
            }`}
          >
            <span className="flex items-center gap-2">
              <span className="text-lg">💭</span> Chat
            </span>
          </button>
          <button
            onClick={() => onViewChange('dashboard')}
            className={`px-5 py-2.5 rounded-xl text-sm font-semibold transition-all duration-300 ${
              currentView === 'dashboard'
                ? 'bg-white text-slate-900 shadow-sm border border-slate-200/50'
                : 'text-slate-500 hover:text-slate-700 hover:bg-black/5'
            }`}
          >
            <span className="flex items-center gap-2">
              <span className="text-lg">📊</span> Dashboard
            </span>
          </button>
        </div>

        {/* User Menu */}
        <div className="flex items-center gap-4">
          <button className="w-10 h-10 flex items-center justify-center text-slate-500 hover:text-slate-900 hover:bg-slate-100 rounded-xl transition-all duration-300">
            <span className="text-xl">🔔</span>
          </button>
          <button className="w-10 h-10 flex items-center justify-center text-slate-500 hover:text-slate-900 hover:bg-slate-100 rounded-xl transition-all duration-300">
            <span className="text-xl">⚙️</span>
          </button>
          <div className="w-10 h-10 rounded-xl flex items-center justify-center shadow-md cursor-pointer hover:scale-105 transition-transform bg-gradient-to-br from-slate-800 to-slate-900 border border-slate-700">
            <span className="font-bold text-white">M</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Header;
