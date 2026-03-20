import React, { useState, useEffect } from 'react';

const API_BASE = import.meta.env.VITE_API_URL || 'https://localhost:44328';

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
    const interval = setInterval(loadDashboardData, 30000); // Refresh every 30s
    return () => clearInterval(interval);
  }, []);

  const loadDashboardData = async () => {
    try {
      const response = await fetch(`${API_BASE}/api/KnowledgeGap/dashboard`);
      const data = await response.json();

      setStats({
        totalQueries: data.totalGaps || 0,
        knowledgeGaps: data.summary?.newGaps || 0,
        avgConfidence: 87,
        activeUsers: 156,
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
      const response = await fetch(`${API_BASE}/api/KnowledgeGap/sync-to-qdrant`, {
        method: 'POST',
      });
      if (response.ok) {
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
      case 'New':
        return 'bg-orange-500/20 text-orange-400 border border-orange-500/30';
      case 'Reviewing':
        return 'bg-blue-500/20 text-blue-400 border border-blue-500/30';
      case 'Resolved':
        return 'bg-green-500/20 text-green-400 border border-green-500/30';
      default:
        return 'bg-slate-500/20 text-slate-400 border border-slate-500/30';
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen bg-[#f8fafc]">
        <div className="w-16 h-16 border-4 border-slate-200 border-t-primary rounded-full animate-spin mb-4"></div>
        <p className="text-slate-600 font-medium tracking-wide">Initializing Dashboard...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#f8fafc] relative z-10 overflow-auto">
      {/* Header */}
      <div className="glass-panel px-8 py-6 mb-8 mt-4 mx-8 rounded-3xl">
        <div className="flex items-center justify-between max-w-7xl mx-auto">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-slate-900 mb-2">Dashboard Overview</h1>
            <p className="text-sm text-slate-500">Monitor your AI assistant performance in real-time</p>
          </div>
          <div className="flex gap-4">
            <button
              onClick={syncQdrant}
              className="px-5 py-2.5 glass-card text-slate-700 rounded-xl font-semibold hover:text-slate-900 hover:shadow-md transition-all flex items-center gap-2"
            >
              <span className="text-lg">🔄</span> Sync Qdrant
            </button>
            <button
              onClick={() => (window.location.href = '/chat')}
              className="px-5 py-2.5 bg-gradient-to-r from-violet-500 to-cyan-500 text-white rounded-xl font-semibold hover:shadow-lg hover:shadow-violet-500/20 hover:scale-105 transition-all flex items-center gap-2"
            >
              <span className="text-lg">💬</span> Open UI
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-8 pb-12">
        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard icon="💬" title="Total Queries" value={stats.totalQueries} change="+12%" positive color="purple" />
          <StatCard icon="⚡" title="Knowledge Gaps" value={stats.knowledgeGaps} change="-8%" positive color="orange" />
          <StatCard icon="🎯" title="Avg Confidence" value={`${stats.avgConfidence}%`} change="+5%" positive color="green" />
          <StatCard icon="👥" title="Active Users" value={stats.activeUsers} change="+23%" positive color="blue" />
        </div>

        {/* Knowledge Gaps Table */}
        <div className="glass-panel rounded-3xl overflow-hidden">
          <div className="px-8 py-6 border-b border-slate-100 flex items-center justify-between bg-white/50">
            <h2 className="text-xl font-bold text-slate-900 flex items-center gap-3">
              <span className="text-2xl">🔥</span> Knowledge Gaps
            </h2>
            <div className="relative">
              <input
                type="text"
                placeholder="Search queries..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 pr-4 py-2.5 bg-white border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all w-64 placeholder-slate-400 shadow-sm"
              />
              <span className="absolute left-3 top-2.5 text-slate-400 text-lg">🔍</span>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/80 border-b border-slate-100">
                <tr>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Question</th>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Count</th>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Confidence</th>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Status</th>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Last Asked</th>
                  <th className="px-8 py-5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 bg-white/50">
                {filteredGaps.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-8 py-16 text-center text-slate-500">
                      <span className="text-4xl block mb-4">✨</span>
                      No knowledge gaps found. The AI is performing perfectly.
                    </td>
                  </tr>
                ) : (
                  filteredGaps.map((gap, index) => (
                    <tr key={index} className="hover:bg-slate-50/80 transition-colors group">
                      <td className="px-8 py-5">
                        <span className="font-semibold text-slate-900 group-hover:text-primary transition-colors">{gap.question}</span>
                      </td>
                      <td className="px-8 py-5 text-slate-600 font-medium">{gap.occurrenceCount}x</td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-2">
                          <div className="w-16 h-2 bg-slate-100 rounded-full overflow-hidden shadow-inner">
                            <div className="h-full bg-gradient-to-r from-violet-500 to-cyan-500" style={{ width: `${gap.confidenceScore * 100}%` }}></div>
                          </div>
                          <span className="text-slate-600 text-sm font-medium">{Math.round(gap.confidenceScore * 100)}%</span>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <span className={`px-3 py-1.5 rounded-full text-xs font-semibold ${getStatusColor(gap.status)}`}>
                          {gap.status}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-slate-500 text-sm font-medium">
                        {new Date(gap.lastAsked).toLocaleDateString()}
                      </td>
                      <td className="px-8 py-5">
                        {gap.status !== 'Resolved' ? (
                          <button className="px-4 py-2 bg-white hover:bg-slate-50 text-slate-700 text-xs font-bold rounded-lg transition-all border border-slate-200 hover:border-slate-300 shadow-sm">
                            Resolve
                          </button>
                        ) : (
                          <span className="text-green-600 font-medium flex items-center gap-1.5"><span className="text-lg">✓</span> Done</span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

interface StatCardProps {
  icon: string;
  title: string;
  value: number | string;
  change: string;
  positive: boolean;
  color: 'purple' | 'orange' | 'green' | 'blue';
}

const StatCard: React.FC<StatCardProps> = ({ icon, title, value, change, positive, color }) => {
  const colorClasses = {
    purple: 'bg-indigo-50 text-indigo-600 border-indigo-100 shadow-indigo-100/50',
    orange: 'bg-orange-50 text-orange-600 border-orange-100 shadow-orange-100/50',
    green: 'bg-emerald-50 text-emerald-600 border-emerald-100 shadow-emerald-100/50',
    blue: 'bg-cyan-50 text-cyan-600 border-cyan-100 shadow-cyan-100/50',
  };

  return (
    <div className={`glass-card p-6 rounded-3xl relative overflow-hidden group`}>
      <div className={`absolute -right-6 -top-6 w-24 h-24 rounded-full blur-2xl opacity-40 group-hover:opacity-60 transition-opacity duration-500 ${colorClasses[color].split(' ')[0]}`}></div>
      
      <div className="flex items-center justify-between mb-4 relative z-10">
        <span className="text-sm font-semibold text-slate-500 uppercase tracking-wider">{title}</span>
        <div className={`w-12 h-12 rounded-2xl flex items-center justify-center text-2xl border ${colorClasses[color]}`}>
          {icon}
        </div>
      </div>
      <div className="text-4xl font-bold text-slate-900 mb-3 tracking-tight relative z-10">{value}</div>
      <div className={`text-sm font-medium flex items-center gap-1.5 relative z-10 ${positive ? 'text-emerald-600' : 'text-rose-600'}`}>
        <span className="bg-slate-100 px-1.5 py-0.5 rounded-md">{positive ? '↑' : '↓'} {change}</span>
        <span className="text-slate-400 ml-1">vs last week</span>
      </div>
    </div>
  );
};

export default Dashboard;
