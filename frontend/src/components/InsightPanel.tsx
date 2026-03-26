import React, { useState, useEffect, useCallback } from 'react';
import api from '../services/api';
import { motion } from 'framer-motion';


interface MarketItem {
  symbol: string;
  value: string;
  change: string;
  percent: string;
  trend: string;
  color: string;
  lastUpdate: string;
}

interface MarketData {
  nifty: MarketItem;
  sensex: MarketItem;
  usdInr: { value: string; trend: string; color: string };
}


const InsightCard: React.FC<{ 
  symbol: string; 
  value: string; 
  change?: string; 
  percent?: string; 
  color: string; 
  lastUpdate?: string;
  loading?: boolean 
}> = ({ symbol, value, change, percent, color, lastUpdate, loading }) => (

  <motion.div 
    initial={{ opacity: 0, y: 10 }}
    animate={{ opacity: 1, y: 0 }}
    whileHover={{ y: -2, scale: 1.01 }}
    className="p-5 rounded-3xl border border-border-primary bg-bg-primary/40 backdrop-blur-md mb-4 hover:border-indigo-500/30 hover:shadow-2xl hover:shadow-indigo-500/5 transition-all duration-300 group"
  >
    <div className="text-[10px] font-black text-text-muted uppercase tracking-wider mb-2 group-hover:text-indigo-400 transition-colors">
      {symbol}
    </div>
    
    <div className="flex flex-col">
      {loading ? (
        <div className="space-y-2">
          <div className="h-9 w-32 bg-bg-secondary/50 animate-pulse rounded-xl" />
          <div className="h-4 w-48 bg-bg-secondary/30 animate-pulse rounded-lg" />
        </div>
      ) : (
        <>
          <motion.div 
            key={value}
            initial={{ opacity: 0.5 }}
            animate={{ opacity: 1 }}
            className="text-3xl font-black text-text-primary tracking-tighter mb-1"
          >
            {value}
          </motion.div>
          
          <div className={`flex items-center gap-2 text-xs font-bold ${color === 'green' ? 'text-emerald-500' : 'text-rose-500'}`}>
            <span>{change} ({percent})</span>
            <span className="text-[10px] opacity-70 font-medium text-text-muted">today</span>
          </div>

          <div className="mt-3 pt-3 border-t border-border-primary/50 text-[10px] text-text-muted font-medium flex items-center justify-between">
            <span className="opacity-60">{lastUpdate}</span>
            <span className="text-indigo-500/50 hover:text-indigo-500 cursor-help transition-colors">Disclaimer</span>
          </div>
        </>
      )}
    </div>
  </motion.div>
);

