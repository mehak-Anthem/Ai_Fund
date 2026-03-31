import axios, { AxiosInstance } from 'axios';
import {
  AdminLoginRequest,
  AdminLoginResponse,
  DashboardStats,
  AnalyticsData,
  KnowledgeGap,
  KnowledgeGapUpdateRequest,
  QdrantStatus,
  TrendingQuery,
} from '../types/admin.types';

const API_BASE_URL = `${
  import.meta.env.VITE_API_URL ||
  (import.meta.env.DEV ? 'https://localhost:44328' : 'https://ai-fund.onrender.com')
}/api`;

class AdminApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('adminToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('adminToken');
          window.location.href = '/#/admin/login';
        }
        return Promise.reject(error);
      }
    );
  }

  async login(credentials: AdminLoginRequest): Promise<AdminLoginResponse> {
    const response = await this.api.post<AdminLoginResponse>('/admin/login', credentials);
    return response.data;
  }

  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.api.get<DashboardStats>('/admin/dashboard');
    return response.data;
  }

  async getAnalytics(): Promise<AnalyticsData> {
    const response = await this.api.get<AnalyticsData>('/admin/analytics');
    return response.data;
  }

  async getKnowledgeGaps(): Promise<KnowledgeGap[]> {
    const response = await this.api.get<KnowledgeGap[]>('/admin/knowledge-gaps');
    return response.data;
  }

  async updateKnowledgeGapStatus(
    id: string,
    data: KnowledgeGapUpdateRequest
  ): Promise<KnowledgeGap> {
    const response = await this.api.put<KnowledgeGap>(`/admin/knowledge-gap/${id}`, data);
    return response.data;
  }

  async getTrendingQueries(): Promise<TrendingQuery[]> {
    const response = await this.api.get<TrendingQuery[]>('/admin/trending-queries');
    return response.data;
  }

  async getQdrantStatus(): Promise<QdrantStatus> {
    const response = await this.api.get<QdrantStatus>('/KnowledgeGap/qdrant-status');
    return response.data;
  }

  async syncKnowledgeToQdrant(): Promise<{ message?: string }> {
    const response = await this.api.post<{ message?: string }>('/KnowledgeGap/sync-to-qdrant');
    return response.data;
  }
}

export const adminApi = new AdminApiService();
