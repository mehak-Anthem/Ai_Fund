import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { motion } from 'framer-motion';

interface DashboardStats {
  totalQueries: number;
  knowledgeGaps: number;
  avgConfidence: number;
  activeUsers: number;
}

interface KnowledgeGap {
  question: string;
  occurrenceCount: number;
  confidenceScore: number;
  status: string;
  lastAsked: string;
}

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats>({
    totalQueries: 0,
    knowledgeGaps: 0,
    avgConfidence: 0,
    activeUsers: 0,
  });
  const [gaps, setGaps] = useState<KnowledgeGap[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadDashboardData();
    const interval = setInterval(loadDashboardData, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadDashboardData = async () => {
    try {
      const response = await api.get('/KnowledgeGap/dashboard');
      const data = response.data;
      setStats({
        totalQueries: data.totalLogs || 0,
        knowledgeGaps: data.totalGaps || 0,
        avgConfidence: Math.round(data.avgConfidence * 100) || 0,
        activeUsers: data.activeUsers || 0,
      });
      setGaps(data.topMissingQuestions || []);
      setLoading(false);
    } catch (error) {
      console.error('Error loading dashboard:', error);
      setLoading(false);
    }
  };

  const syncQdrant = async () => {
    try {
      const response = await api.post('/KnowledgeGap/sync-to-qdrant');
      if (response.status === 200) {
        alert('✅ Qdrant synced successfully!');
        loadDashboardData();
      }
    } catch (error) {
      alert('❌ Sync failed');
    }
  };

  const filteredGaps = gaps.filter((gap) =>
    gap.question.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'New': return 'bg-amber-500/10 text-amber-500 border-amber-500/20';
      case 'Reviewing': return 'bg-indigo-500/10 text-indigo-500 border-indigo-500/20';
      case 'Resolved': return 'bg-emerald-500/10 text-emerald-500 border-emerald-500/20';
      default: return 'bg-text-muted/10 text-text-muted border-text-muted/20';
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh]">
        <div className="w-12 h-12 border-2 border-indigo-500/20 border-t-indigo-500 rounded-full animate-spin mb-4" />
        <p className="text-text-muted font-bold tracking-widest text-[10px] uppercase">Intelligence Loading...</p>
      </div>
    );
  }

  return (
    <div className="animate-fadeIn">
      {/* Search & Actions Bar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-10">
        <div className="relative group flex-1 max-w-md">
          <input
            type="text"
            placeholder="Search knowledge gaps..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-bg-secondary border border-border-primary rounded-2xl text-sm font-medium focus:ring-2 focus:ring-indigo-500/20 transition-all outline-none"
          />
          <span className="absolute left-4 top-1/2 -translate-y-1/2 text-text-muted group-focus-within:text-indigo-500 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5">
              <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
            </svg>
          </span>
        </div>
        <button
          onClick={syncQdrant}
          className="px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold text-sm shadow-lg shadow-indigo-500/20 hover:shadow-indigo-500/40 hover:scale-105 active:scale-95 transition-all flex items-center gap-2"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-4 h-4">
            <path strokeLinecap="round" strokeLinejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0l3.181 3.183a8.25 8.25 0 0013.803-3.7M4.031 9.865a8.25 8.25 0 0113.803-3.7l3.181 3.182m0 0a8.25 8.25 0 01-13.803 3.7l3.181-3.182" />
          </svg>
          Sync Intelligence
        </button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
        <StatCard title="Total Interactions" value={stats.totalQueries} detail="+12.5% vs LW" color="indigo" />
        <StatCard title="Critical Gaps" value={stats.knowledgeGaps} detail="Requires Action" color="amber" />
        <StatCard title="Avg Confidence" value={`${stats.avgConfidence}%`} detail="Target: 95%" color="emerald" />
        <StatCard title="Active Users" value={stats.activeUsers} detail="Real-time" color="blue" />
      </div>

      {/* Main Table */}
      <div className="premium-card overflow-hidden bg-bg-surface">
        <div className="px-8 py-6 border-b border-border-primary flex items-center justify-between">
          <h2 className="text-sm font-bold text-text-primary uppercase tracking-widest flex items-center gap-3">
            <span className="w-2 h-2 rounded-full bg-indigo-500" />
            Active Knowledge Gaps
          </h2>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-bg-secondary/50 border-b border-border-primary">
                <th className="px-10 py-5 text-[10px] font-bold text-text-muted uppercase tracking-widest">Question Segment</th>
                <th className="px-10 py-5 text-[10px] font-bold text-text-muted uppercase tracking-widest">Confidence</th>
                <th className="px-10 py-5 text-[10px] font-bold text-text-muted uppercase tracking-widest">Status</th>
                <th className="px-10 py-5 text-[10px] font-bold text-text-muted uppercase tracking-widest text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border-primary/30">
              {filteredGaps.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-10 py-20 text-center opacity-40 italic text-sm">
                    No intelligence gaps detected in the system.
                  </td>
                </tr>
              ) : (
                filteredGaps.map((gap, index) => (
                  <motion.tr 
                    key={index}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-bg-secondary/40 transition-colors group"
                  >
                    <td className="px-10 py-6">
                      <div className="flex flex-col">
                        <span className="text-sm font-bold text-text-primary mb-1 group-hover:text-indigo-500 transition-colors">{gap.question}</span>
                        <span className="text-[10px] text-text-muted font-medium">Asked {gap.occurrenceCount} times • {new Date(gap.lastAsked).toLocaleDateString()}</span>
                      </div>
                    </td>
                    <td className="px-10 py-6">
                      <div className="flex items-center gap-3">
                        <div className="w-24 h-1.5 bg-bg-secondary rounded-full overflow-hidden border border-border-primary">
                          <div 
                            className={`h-full transition-all duration-1000 ${gap.confidenceScore < 0.5 ? 'bg-amber-500' : 'bg-emerald-500'}`} 
                            style={{ width: `${gap.confidenceScore * 100}%` }} 
                          />
                        </div>
                        <span className="text-[11px] font-bold text-text-secondary">{Math.round(gap.confidenceScore * 100)}%</span>
                      </div>
                    </td>
                    <td className="px-10 py-6">
                      <span className={`px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border ${getStatusColor(gap.status)}`}>
                        {gap.status}
                      </span>
                    </td>
                    <td className="px-10 py-6 text-right">
                      <button className="px-4 py-2 rounded-xl text-[10px] font-bold text-text-primary bg-bg-primary border border-border-primary hover:border-indigo-500/40 hover:text-indigo-500 transition-all active:scale-95">
                        REFINE AI
                      </button>
                    </td>
                  </motion.tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

const StatCard: React.FC<{ title: string; value: string | number; detail: string; color: string }> = ({ title, value, detail, color }) => {
  const colors: Record<string, string> = {
    indigo: 'text-indigo-500 bg-indigo-500/5 ring-indigo-500/20',
    amber: 'text-amber-500 bg-amber-500/5 ring-amber-500/20',
    emerald: 'text-emerald-500 bg-emerald-500/5 ring-emerald-500/20',
    blue: 'text-blue-500 bg-blue-500/5 ring-blue-500/20',
  };

  return (
    <div className="premium-card p-6 bg-bg-surface group hover:translate-y-[-4px]">
      <div className="text-[10px] font-bold text-text-muted uppercase tracking-[0.2em] mb-4">{title}</div>
      <div className="text-3xl font-black text-text-primary tracking-tighter mb-2">{value}</div>
      <div className={`inline-block px-2 py-0.5 rounded-lg text-[10px] font-bold ring-1 ${colors[color]}`}>
        {detail}
      </div>
    </div>
  );
};

export default Dashboard;
