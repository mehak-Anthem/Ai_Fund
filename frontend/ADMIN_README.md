# 🎯 FundAI Admin Panel

## 📋 Overview

A modern, production-ready admin panel for monitoring and managing the FundAI application. Built with React 19, TypeScript, and Tailwind CSS.

---

## ✨ Features

### 🔐 Authentication
- Secure JWT-based login
- Protected routes
- Auto-redirect on unauthorized access
- Token persistence in localStorage

### 📊 Dashboard
- **Total Queries** - Track all user queries
- **Avg AI Confidence** - Monitor AI performance
- **Unanswered Queries** - Identify gaps
- **Active Users** - Real-time user tracking
- **AI Performance Score** - Overall system health
- Auto-refresh every 30 seconds

### 📈 Analytics
- **Queries Over Time** - Line chart showing query trends
- **AI Confidence Trend** - Track confidence improvements
- **Category Usage** - Bar chart of popular categories
- Auto-refresh every 30 seconds
- Interactive charts with Recharts

### 🧠 Knowledge Gap Management
- View all low-confidence queries
- Filter by status (New/Reviewing/Resolved)
- Search functionality
- Update status inline
- Track question frequency
- Monitor confidence scores

### 🎨 UI/UX
- Dark/Light mode toggle
- Glassmorphism design
- Smooth animations with Framer Motion
- Responsive layout
- Skeleton loaders
- Toast notifications
- Premium fintech styling

---

## 🚀 Installation

### 1. Install Dependencies

```bash
cd d:\Ai_fund2\Ai_Fund\frontend
npm install
```

This will install:
- `recharts` - For charts
- `react-hot-toast` - For notifications
- `zustand` - For state management

### 2. Start Development Server

```bash
npm run dev
```

---

## 📁 Project Structure

```
src/admin/
├── components/
│   ├── AdminSidebar.tsx          # Navigation sidebar
│   ├── AdminHeader.tsx           # Page header with user info
│   ├── StatCard.tsx              # Reusable stat card
│   ├── ChartCard.tsx             # Reusable chart container
│   ├── KnowledgeGapTable.tsx     # Table with filtering
│   ├── StatusBadge.tsx           # Status indicator
│   ├── SkeletonLoader.tsx        # Loading states
│   └── AdminProtectedRoute.tsx   # Route protection
├── pages/
│   ├── AdminLogin.tsx            # Login page
│   ├── AdminDashboard.tsx        # Main dashboard
│   ├── AdminAnalytics.tsx        # Analytics page
│   └── AdminKnowledgeGaps.tsx    # Knowledge gaps page
├── hooks/
│   ├── useDashboardData.ts       # Dashboard data hook
│   ├── useAnalytics.ts           # Analytics data hook
│   └── useKnowledgeGaps.ts       # Knowledge gaps hook
├── services/
│   └── adminApi.ts               # API service layer
├── store/
│   └── adminStore.ts             # Zustand store
├── types/
│   └── admin.types.ts            # TypeScript types
└── index.ts                      # Exports
```

---

## 🔌 API Integration

### Base URL
```typescript
const API_BASE_URL = 'https://localhost:44328/api';
```

### Endpoints

#### Authentication
```
POST /api/admin/login
Body: { username: string, password: string }
Response: { token: string, user: { id, username, role } }
```

#### Dashboard
```
GET /api/admin/dashboard
Response: {
  totalQueries: number,
  avgConfidence: number,
  unanswered: number,
  activeUsers: number,
  aiPerformanceScore?: number
}
```

#### Analytics
```
GET /api/admin/analytics
Response: {
  queriesOverTime: [{ date: string, value: number }],
  confidenceTrend: [{ date: string, value: number }],
  categoryUsage: [{ category: string, count: number }]
}
```

#### Knowledge Gaps
```
GET /api/admin/knowledge-gaps
Response: KnowledgeGap[]

PUT /api/admin/knowledge-gap/{id}
Body: { status: 'New' | 'Reviewing' | 'Resolved' }
Response: KnowledgeGap
```

---

## 🎨 Customization

### Theme Colors

Edit `tailwind.config.js`:

