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


const Sparkline: React.FC<{ data: number[]; color: string; loading?: boolean }> = ({ data, color, loading }) => {
  if (loading || data.length === 0) {
    return <div className="h-12 w-full bg-bg-secondary/20 animate-pulse rounded-lg mt-4" />;
  }

  const min = Math.min(...data);
  const max = Math.max(...data);
  const range = max - min;
  const padding = range * 0.1;
  const adjustedMin = min - padding;
  const adjustedMax = max + padding;
  const adjustedRange = adjustedMax - adjustedMin;

  const points = data.map((val, i) => ({
    x: (i / (data.length - 1)) * 100,
    y: 100 - ((val - adjustedMin) / adjustedRange) * 100
  }));

  const pathData = points.reduce((acc, point, i) => 
    i === 0 ? `M ${point.x},${point.y}` : `${acc} L ${point.x},${point.y}`, 
  "");

  return (
    <div className="h-14 w-full mt-4 relative group/chart">
      <svg className="w-full h-full overflow-visible" viewBox="0 0 100 100" preserveAspectRatio="none">
        <defs>
          <linearGradient id={`grad-${color}`} x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor={color === 'green' ? '#10b981' : '#f43f5e'} stopOpacity="0.2" />
            <stop offset="100%" stopColor={color === 'green' ? '#10b981' : '#f43f5e'} stopOpacity="0" />
          </linearGradient>
        </defs>
        <motion.path
          initial={{ pathLength: 0, opacity: 0 }}
          animate={{ pathLength: 1, opacity: 1 }}
          transition={{ duration: 1.5, ease: "easeInOut" }}
          d={pathData}
          fill="none"
          stroke={color === 'green' ? '#10b981' : '#f43f5e'}
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <motion.path
          initial={{ opacity: 0 }}
          animate={{ opacity: 0.5 }}
          d={`${pathData} L 100,100 L 0,100 Z`}
          fill={`url(#grad-${color})`}
        />
      </svg>
    </div>
  );
};

const InsightCard: React.FC<{ 
  symbol: string; 
  value: string; 
  change?: string; 
  percent?: string; 
  color: string; 
  lastUpdate?: string;
  loading?: boolean 
}> = ({ symbol, value, change, percent, color, lastUpdate, loading }) => {
  const [range, setRange] = useState<'1d' | '5d' | '1mo'>('1d');
  const [chartData, setChartData] = useState<number[]>([]);
  const [chartLoading, setChartLoading] = useState(false);

  useEffect(() => {
    const fetchChart = async () => {
      setChartLoading(true);
      try {
        const id = symbol.includes('NIFTY') ? 'NIFTY' : 'SENSEX';
        const response = await api.get(`/Market/chart/${id}?range=${range}`);
        const validData = (response.data as (number | null)[]).filter((v): v is number => v !== null);
        setChartData(validData);
      } catch (error) {
        console.error('Error fetching chart:', error);
      } finally {
        setChartLoading(false);
      }
    };
    fetchChart();
  }, [symbol, range]);

  // Derive dynamic values for historical ranges
  let displayValue = value;
  let displayChange = change;
  let displayPercent = percent;
  let displayColor = color;
  let rangeLabel = "today";

  if (range !== '1d' && chartData.length >= 2) {
    const first = chartData[0];
    const last = chartData[chartData.length - 1];
    const diff = last - first;
    const diffPercent = (diff / first) * 100;
    
    displayValue = last.toLocaleString('en-IN', { maximumFractionDigits: 2 });
    displayChange = `${diff >= 0 ? '+' : ''}${diff.toLocaleString('en-IN', { maximumFractionDigits: 2 })}`;
    displayPercent = `${diffPercent.toFixed(2)}%`;
    displayColor = diff >= 0 ? 'green' : 'rose';
    rangeLabel = range === '5d' ? 'past 5 days' : 'past month';
  }

  return (
    <motion.div 
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      whileHover={{ y: -2, scale: 1.01 }}
      className="p-5 rounded-3xl border border-border-primary bg-bg-primary/40 backdrop-blur-md mb-4 hover:border-indigo-500/30 hover:shadow-2xl hover:shadow-indigo-500/5 transition-all duration-300 group"
    >
      <div className="flex items-center justify-between mb-2">
        <div className="text-[10px] font-black text-text-muted uppercase tracking-wider group-hover:text-indigo-400 transition-colors">
          {symbol}
        </div>
        <div className="flex gap-1.5">
          {(['1d', '5d', '1mo'] as const).map((r) => (
            <button
              key={r}
              onClick={(e) => { e.stopPropagation(); setRange(r); }}
              className={`text-[8px] font-black px-1.5 py-0.5 rounded-md transition-all ${range === r ? 'bg-indigo-500 text-white' : 'text-text-muted hover:bg-bg-secondary'}`}
            >
              {r.toUpperCase()}
            </button>
          ))}
        </div>
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
              initial={{ opacity: 0.5 }}
              animate={{ opacity: 1 }}
              className="text-3xl font-black text-text-primary tracking-tighter mb-1"
            >
              {displayValue}
            </motion.div>
            
            <div className={`flex items-center gap-2 text-xs font-bold ${displayColor === 'green' ? 'text-emerald-500' : 'text-rose-500'}`}>
              <span>{displayChange} ({displayPercent})</span>
              <span className="text-[10px] opacity-70 font-medium text-text-muted">{rangeLabel}</span>
            </div>

            <Sparkline data={chartData} color={displayColor} loading={chartLoading} />

            <div className="mt-4 pt-3 border-t border-border-primary/50 text-[10px] text-text-muted font-medium flex items-center justify-between">
              <span className="opacity-60">{lastUpdate}</span>
              <span className="text-indigo-500/50 hover:text-indigo-500 cursor-help transition-colors">Disclaimer</span>
            </div>
          </>
        )}
      </div>
    </motion.div>
  );
};



