import React, { useState, useEffect } from 'react';
import api from '../services/api';

interface MarketItem {
  value: string;
  trend: string;
  color: string;
}

interface MarketData {
  nifty: MarketItem;
  sensex: MarketItem;
  usdInr: MarketItem;
}

const InsightCard: React.FC<{ title: string; value: string; trend?: string; color: string; loading?: boolean }> = ({ title, value, trend, color, loading }) => (
  <div className="p-4 rounded-2xl border border-border-primary bg-bg-primary/50 mb-4 hover:border-indigo-500/20 transition-all duration-300">
    <div className="text-[10px] font-bold text-text-muted uppercase tracking-widest mb-1">{title}</div>
    <div className="flex items-end justify-between">
      {loading ? (
        <div className="h-6 w-24 bg-bg-secondary animate-pulse rounded-lg" />
      ) : (
        <>
          <div className="text-xl font-extrabold text-text-primary">{value}</div>
          {trend && (
            <div className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${color === 'green' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-rose-500/10 text-rose-500'}`}>
              {trend}
            </div>
          )}
        </>
      )}
    </div>
  </div>
);

const InsightPanel: React.FC = () => {
  const [data, setData] = useState<MarketData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMarketData = async () => {
      try {
        const response = await api.get('/Market/overview');
        setData(response.data);
      } catch (error) {
        console.error('Error fetching market data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMarketData();
    const interval = setInterval(fetchMarketData, 60000); // Refresh every minute
    return () => clearInterval(interval);
  }, []);

  return (
    <aside className="hidden lg:flex flex-col w-80 h-full border-l border-border-primary bg-bg-secondary/30 backdrop-blur-xl p-6 overflow-y-auto scrollbar-hide">
      <div className="mb-8">
        <h3 className="text-sm font-bold text-text-primary mb-6 flex items-center gap-2">
          <span className="w-2 h-2 rounded-full bg-indigo-500 animate-pulse" />
          Market Overview
        </h3>
        
        <InsightCard 
          title="NIFTY 50" 
          value={data?.nifty.value || "22,453.80"} 
          trend={data?.nifty.trend || "+0.45%"} 
          color={data?.nifty.color || "green"} 
          loading={loading && !data}
        />
        <InsightCard 
          title="SENSEX" 
          value={data?.sensex.value || "73,903.15"} 
          trend={data?.sensex.trend || "+0.38%"} 
          color={data?.sensex.color || "green"} 
          loading={loading && !data}
        />
        <InsightCard 
          title="USD/INR" 
          value={data?.usdInr.value || "₹83.42"} 
          trend={data?.usdInr.trend || "-0.02%"} 
          color={data?.usdInr.color || "rose"} 
          loading={loading && !data}
        />
      </div>


      <div className="mt-4">
        <h3 className="text-sm font-bold text-text-primary mb-4 flex items-center gap-2">
          ✨ AI Insights
        </h3>
        <div className="space-y-4">
          <div className="p-4 rounded-2xl bg-indigo-500/5 border border-indigo-500/10">
            <p className="text-xs text-text-secondary leading-relaxed">
              Based on your recent queries, you're interested in **Aggressive Hybrid Funds**. 
              <br/><br/>
              <span className="text-indigo-500 font-bold">Pro Tip:</span> Consider a SIP approach to mitigate volatility in high-equity schemes.
            </p>
          </div>
          <div className="p-4 rounded-2xl bg-bg-primary/50 border border-border-primary italic">
            <p className="text-xs text-text-muted leading-relaxed">
              "The best time to plant a tree was 20 years ago. The second best time is now."
            </p>
          </div>
        </div>
      </div>

      <div className="mt-auto pt-8">
        <div className="p-5 rounded-3xl bg-gradient-to-br from-indigo-600 to-blue-700 text-white shadow-xl shadow-indigo-500/20">
          <h4 className="text-sm font-bold mb-2">Portfolio Shield 🛡️</h4>
          <p className="text-[10px] opacity-80 leading-relaxed mb-4">
            Activate advanced AI risk monitoring for your mutual fund portfolio.
          </p>
          <button className="w-full py-2 bg-white text-indigo-600 rounded-xl font-bold text-[11px] hover:bg-opacity-90 transition-all">
            Upgrade to Pro
          </button>
        </div>
      </div>
    </aside>
  );
};

export default InsightPanel;
