import { useState, useEffect, useRef } from 'react';
import Layout from './components/Layout';
import Dashboard from './components/Dashboard';
import ChatMessage from './components/ChatMessage';
import TypingIndicator from './components/TypingIndicator';
import EmptyState from './components/EmptyState';
import ChatInput from './components/ChatInput';
import { Message, ChatResponse } from './types';
import api from './services/api';
import { useAuth } from './context/AuthContext';

function MainApp() {
  const { user } = useAuth();
  const [currentView, setCurrentView] = useState<'chat' | 'dashboard'>('chat');
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const chatAreaRef = useRef<HTMLDivElement>(null);

  const isAdmin = user?.role === 'Admin';

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
      const response = await api.get(`/MutualFund/ask?query=${encodeURIComponent(content)}`);
      const data: ChatResponse = response.data;

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
        content: 'I encountered an error. Please try again.',
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
    <Layout
      currentView={currentView}
      onViewChange={setCurrentView}
      isAdmin={isAdmin}
      username={user?.username || 'Guest'}
    >
      <div className="h-full flex flex-col relative">
        {currentView === 'dashboard' && isAdmin ? (
          <div className="flex-1 overflow-y-auto p-8 scrollbar-hide">
             <Dashboard />
          </div>
        ) : (
          <div className="flex-1 flex flex-col overflow-hidden">
            <div
              ref={chatAreaRef}
              className="flex-1 overflow-y-auto px-4 md:px-12 py-8 scrollbar-hide"
            >
              <div className="max-w-3xl mx-auto space-y-8 pb-32">
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

            <div className="absolute bottom-0 left-0 right-0 p-8 z-20 pointer-events-none">
              <div className="max-w-3xl mx-auto pointer-events-auto">
                <ChatInput onSend={handleSendMessage} disabled={isTyping} />
                <p className="text-[10px] text-center text-text-muted mt-4 font-medium opacity-60">
                  FundAI can provide helpful insights, but always verify critical financial information.
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default MainApp;