const InsightPanel: React.FC = () => {
  const [data, setData] = useState<MarketData | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);


  const fetchMarketData = useCallback(async (isManual = false) => {
    if (isManual) setRefreshing(true);
    else if (!data) setLoading(true);

    try {
      const response = await api.get('/Market/overview');
      setData(response.data);

    } catch (error) {
      console.error('Error fetching market data:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [data]);

  useEffect(() => {
    fetchMarketData();
    const interval = setInterval(() => fetchMarketData(), 10000); // Super-live: 10 second refresh
    return () => clearInterval(interval);
  }, [fetchMarketData]);


  return (
    <aside className="hidden lg:flex flex-col w-96 h-full border-l border-border-primary bg-bg-secondary/20 backdrop-blur-3xl p-6 overflow-y-auto scrollbar-hide">
      <div className="mb-8">
        <div className="flex items-center justify-between mb-8">
          <div className="flex flex-col">
            <h3 className="text-base font-black text-text-primary tracking-tight flex items-center gap-2">
              Financial Intelligence
            </h3>
            <div className="flex items-center gap-1.5 mt-1">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
              <span className="text-[10px] font-black text-emerald-500 uppercase tracking-widest">Real-time Feed</span>
            </div>
          </div>
          <motion.button 
            whileHover={{ scale: 1.1, rotate: 180 }}
            whileTap={{ scale: 0.9 }}
            onClick={() => fetchMarketData(true)}
            disabled={refreshing || loading}
            className={`p-2.5 rounded-2xl bg-bg-primary/50 border border-border-primary text-text-muted hover:text-indigo-500 hover:border-indigo-500/30 transition-all shadow-lg ${refreshing ? 'animate-spin' : ''}`}
            title="Refresh Market Data"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M21 12a9 9 0 1 1-9-9c2.52 0 4.93 1 6.74 2.74L21 8"/>
              <path d="M21 3v5h-5"/>
            </svg>
          </motion.button>
        </div>
        
        <div className="space-y-4">
          <InsightCard 
            symbol={data?.nifty.symbol || "INDEXNSE: NIFTY_50"} 
            value={data?.nifty.value || "23,306.45"} 
            change={data?.nifty.change || "+394.05"}
            percent={data?.nifty.percent || "1.72%"}
            color={data?.nifty.color || "green"} 
            lastUpdate={data?.nifty.lastUpdate || "25 Mar, 3:31 pm IST"}
            loading={loading && !data}
          />
          <InsightCard 
            symbol={data?.sensex.symbol || "INDEXBOM: SENSEX"} 
            value={data?.sensex.value || "76,456.20"} 
            change={data?.sensex.change || "+512.40"}
            percent={data?.sensex.percent || "0.67%"}
            color={data?.sensex.color || "green"} 
            lastUpdate={data?.sensex.lastUpdate || "25 Mar, 3:31 pm IST"}
            loading={loading && !data}
          />
          
          <div className="p-4 rounded-2xl bg-bg-primary/30 border border-border-primary flex items-center justify-between group hover:border-indigo-500/20 transition-all">
            <div className="flex flex-col">
              <span className="text-[9px] font-black text-text-muted uppercase tracking-widest mb-1">Currency Pair</span>
              <span className="text-sm font-black text-text-primary">USD / INR</span>
            </div>
            <div className="text-right">
              <div className="text-sm font-black text-text-primary">{data?.usdInr.value || "₹83.42"}</div>
              <div className="text-[10px] font-bold text-rose-500">{data?.usdInr.trend || "-0.02%"}</div>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-6">
        <h3 className="text-[11px] font-black text-text-muted uppercase tracking-[0.2em] mb-4 flex items-center gap-2">
          AI INSIGHTS ⚡
        </h3>
        <div className="space-y-4">
          <motion.div 
            whileHover={{ x: 4 }}
            className="p-5 rounded-3xl bg-indigo-500/[0.04] border border-indigo-500/10 hover:border-indigo-500/30 transition-all cursor-default group"
          >
            <p className="text-xs text-text-secondary leading-relaxed group-hover:text-text-primary transition-colors">
              Your interest in <span className="text-indigo-500 font-bold">Aggressive Hybrid Funds</span> suggests a high risk appetite. 
              <br/><br/>
              <span className="text-indigo-400 font-black text-[10px] uppercase">Smart Recommendation:</span> Consider balancing with Sovereign Gold Bonds for diversity.
            </p>
          </motion.div>
        </div>
      </div>

      <div className="mt-auto pt-8">
        <motion.div 
          whileHover={{ y: -4 }}
          className="p-6 rounded-[2.5rem] bg-gradient-to-br from-indigo-600 via-indigo-600 to-blue-700 text-white shadow-2xl shadow-indigo-500/30 relative overflow-hidden group border border-white/10"
        >
          <div className="absolute top-0 right-0 w-32 h-32 bg-white/20 rounded-full -mr-12 -mt-12 blur-3xl group-hover:bg-white/30 transition-all duration-500" />
          
          <div className="relative z-10 text-center">
            <h4 className="text-base font-black tracking-tight mb-1">Portfolio Pro</h4>
            <p className="text-xs opacity-80 leading-relaxed mb-5 font-medium">
              Unlock the full potential of AI risk management.
            </p>
            <button className="w-full py-3 bg-white text-indigo-700 rounded-2xl font-black text-xs hover:shadow-2xl transition-all">
              Upgrade Now
            </button>
          </div>
        </motion.div>
      </div>
    </aside>
  );
};

export default InsightPanel;


