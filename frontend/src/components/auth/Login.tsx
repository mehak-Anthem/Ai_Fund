import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import api from '../../services/api';
import { motion, AnimatePresence } from 'framer-motion';
import { Lock, User, AlertCircle, ArrowRight, ShieldCheck } from 'lucide-react';

const Login: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const response = await api.post('/auth/login', { username, password });
      login(response.data.token, { 
        username: response.data.username, 
        email: response.data.email,
        role: response.data.role 
      });

      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid username or password');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-bg-primary p-6 relative overflow-hidden font-['Inter']">
      {/* Dynamic Background Elements */}
      <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-500/10 blur-[120px] rounded-full animate-pulse" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-violet-500/10 blur-[120px] rounded-full animate-pulse decoration-3000" />

      <motion.div 
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6, ease: [0.23, 1, 0.32, 1] }}
        className="max-w-md w-full glass-panel p-10 md:p-12 rounded-[2rem] shadow-[0_32px_64px_rgba(0,0,0,0.2)] border border-white/10 relative z-10 backdrop-blur-3xl"
      >
        <div className="text-center mb-10">
          <motion.div 
            initial={{ scale: 0.8, rotate: -10 }}
            animate={{ scale: 1, rotate: 0 }}
            className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-indigo-500 to-violet-600 text-white rounded-2xl mb-6 shadow-xl shadow-indigo-500/20"
          >
            <ShieldCheck size={32} strokeWidth={2.5} />
          </motion.div>
          <h1 className="text-3xl font-black text-text-primary mb-2 tracking-tight">Welcome Back</h1>
          <p className="text-text-muted font-bold text-[11px] opacity-60">Sign in to your account to continue</p>
        </div>

        <AnimatePresence mode="wait">
          {error && (
            <motion.div 
              initial={{ opacity: 0, height: 0, y: -10 }}
              animate={{ opacity: 1, height: 'auto', y: 0 }}
              exit={{ opacity: 0, height: 0, y: -10 }}
              className="mb-8 p-4 bg-rose-500/10 border border-rose-500/20 text-rose-500 rounded-xl flex items-center gap-3 text-[13px] font-bold"
            >
              <AlertCircle size={18} />
              {error}
            </motion.div>
          )}
        </AnimatePresence>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <label className={`block text-[11px] font-bold uppercase tracking-wider ml-1 transition-colors duration-300 ${error ? 'text-rose-500' : 'text-text-muted'}`}>
              Username
            </label>
            <div className="relative group">
              <User className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors ${error ? 'text-rose-500' : 'text-text-muted group-focus-within:text-indigo-500'}`} size={18} />
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={`w-full bg-bg-secondary/30 border rounded-xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 transition-all font-semibold text-text-primary placeholder:text-text-muted/30 ${
                  error 
                    ? 'border-rose-500/50 focus:ring-rose-500/10 bg-rose-500/5' 
                    : 'border-border-primary focus:ring-indigo-500/10 focus:border-indigo-500'
                }`}
                placeholder="Enter username"
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <div className="flex justify-between items-center ml-1">
              <label className={`block text-[11px] font-bold uppercase tracking-wider transition-colors duration-300 ${error ? 'text-rose-500' : 'text-text-muted'}`}>
                Password
              </label>
              <button type="button" className="text-[10px] font-bold text-indigo-500 uppercase tracking-wider hover:text-indigo-400 transition-colors">
                Forgot?
              </button>
            </div>
            <div className="relative group">
              <Lock className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors ${error ? 'text-rose-500' : 'text-text-muted group-focus-within:text-indigo-500'}`} size={18} />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`w-full bg-bg-secondary/30 border rounded-xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 transition-all font-semibold text-text-primary placeholder:text-text-muted/30 ${
                  error 
                    ? 'border-rose-500/50 focus:ring-rose-500/10 bg-rose-500/5' 
                    : 'border-border-primary focus:ring-indigo-500/10 focus:border-indigo-500'
                }`}
                placeholder="••••••••"
                required
              />
            </div>
          </div>

          <motion.button
            whileHover={{ y: -1 }}
            whileTap={{ scale: 0.99 }}
            type="submit"
            disabled={isSubmitting}
            className="w-full py-4 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl font-bold text-sm shadow-lg shadow-indigo-600/20 transition-all flex items-center justify-center gap-2 mt-8"
          >
            <span>{isSubmitting ? 'Signing in...' : 'Sign In'}</span>
            {!isSubmitting && <ArrowRight size={18} />}
          </motion.button>
        </form>

        <div className="mt-10 pt-8 border-t border-white/5 flex flex-col items-center gap-4">
          <p className="text-text-muted text-[11px] font-medium">
            Don't have an account?
          </p>
          <Link to="/register" className="text-sm font-bold text-indigo-500 hover:text-indigo-400 transition-colors">
            Create Account
          </Link>
        </div>
      </motion.div>
    </div>
  );
};
export default Login;
