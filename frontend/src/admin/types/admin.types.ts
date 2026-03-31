// Admin Panel Type Definitions

export interface AdminLoginRequest {
  username: string;
  password: string;
}

export interface AdminLoginResponse {
  token: string;
  user: {
    id: string;
    username: string;
    role: string;
  };
}

export interface DashboardStats {
  totalQueries: number;
  avgConfidence: number;
  unanswered: number;
  activeUsers: number;
  aiPerformanceScore?: number;
}

export interface AnalyticsData {
  queriesOverTime: TimeSeriesData[];
  confidenceTrend: TimeSeriesData[];
  categoryUsage: CategoryData[];
}

export interface TimeSeriesData {
  date: string;
  value: number;
}

export interface CategoryData {
  category: string;
  count: number;
}

export interface KnowledgeGap {
  id: string;
  question: string;
  confidenceScore: number;
  status: 'New' | 'Reviewing' | 'Resolved';
  count: number;
  lastAsked: string;
  createdAt: string;
}

export interface KnowledgeGapUpdateRequest {
  status: 'New' | 'Reviewing' | 'Resolved';
}

export interface QdrantStatus {
  collectionExists: boolean;
  collectionName: string;
  status: string;
}

export interface TrendingQuery {
  query: string;
  count: number;
  avgConfidence: number;
}

export type ThemeMode = 'light' | 'dark';

export interface AdminState {
  isAuthenticated: boolean;
  token: string | null;
  user: AdminLoginResponse['user'] | null;
  theme: ThemeMode;
  login: (token: string, user: AdminLoginResponse['user']) => void;
  logout: () => void;
  toggleTheme: () => void;
}
