# 🚀 Admin Panel Quick Start

## Installation (One-Time Setup)

```bash
cd d:\Ai_fund2\Ai_Fund\frontend
npm install recharts react-hot-toast zustand
```

## Start Application

```bash
npm run dev
```

## Access Admin Panel

1. **Login Page**: `http://localhost:3000/#/admin/login`
2. **Dashboard**: `http://localhost:3000/#/admin/dashboard`
3. **Analytics**: `http://localhost:3000/#/admin/analytics`
4. **Knowledge Gaps**: `http://localhost:3000/#/admin/knowledge-gaps`

## Default Credentials (Update in Backend)

```
Username: admin
Password: admin123
```

## Features at a Glance

### 📊 Dashboard
- Total Queries
- Avg AI Confidence
- Unanswered Queries
- Active Users
- AI Performance Score

### 📈 Analytics
- Queries Over Time (Line Chart)
- AI Confidence Trend (Line Chart)
- Category Usage (Bar Chart)

### 🧠 Knowledge Gaps
- Search & Filter
- Status Management
- Inline Updates

## API Endpoints Required

Your backend needs these endpoints:

```
POST   /api/admin/login
GET    /api/admin/dashboard
GET    /api/admin/analytics
GET    /api/admin/knowledge-gaps
PUT    /api/admin/knowledge-gap/{id}
```

## File Structure Created

```
src/admin/
├── components/      (7 files)
├── pages/          (4 files)
├── hooks/          (3 files)
├── services/       (1 file)
├── store/          (1 file)
├── types/          (1 file)
└── index.ts
```

## Next Steps

1. Install dependencies
2. Start dev server
3. Navigate to `/admin/login`
4. Login with credentials
5. Explore dashboard, analytics, and knowledge gaps

## Customization

- **Colors**: Edit `tailwind.config.js`
- **API URL**: Edit `src/admin/services/adminApi.ts`
- **Refresh Interval**: Edit hooks (default: 30s)
- **Theme**: Toggle in sidebar

## Support

Check `ADMIN_README.md` for detailed documentation.
