import React from 'react';

const TypingIndicator: React.FC = () => {
  return (
    <div className="flex mb-6 animate-fadeIn">
      <div className="flex gap-1.5 px-5 py-4 glass-panel rounded-2xl rounded-bl-sm">
        <div className="w-2 h-2 bg-violet-500 rounded-full animate-bounce shadow-[0_0_8px_rgba(139,92,246,0.3)]" style={{ animationDelay: '0ms' }} />
        <div className="w-2 h-2 bg-gradient-to-r from-violet-500 to-cyan-500 rounded-full animate-bounce shadow-[0_0_8px_rgba(6,182,212,0.3)]" style={{ animationDelay: '150ms' }} />
        <div className="w-2 h-2 bg-cyan-500 rounded-full animate-bounce shadow-[0_0_8px_rgba(6,182,212,0.3)]" style={{ animationDelay: '300ms' }} />
      </div>
    </div>
  );
};

export default TypingIndicator;