```javascript
colors: {
  primary: '#0f766e',      // Teal
  secondary: '#14b8a6',    // Emerald
  // Add your colors
}
```

### Auto-Refresh Interval

Change in hooks:

```typescript
// Default: 30 seconds
useDashboardData(true, 30000);

// Custom: 60 seconds
useDashboardData(true, 60000);
```

---

## 🔒 Security

### JWT Token
- Stored in localStorage
- Auto-attached to all API requests
- Auto-redirect on 401 errors

### Protected Routes
All admin routes require authentication:
- `/admin/dashboard`
- `/admin/analytics`
- `/admin/knowledge-gaps`

---

## 🎯 Usage

### 1. Login
Navigate to `/admin/login` and enter credentials.

### 2. Dashboard
View real-time stats and AI performance.

### 3. Analytics
Monitor trends with interactive charts.

### 4. Knowledge Gaps
Manage low-confidence queries:
- Search for specific questions
- Filter by status
- Update status inline

---

## 🛠️ Development

### Run Development Server
```bash
npm run dev
```

### Build for Production
```bash
npm run build
```

### Preview Production Build
```bash
npm run preview
```

---

## 📊 State Management

Using Zustand for global state:

```typescript
const { 
  isAuthenticated, 
  token, 
  user, 
  theme,
  login, 
  logout, 
  toggleTheme 
} = useAdminStore();
```

---

## 🎨 Components

### StatCard
```tsx
<StatCard
  title="Total Queries"
  value={1234}
  icon={MessageSquare}
  trend={{ value: 12, isPositive: true }}
  suffix="%"
  loading={false}
/>
```

### ChartCard
```tsx
<ChartCard title="Queries Over Time" loading={false}>
  <LineChart data={data}>
    {/* Chart content */}
  </LineChart>
</ChartCard>
```

### StatusBadge
```tsx
<StatusBadge status="New" />
<StatusBadge status="Reviewing" />
<StatusBadge status="Resolved" />
```

---

## 🔄 Auto-Refresh

Dashboard and Analytics auto-refresh every 30 seconds:

```typescript
// In hooks
useEffect(() => {
  fetchData();
  
  const intervalId = setInterval(() => {
    fetchData(false); // Silent refresh
  }, 30000);
  
  return () => clearInterval(intervalId);
}, []);
```

---

## 🎯 Best Practices

1. **Type Safety** - All API responses are typed
2. **Error Handling** - Toast notifications for errors
3. **Loading States** - Skeleton loaders during fetch
4. **Responsive Design** - Mobile-first approach
5. **Clean Code** - Modular components
6. **Performance** - Optimized re-renders

---

## 🐛 Troubleshooting

### API Connection Error
- Ensure backend is running on `https://localhost:44328`
- Check CORS settings
- Verify JWT token

### Dark Mode Not Working
- Check localStorage for `admin-storage`
- Verify `dark` class on `<html>` element

### Charts Not Rendering
- Ensure `recharts` is installed
- Check data format matches expected structure

---

## 📱 Responsive Breakpoints

```css
sm: 640px   /* Mobile */
md: 768px   /* Tablet */
lg: 1024px  /* Desktop */
xl: 1280px  /* Large Desktop */
```

---

## 🎨 Color Scheme

### Light Mode
- Background: `#f8fafc`
- Surface: `#ffffff`
- Text: `#111827`
- Border: `#e5e7eb`

### Dark Mode
- Background: `#111827`
- Surface: `#1f2937`
- Text: `#f9fafb`
- Border: `#374151`

---

## 🚀 Deployment

### Build
```bash
npm run build
```

### Output
```
dist/
├── index.html
├── assets/
│   ├── index-[hash].js
│   └── index-[hash].css
```

### Deploy
Upload `dist/` folder to your hosting service.

---

## 📝 Notes

- All data is fetched dynamically from API
- No hardcoded values
- Production-ready code
- Clean and modular architecture
- Easy to extend and customize

---

## 🎯 Future Enhancements

- [ ] Export data to CSV
- [ ] Advanced filtering
- [ ] Real-time notifications
- [ ] User management
- [ ] Audit logs
- [ ] Custom date ranges

---

**Built with ❤️ using React 19, TypeScript, and Tailwind CSS**
