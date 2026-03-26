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
      <div className="flex flex-col items-center justify-center min-h-screen bg-whiteboard">
        <div className="w-16 h-16 border-4 border-slate-100 border-t-blue-600 rounded-full animate-spin mb-4"></div>
        <p className="text-slate-500 font-bold tracking-tight">Loading Dashboard...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-whiteboard relative z-10 overflow-auto">
      {/* Header */}
      <div className="bg-white border border-slate-200 px-8 py-6 mb-8 mt-4 mx-8 rounded-3xl shadow-sm">
        <div className="flex items-center justify-between max-w-7xl mx-auto">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 mb-2">Dashboard</h1>
            <p className="text-sm font-medium text-slate-400">Real-time AI performance monitoring</p>
          </div>
          <div className="flex gap-3">
            <button
              onClick={syncQdrant}
              className="px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-xl font-bold hover:bg-slate-50 transition-all flex items-center gap-2 shadow-sm"
            >
              🔄 Sync Qdrant
            </button>
            <button
              onClick={() => (window.location.href = '/chat')}
              className="px-5 py-2.5 bg-slate-900 text-white rounded-xl font-bold hover:bg-black transition-all flex items-center gap-2 shadow-lg shadow-slate-200"
            >
              💬 Open Chat
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-8 pb-12">
        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
          <StatCard icon="💬" title="Queries" value={stats.totalQueries} change="+12%" positive color="purple" />
          <StatCard icon="⚡" title="Gaps" value={stats.knowledgeGaps} change="-8%" positive color="orange" />
          <StatCard icon="🎯" title="Confidence" value={`${stats.avgConfidence}%`} change="+5%" positive color="green" />
          <StatCard icon="👥" title="Users" value={stats.activeUsers} change="+23%" positive color="blue" />
        </div>

        {/* Knowledge Gaps Table */}
        <div className="bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm">
          <div className="px-8 py-6 border-b border-slate-100 flex items-center justify-between">
            <h2 className="text-xl font-extrabold text-slate-900 flex items-center gap-3">
              🔥 Knowledge Gaps
            </h2>
            <div className="relative">
              <input
                type="text"
                placeholder="Search..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all w-64 font-medium"
              />
              <span className="absolute left-3 top-2 text-slate-400">🔍</span>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-slate-50/50 border-b border-slate-100">
                <tr>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Question</th>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Count</th>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Confidence</th>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Status</th>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Date</th>
                  <th className="px-8 py-4 text-left text-[11px] font-bold text-slate-400 uppercase tracking-widest">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filteredGaps.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-8 py-16 text-center text-slate-400">
                      <span className="text-3xl block mb-2">✨</span>
                      <p className="font-bold">No knowledge gaps found.</p>
                    </td>
                  </tr>
                ) : (
                  filteredGaps.map((gap, index) => (
                    <tr key={index} className="hover:bg-slate-50/50 transition-colors group">
                      <td className="px-8 py-5">
                        <span className="font-bold text-slate-900 group-hover:text-blue-600 transition-colors">{gap.question}</span>
                      </td>
                      <td className="px-8 py-5 text-slate-500 font-bold">{gap.occurrenceCount}x</td>
                      <td className="px-8 py-5">
                        <div className="flex items-center gap-2">
                          <div className="w-16 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                            <div className="h-full bg-blue-600" style={{ width: `${gap.confidenceScore * 100}%` }}></div>
                          </div>
                          <span className="text-slate-500 text-[11px] font-bold">{Math.round(gap.confidenceScore * 100)}%</span>
                        </div>
                      </td>
                      <td className="px-8 py-5">
                        <span className={`px-2.5 py-1 rounded-lg text-[10px] font-extrabold uppercase tracking-wider ${getStatusColor(gap.status)}`}>
                          {gap.status}
                        </span>
                      </td>
                      <td className="px-8 py-5 text-slate-400 text-xs font-bold">
                        {new Date(gap.lastAsked).toLocaleDateString()}
                      </td>
                      <td className="px-8 py-5">
                        {gap.status !== 'Resolved' ? (
                          <button className="px-4 py-2 bg-white hover:bg-slate-50 text-slate-900 text-[11px] font-extrabold uppercase border border-slate-200 rounded-lg shadow-sm active:scale-95 transition-all">
                            Resolve
                          </button>
                        ) : (
                          <span className="text-emerald-600 text-[11px] font-extrabold uppercase flex items-center gap-1">✓ Complete</span>
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
    purple: 'bg-blue-50 text-blue-600 border-blue-100',
    orange: 'bg-amber-50 text-amber-600 border-amber-100',
    green: 'bg-emerald-50 text-emerald-600 border-emerald-100',
    blue: 'bg-slate-50 text-slate-600 border-slate-100',
  };

  return (
    <div className="bg-white border border-slate-200 p-6 rounded-3xl shadow-sm hover:shadow-md transition-all group">
      <div className="flex items-center justify-between mb-4">
        <span className="text-[11px] font-bold text-slate-400 uppercase tracking-widest">{title}</span>
        <div className={`w-10 h-10 rounded-xl flex items-center justify-center text-xl border ${colorClasses[color]}`}>
          {icon}
        </div>
      </div>
      <div className="text-3xl font-extrabold text-slate-900 mb-2 tracking-tight">{value}</div>
      <div className={`text-[11px] font-bold flex items-center gap-1.5 ${positive ? 'text-emerald-600' : 'text-rose-600'}`}>
        <span className="bg-slate-50 px-1.5 py-0.5 rounded border border-slate-100">{positive ? '↑' : '↓'} {change}</span>
        <span className="text-slate-400">vs last week</span>
      </div>
    </div>
  );
};

export default Dashboard;
