import React from 'react';
import { Message } from '../types';

interface ChatMessageProps {
  message: Message;
  onCopy: (content: string) => void;
  onRegenerate: () => void;
}

const ChatMessage: React.FC<ChatMessageProps> = ({ message, onCopy, onRegenerate }) => {
  const formatTime = (date: Date) => {
    return new Date(date).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div
      className={`flex mb-6 animate-fadeIn ${
        message.isUser ? 'justify-end' : 'justify-start'
      }`}
    >
      <div className={`max-w-[75%] ${message.isUser ? 'items-end' : 'items-start'} flex flex-col`}>
        {/* Message Bubble */}
        <div
          className={`px-5 py-4 rounded-2xl text-[15px] leading-relaxed shadow-sm ${
            message.isUser
              ? 'bg-gradient-to-r from-violet-500 to-cyan-500 text-white rounded-tr-sm shadow-md shadow-violet-500/20'
              : 'glass-panel text-slate-800 rounded-tl-sm'
          }`}
        >
          {message.content}
        </div>

        {/* Metadata (AI only) */}
        {!message.isUser && (
          <>
            <div className="flex items-center gap-3 mt-3 ml-2 text-xs text-slate-400">
              {message.source && (
                <span className="px-3 py-1 bg-slate-100/80 border border-slate-200/50 rounded-full text-slate-600 font-medium">
                  {message.source} {message.confidence && `${Math.round(message.confidence * 100)}%`}
                </span>
              )}
              <span>{formatTime(message.timestamp)}</span>
            </div>

            {/* Actions */}
            <div className="flex gap-2 mt-2 ml-2">
              <button
                onClick={() => onCopy(message.content)}
                className="px-3 py-1.5 text-xs bg-white/60 border border-slate-200/50 rounded-lg text-slate-500 hover:bg-slate-50 hover:text-slate-800 transition-all duration-300 shadow-sm"
              >
                📋 Copy
              </button>
              <button
                onClick={onRegenerate}
                className="px-3 py-1.5 text-xs bg-white/60 border border-slate-200/50 rounded-lg text-slate-500 hover:bg-slate-50 hover:text-slate-800 transition-all duration-300 shadow-sm"
              >
                🔄 Regenerate
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default ChatMessage;
