# Admin Panel Setup

## Install Dependencies

```bash
cd d:\Ai_fund2\Ai_Fund\frontend
npm install recharts react-hot-toast zustand
```

## Folder Structure

```
src/
├── admin/
│   ├── components/
│   │   ├── AdminSidebar.tsx
│   │   ├── AdminHeader.tsx
│   │   ├── StatCard.tsx
│   │   ├── ChartCard.tsx
│   │   ├── KnowledgeGapTable.tsx
│   │   ├── StatusBadge.tsx
│   │   └── SkeletonLoader.tsx
│   ├── pages/
│   │   ├── AdminLogin.tsx
│   │   ├── AdminDashboard.tsx
│   │   ├── AdminAnalytics.tsx
│   │   └── AdminKnowledgeGaps.tsx
│   ├── hooks/
│   │   ├── useDashboardData.ts
│   │   ├── useAnalytics.ts
│   │   └── useKnowledgeGaps.ts
│   ├── services/
│   │   └── adminApi.ts
│   ├── store/
│   │   └── adminStore.ts
│   └── types/
│       └── admin.types.ts
```

## Features

- ✅ Admin Login with JWT
- ✅ Protected Routes
- ✅ Dashboard with Stats
- ✅ Analytics with Charts
- ✅ Knowledge Gap Management
- ✅ Dark/Light Mode
- ✅ Auto-refresh (30s)
- ✅ Responsive Design
- ✅ Skeleton Loaders
- ✅ Toast Notifications
