import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import api from '../../services/api';
import { motion, AnimatePresence } from 'framer-motion';
import { User, Mail, Lock, AlertCircle, ArrowRight, Fingerprint } from 'lucide-react';

const Register: React.FC = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
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
      const response = await api.post('/auth/register', { username, email, password });
      login(response.data.token, { 
        username: response.data.username, 
        email: response.data.email,
        role: response.data.role || 'User' 
      });

      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-bg-primary p-6 relative overflow-hidden font-['Inter']">
      {/* Background Ambience */}
      <div className="absolute top-[-10%] right-[-10%] w-[40%] h-[40%] bg-indigo-500/10 blur-[120px] rounded-full animate-pulse" />
      <div className="absolute bottom-[-10%] left-[-10%] w-[40%] h-[40%] bg-emerald-500/10 blur-[120px] rounded-full animate-pulse decoration-3000" />

      <motion.div 
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.6, ease: [0.23, 1, 0.32, 1] }}
        className="max-w-md w-full glass-panel p-10 md:p-12 rounded-[2rem] shadow-[0_32px_64px_rgba(0,0,0,0.2)] border border-white/10 relative z-10 backdrop-blur-3xl"
      >
        <div className="text-center mb-10">
          <motion.div 
            whileHover={{ scale: 1.1, rotate: 5 }}
            className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-emerald-500 to-indigo-600 text-white rounded-2xl mb-6 shadow-xl shadow-emerald-500/20"
          >
            <Fingerprint size={32} strokeWidth={2.5} />
          </motion.div>
          <h1 className="text-3xl font-black text-text-primary mb-2 tracking-tight">Create Account</h1>
          <p className="text-text-muted font-bold text-[11px] opacity-60">Join our financial intelligence community</p>
        </div>

        <AnimatePresence>
          {error && (
            <motion.div 
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
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
              <User className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors ${error ? 'text-rose-500' : 'text-text-muted group-focus-within:text-emerald-500'}`} size={18} />
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className={`w-full bg-bg-secondary/30 border rounded-xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 transition-all font-semibold text-text-primary placeholder:text-text-muted/30 ${
                  error 
                    ? 'border-rose-500/50 focus:ring-rose-500/10 bg-rose-500/5' 
                    : 'border-border-primary focus:ring-emerald-500/10 focus:border-emerald-500'
                }`}
                placeholder="Choose a username"
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className={`block text-[11px] font-bold uppercase tracking-wider ml-1 transition-colors duration-300 ${error ? 'text-rose-500' : 'text-text-muted'}`}>
              Email Address
            </label>
            <div className="relative group">
              <Mail className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors ${error ? 'text-rose-500' : 'text-text-muted group-focus-within:text-emerald-500'}`} size={18} />
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={`w-full bg-bg-secondary/30 border rounded-xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 transition-all font-semibold text-text-primary placeholder:text-text-muted/30 ${
                  error 
                    ? 'border-rose-500/50 focus:ring-rose-500/10 bg-rose-500/5' 
                    : 'border-border-primary focus:ring-emerald-500/10 focus:border-emerald-500'
                }`}
                placeholder="name@example.com"
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className={`block text-[11px] font-bold uppercase tracking-wider ml-1 transition-colors duration-300 ${error ? 'text-rose-500' : 'text-text-muted'}`}>
              Password
            </label>
            <div className="relative group">
              <Lock className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors ${error ? 'text-rose-500' : 'text-text-muted group-focus-within:text-emerald-500'}`} size={18} />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`w-full bg-bg-secondary/30 border rounded-xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 transition-all font-semibold text-text-primary placeholder:text-text-muted/30 ${
                  error 
                    ? 'border-rose-500/50 focus:ring-rose-500/10 bg-rose-500/5' 
                    : 'border-border-primary focus:ring-emerald-500/10 focus:border-emerald-500'
                }`}
                placeholder="At least 8 characters"
                required
              />
            </div>
          </div>

          <motion.button
            whileHover={{ y: -1 }}
            whileTap={{ scale: 0.99 }}
            type="submit"
            disabled={isSubmitting}
            className="w-full py-4 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-sm shadow-lg shadow-emerald-600/20 transition-all flex items-center justify-center gap-2 mt-8"
          >
            <span>{isSubmitting ? 'Creating account...' : 'Create Account'}</span>
            {!isSubmitting && <ArrowRight size={18} />}
          </motion.button>
        </form>

        <div className="mt-10 pt-8 border-t border-white/5 flex flex-col items-center gap-4">
          <p className="text-text-muted text-[11px] font-medium">
            Already have an account?
          </p>
          <Link to="/login" className="text-sm font-bold text-emerald-500 hover:text-emerald-400 transition-colors">
            Log In
          </Link>
        </div>
      </motion.div>
    </div>
  );
};

export default Register;
