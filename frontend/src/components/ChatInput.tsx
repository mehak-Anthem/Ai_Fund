import React, { useState } from 'react';

interface ChatInputProps {
  onSend: (content: string) => void;
  disabled: boolean;
}

const ChatInput: React.FC<ChatInputProps> = ({ onSend, disabled }) => {
  const [content, setContent] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (content.trim() && !disabled) {
      onSend(content.trim());
      setContent('');
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className={`glass-panel p-2 rounded-[28px] flex items-center gap-2 shadow-2xl transition-all duration-500 group ${
        disabled ? 'opacity-50 grayscale' : 'hover:shadow-indigo-500/10'
      }`}
    >
      <div className="flex-1 relative">
        <input
          type="text"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          disabled={disabled}
          placeholder="Ask FundAI anything..."
          className="w-full bg-transparent border-none focus:ring-0 px-6 py-4 text-[15px] font-medium text-text-primary placeholder:text-text-muted transition-all"
        />
      </div>

      <button
        type="submit"
        disabled={disabled || !content.trim()}
        className={`w-12 h-12 rounded-2xl flex items-center justify-center transition-all duration-300 shadow-lg ${
          content.trim() && !disabled
            ? 'bg-indigo-600 text-white shadow-indigo-500/20 scale-100 hover:scale-105 hover:shadow-indigo-500/40 active:scale-95'
            : 'bg-bg-secondary text-text-muted scale-95'
        }`}
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-5 h-5">
          <path strokeLinecap="round" strokeLinejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
        </svg>
      </button>
    </form>
  );
};

export default ChatInput;