interface NewsArticle {
  uuid: string;
  title: string;
  description: string;
  url: string;
  source: string;
  published_at: string;
  image_url?: string;
}

const NewsItem: React.FC<{ article: NewsArticle }> = ({ article }) => (
  <motion.a
    href={article.url}
    target="_blank"
    rel="noopener noreferrer"
    whileHover={{ x: 4 }}
    className="block p-4 rounded-[2rem] bg-bg-primary/30 border border-border-primary hover:border-indigo-500/30 hover:bg-bg-primary/50 transition-all group mb-3 relative overflow-hidden"
  >
    <div className="flex gap-4">
      {article.image_url ? (
        <div className="w-16 h-16 rounded-2xl overflow-hidden flex-shrink-0 border border-border-primary/50">
          <img src={article.image_url} alt="" className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" />
        </div>
      ) : (
        <div className="w-16 h-16 rounded-2xl bg-indigo-500/10 flex items-center justify-center flex-shrink-0 border border-border-primary/50">
          <svg className="w-6 h-6 text-indigo-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M4 22h16a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v16a2 2 0 0 1-2 2Zm0 0a2 2 0 0 1-2-2v-9c0-1.1.9-2 2-2h2" />
            <path d="M18 14h-8" /><path d="M15 18h-5" /><path d="M10 6h8v4h-8V6Z" />
          </svg>
        </div>
      )}
      <div className="flex-1 min-w-0">
        <h4 className="text-[11px] font-black text-text-primary leading-tight line-clamp-2 mb-1 group-hover:text-indigo-400 transition-colors">
          {article.title}
        </h4>
        <div className="flex items-center gap-2 text-[9px] font-bold text-text-muted">
          <span className="uppercase tracking-wider">{article.source}</span>
          <span className="w-1 h-1 rounded-full bg-border-primary" />
          <span>{new Date(article.published_at).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
        </div>
      </div>
    </div>
  </motion.a>
);

const InsightPanel: React.FC = () => {
  const [data, setData] = useState<MarketData | null>(null);
  const [news, setNews] = useState<NewsArticle[]>([]);
  const [loading, setLoading] = useState(true);
  const [newsLoading, setNewsLoading] = useState(true);
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

  const fetchNews = useCallback(async () => {
    setNewsLoading(true);
    try {
      const response = await api.get('/Market/news');
      setNews(response.data);
    } catch (error) {
      console.error('Error fetching news:', error);
    } finally {
      setNewsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchMarketData();
    fetchNews();
    const interval = setInterval(() => {
      fetchMarketData();
      fetchNews();
    }, 60000); // 1 minute refresh to avoid API rate limits
    return () => clearInterval(interval);
  }, [fetchMarketData, fetchNews]);


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
            onClick={() => { fetchMarketData(true); fetchNews(); }}
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
          
          <div className="p-4 rounded-2xl bg-bg-primary/30 border border-border-primary flex items-center justify-between group hover:border-emerald-500/20 transition-all">
            <div className="flex flex-col">
              <span className="text-[9px] font-black text-text-muted uppercase tracking-widest mb-1">Currency Pair</span>
              <span className="text-sm font-black text-text-primary">USD / INR</span>
            </div>
            <div className="text-right">
              <div className="text-sm font-black text-text-primary">{data?.usdInr.value || "₹83.42"}</div>
              <div className={`text-[10px] font-bold ${data?.usdInr.color === 'rose' ? 'text-rose-500' : 'text-emerald-500'}`}>{data?.usdInr.trend || "-0.02%"}</div>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-2">
        <h3 className="text-[11px] font-black text-text-muted uppercase tracking-[0.2em] mb-4 flex items-center justify-between">
          <span>LIVE MARKET NEWS ⚡</span>
          {newsLoading && <span className="w-1.5 h-1.5 rounded-full bg-indigo-500 animate-ping" />}
        </h3>
        <div className="flex flex-col">
          {newsLoading && news.length === 0 ? (
            [1, 2, 3].map((i) => (
              <div key={i} className="p-4 rounded-[2rem] bg-bg-primary/20 border border-border-primary mb-3 animate-pulse">
                <div className="flex gap-4">
                  <div className="w-16 h-16 rounded-2xl bg-bg-secondary/50 flex-shrink-0" />
                  <div className="flex-1 space-y-2">
                    <div className="h-4 w-full bg-bg-secondary/50 rounded-lg" />
                    <div className="h-3 w-2/3 bg-bg-secondary/30 rounded-lg" />
                  </div>
                </div>
              </div>
            ))
          ) : (
            <>
              {news.map((article) => (
                <NewsItem key={article.uuid} article={article} />
              ))}
              
              {news.length === 0 && !newsLoading && (
                <div className="text-center py-8 opacity-50 px-4">
                  <p className="text-[10px] font-bold text-text-muted uppercase tracking-widest leading-relaxed">
                    Connecting to global feeds... <br/> No headlines available right now.
                  </p>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      <div className="mt-auto pt-8">
        <motion.div 
          whileHover={{ y: -4 }}
          className="p-6 rounded-[2.5rem] bg-gradient-to-br from-indigo-600 via-indigo-600 to-emerald-600 text-white shadow-2xl shadow-indigo-500/30 relative overflow-hidden group border border-white/10"
        >
          <div className="absolute top-0 right-0 w-32 h-32 bg-white/20 rounded-full -mr-12 -mt-12 blur-3xl group-hover:bg-white/30 transition-all duration-500" />
          
          <div className="relative z-10">
            <div className="flex items-center gap-2 mb-2">
              <div className="p-1.5 rounded-lg bg-white/20 backdrop-blur-md">
                <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
                </svg>
              </div>
              <h4 className="text-sm font-black tracking-tight">Portfolio Alpha</h4>
            </div>
            <p className="text-[10px] opacity-80 leading-relaxed mb-4 font-medium max-w-[180px]">
              Deploy advanced AI risk nodes to shield your capital from volatility.
            </p>
            <button className="px-5 py-2.5 bg-white text-indigo-700 rounded-xl font-black text-xs hover:shadow-2xl transition-all hover:scale-105 active:scale-95">
              Activate Nodes
            </button>
          </div>
        </motion.div>
      </div>
    </aside>
  );
};

export default InsightPanel;


