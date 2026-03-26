import React from 'react';

const EmptyState: React.FC = () => {
  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh] text-center px-4 animate-fadeIn">
      <div className="w-24 h-24 rounded-[32px] bg-indigo-500/10 flex items-center justify-center text-5xl mb-10 shadow-indigo-500/5 ring-1 ring-indigo-500/20">
        ✨
      </div>
      <h1 className="text-4xl font-extrabold text-text-primary mb-6 tracking-tight">
        How can I help you today?
      </h1>
      <p className="text-text-secondary text-lg mb-10 max-w-lg leading-relaxed font-medium">
        I'm your intelligent investment companion. Ask me anything about mutual funds, SIP strategies, or market trends.
      </p>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 w-full max-w-2xl">
        <SuggestionPill label="Best SIP for 5 years?" />
        <SuggestionPill label="Compare FD vs Mutual Funds" />
        <SuggestionPill label="What is current NIFTY rate?" />
        <SuggestionPill label="Explain Tax Saving Funds" />
      </div>
    </div>
  );
};

const SuggestionPill: React.FC<{ label: string }> = ({ label }) => (
  <button className="px-5 py-4 rounded-2xl bg-bg-secondary border border-border-primary text-sm font-bold text-text-secondary hover:border-indigo-500/30 hover:text-indigo-500 hover:bg-indigo-500/5 transition-all duration-300 text-left flex items-center justify-between group">
    {label}
    <span className="opacity-0 group-hover:opacity-100 transition-opacity transform translate-x-2 group-hover:translate-x-0">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-4 h-4">
        <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 19.5l15-15m0 0H8.25m11.25 0v11.25" />
      </svg>
    </span>
  </button>
);

export default EmptyState;
