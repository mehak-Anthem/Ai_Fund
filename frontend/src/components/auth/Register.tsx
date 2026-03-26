import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import api from '../../services/api';
import { UserPlus, User, Mail, Lock, AlertCircle, ArrowRight } from 'lucide-react';

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
    <div className="min-h-screen flex items-center justify-center bg-whiteboard p-4 transition-colors duration-300">
      <div className="max-w-md w-full glass-panel p-10 rounded-[32px] shadow-2xl">
        <div className="text-center mb-10">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-indigo-500/10 text-indigo-600 rounded-3xl mb-6 ring-1 ring-indigo-500/20">
            <UserPlus size={36} />
          </div>
          <h1 className="text-4xl font-black text-text-primary mb-3 tracking-tight">Join FundAI</h1>
          <p className="text-text-secondary font-medium uppercase tracking-widest text-[10px]">Create Your Intelligence Profile</p>
        </div>


        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-100 text-red-600 rounded-2xl flex items-center gap-3 text-sm">
            <AlertCircle size={18} />
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-[11px] font-bold text-text-muted uppercase tracking-widest mb-2 ml-1">Username</label>
            <div className="relative group">
              <User className="absolute left-4 top-1/2 -translate-y-1/2 text-text-muted group-focus-within:text-indigo-500 transition-colors" size={18} />
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full bg-bg-secondary border border-border-primary rounded-2xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 transition-all font-medium text-text-primary"
                placeholder="Choose a professional username"
                required
              />
            </div>
          </div>


          <div>
            <label className="block text-[11px] font-bold text-text-muted uppercase tracking-widest mb-2 ml-1">Email</label>
            <div className="relative group">
              <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-text-muted group-focus-within:text-indigo-500 transition-colors" size={18} />
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-bg-secondary border border-border-primary rounded-2xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 transition-all font-medium text-text-primary"
                placeholder="name@example.com"
                required
              />
            </div>
          </div>


          <div>
            <label className="block text-[11px] font-bold text-text-muted uppercase tracking-widest mb-2 ml-1">Secure Password</label>
            <div className="relative group">
              <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-text-muted group-focus-within:text-indigo-500 transition-colors" size={18} />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-bg-secondary border border-border-primary rounded-2xl py-4 pl-12 pr-4 focus:outline-none focus:ring-4 focus:ring-indigo-500/10 focus:border-indigo-500 transition-all font-medium text-text-primary"
                placeholder="••••••••"
                required
              />
            </div>
          </div>


          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full py-4 bg-indigo-600 text-white rounded-2xl font-bold text-sm shadow-lg shadow-indigo-500/20 hover:shadow-indigo-500/40 hover:translate-y-[-2px] active:translate-y-[0px] transition-all flex items-center justify-center gap-2 group mt-4 mb-2"
          >
            {isSubmitting ? 'Initializing Account...' : 'Get Instant Access'}
            {!isSubmitting && <ArrowRight size={18} className="group-hover:translate-x-1 transition-transform" />}
          </button>

        </form>

        <p className="text-center mt-8 text-text-muted text-xs font-bold uppercase tracking-widest">
          Already a member?{' '}
          <Link to="/login" className="text-indigo-600 hover:text-indigo-500 transition-colors">
            Sign In
          </Link>
        </p>

      </div>
    </div>
  );
};

export default Register;
