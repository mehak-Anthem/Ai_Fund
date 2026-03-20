import React from 'react';

const EmptyState: React.FC = () => {
    // Shortcut buttons removed

  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
      <div className="text-6xl mb-6">👋</div>
      <h1 className="text-3xl font-bold text-slate-900 mb-3">
        Hello! I'm FundAI
      </h1>
      <p className="text-slate-600 text-base mb-8 max-w-md">
        Your intelligent financial assistant. Ask me anything about mutual funds, SIP, investments, and returns.
      </p>

    </div>
  );
};

export default EmptyState;
