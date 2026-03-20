import { useState, useEffect, useRef } from 'react';
import Header from './components/Header';
import Dashboard from './components/Dashboard';
import ChatMessage from './components/ChatMessage';
import TypingIndicator from './components/TypingIndicator';
import EmptyState from './components/EmptyState';
import ChatInput from './components/ChatInput';
import { Message, ChatResponse } from './types';

const API_BASE = import.meta.env.VITE_API_URL || 'https://localhost:44328';
const API_URL = `${API_BASE}/api/MutualFund/ask`;
const USER_ID = '1';

function App() {
  const [currentView, setCurrentView] = useState<'chat' | 'dashboard'>('chat');
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const chatAreaRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    scrollToBottom();
  }, [messages, isTyping]);

  const scrollToBottom = () => {
    if (chatAreaRef.current) {
      chatAreaRef.current.scrollTop = chatAreaRef.current.scrollHeight;
    }
  };

  const handleSendMessage = async (content: string) => {
    const userMessage: Message = {
      id: Date.now().toString(),
      content,
      isUser: true,
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsTyping(true);

    try {
      const response = await fetch(
        `${API_URL}?query=${encodeURIComponent(content)}&userId=${USER_ID}`
      );

      if (!response.ok) throw new Error('Network error');

      const data: ChatResponse = await response.json();

      const aiMessage: Message = {
        id: (Date.now() + 1).toString(),
        content: data.answer,
        isUser: false,
        timestamp: new Date(),
        source: data.source,
        confidence: data.confidence,
      };

      setMessages((prev) => [...prev, aiMessage]);
    } catch (error) {
      console.error('Error:', error);
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        content: 'Sorry, I encountered an error. Please try again.',
        isUser: false,
        timestamp: new Date(),
      };
      setMessages((prev) => [...prev, errorMessage]);
    } finally {
      setIsTyping(false);
    }
  };

  const handleCopy = (content: string) => {
    navigator.clipboard.writeText(content);
  };

  const handleRegenerate = () => {
    const lastUserMessage = messages.filter((m) => m.isUser).pop();
    if (lastUserMessage) {
      handleSendMessage(lastUserMessage.content);
    }
  };

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-[#f8fafc] relative">
      {/* Ambient glowing orbs for AI vibe (Light Mode) */}
      <div className="absolute top-0 left-1/4 w-96 h-96 bg-violet-500/5 rounded-full blur-[120px] pointer-events-none"></div>
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-cyan-500/5 rounded-full blur-[150px] pointer-events-none"></div>

      {/* Header with Navigation */}
      <div className="z-10">
        <Header currentView={currentView} onViewChange={setCurrentView} />
      </div>

      {/* Main Content */}
      {currentView === 'dashboard' ? (
        <Dashboard />
      ) : (
        <div className="flex-1 flex flex-col overflow-hidden z-10">
          {/* Chat Area */}
          <div
            ref={chatAreaRef}
            className="flex-1 overflow-y-auto px-4 md:px-8 py-8 scrollbar-hide"
          >
            <div className="max-w-3xl mx-auto space-y-6 pb-20">
              {messages.length === 0 ? (
                <EmptyState />
              ) : (
                <>
                  {messages.map((message) => (
                    <ChatMessage
                      key={message.id}
                      message={message}
                      onCopy={handleCopy}
                      onRegenerate={handleRegenerate}
                    />
                  ))}
                  {isTyping && <TypingIndicator />}
                </>
              )}
            </div>
          </div>

          {/* Input Area */}
          <div className="z-20 p-4 bg-gradient-to-t from-[#f8fafc] via-[#f8fafc] to-transparent pb-6">
            <ChatInput onSend={handleSendMessage} disabled={isTyping} />
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
