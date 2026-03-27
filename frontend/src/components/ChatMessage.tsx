import React from 'react';
import { motion } from 'framer-motion';
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
    <motion.div
      initial={{ opacity: 0, y: 10, scale: 0.98 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      transition={{ duration: 0.4, ease: [0.23, 1, 0.32, 1] }}
      className={`flex mb-8 ${message.isUser ? 'justify-end' : 'justify-start'}`}
    >
      <div className={`max-w-[85%] md:max-w-[70%] ${message.isUser ? 'items-end' : 'items-start'} flex flex-col`}>
        {/* Message Bubble */}
        <div
          className={`px-5 py-4 rounded-[28px] text-[15px] leading-relaxed relative ${
            message.isUser
              ? 'bg-indigo-600 text-white rounded-tr-[4px] shadow-lg shadow-indigo-500/10'
              : 'bg-bg-secondary border border-border-primary text-text-primary rounded-tl-[4px]'
          }`}
        >
          <div className="whitespace-pre-wrap font-medium">{message.content}</div>
          
          {/* Subtle Ambient Glow for User Messages */}
          {message.isUser && (
            <div className="absolute inset-0 bg-indigo-400/10 blur-2xl rounded-full -z-10 scale-110 opacity-50" />
          )}
        </div>

        {/* Metadata & Actions */}
        {!message.isUser && (
          <div className="flex flex-col gap-3 mt-3 ml-2">
            <div className="flex items-center gap-3 text-[10px] font-bold uppercase tracking-widest text-text-muted">
              <span>{formatTime(message.timestamp)}</span>
            </div>

            {/* Actions */}
            <div className="flex gap-2">
              <ActionButton onClick={() => onCopy(message.content)} icon="📋" label="Copy" />
              <ActionButton onClick={onRegenerate} icon="🔄" label="Regenerate" />
            </div>
          </div>
        )}

        {message.isUser && (
          <div className="mt-2 mr-2 text-[10px] font-bold text-text-muted uppercase tracking-widest opacity-60">
            {formatTime(message.timestamp)}
          </div>
        )}
      </div>
    </motion.div>
  );
};

const ActionButton: React.FC<{ onClick: () => void; icon: string; label: string }> = ({ onClick, icon, label }) => (
  <button
    onClick={onClick}
    className="flex items-center gap-1.5 px-3 py-1.5 text-[11px] font-bold bg-bg-primary border border-border-primary rounded-xl text-text-secondary hover:text-text-primary hover:border-indigo-500/30 transition-all duration-300 shadow-sm active:scale-95"
  >
    <span>{icon}</span> {label}
  </button>
);

export default ChatMessage;
