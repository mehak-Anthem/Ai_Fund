import React, { useState, KeyboardEvent } from 'react';

interface ChatInputProps {
  onSend: (message: string) => void;
  disabled: boolean;
}

const ChatInput: React.FC<ChatInputProps> = ({ onSend, disabled }) => {
  const [input, setInput] = useState('');

  // Suggestion chips removed

  const handleSend = () => {
    if (input.trim() && !disabled) {
      onSend(input.trim());
      setInput('');
    }
  };

  const handleKeyPress = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };



  return (
    <div className="w-full flex justify-center pb-4">
      <div className="w-full max-w-3xl px-4">
        {/* Input Box */}
        <div className="relative group">
          <div className="absolute -inset-0.5 bg-gradient-to-r from-violet-500 to-cyan-500 rounded-3xl blur opacity-20 group-focus-within:opacity-40 transition duration-500"></div>
          <div className="relative flex items-center glass-panel rounded-3xl p-1.5 transition-all">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Message FundAI..."
              disabled={disabled}
              className="flex-1 py-3 px-5 text-[15px] text-slate-900 placeholder-slate-400 outline-none bg-transparent"
            />
            <button
              onClick={handleSend}
              disabled={disabled || !input.trim()}
              className="w-10 h-10 md:w-11 md:h-11 bg-gradient-to-r from-violet-500 to-cyan-500 hover:shadow-lg disabled:opacity-50 disabled:grayscale text-white rounded-2xl flex items-center justify-center transition-all hover:scale-105"
            >
              <span className="text-lg">➤</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatInput;
