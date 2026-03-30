# 🔧 Setup Guide - Node.js Installation Required

## ❌ Current Issue
`npm` command not found - Node.js is not installed or not in PATH

## ✅ Solution: Install Node.js

### Step 1: Download Node.js
1. Go to: https://nodejs.org/
2. Download **LTS version** (recommended)
3. Run the installer
4. **IMPORTANT**: Check "Add to PATH" during installation

### Step 2: Verify Installation
Open a NEW PowerShell/CMD window and run:
```bash
node --version
npm --version
```

You should see version numbers like:
```
v20.x.x
10.x.x
```

### Step 3: Install Frontend Dependencies
```bash
cd d:\Ai_fund2\Ai_Fund\frontend
npm install
```

### Step 4: Start Frontend
```bash
npm run dev
```

## 🚀 After Installation

### Start Backend (Terminal 1)
```bash
cd d:\Ai_fund2\Ai_Fund\Ai_Fund
dotnet run
```

### Start Frontend (Terminal 2)
```bash
cd d:\Ai_fund2\Ai_Fund\frontend
npm run dev
```

## 🌐 Access URLs

**User App:**
- http://localhost:3000/
- http://localhost:3000/#/

**Admin Panel:**
- http://localhost:3000/#/admin/login
- http://localhost:3000/#/admin/dashboard
- http://localhost:3000/#/admin/analytics
- http://localhost:3000/#/admin/knowledge-gaps

## 📦 What's Already Created

✅ Complete Admin Panel (17 files)
✅ Login page with JWT authentication
✅ Dashboard with stats cards
✅ Analytics with charts
✅ Knowledge Gap management
✅ Dark/Light mode
✅ Auto-refresh (30s)
✅ Responsive design
✅ All TypeScript types
✅ API service layer
✅ State management (Zustand)

## 🎯 Admin Panel Features

### Dashboard
- Total Queries
- Avg AI Confidence
- Unanswered Queries
- Active Users
- AI Performance Score

### Analytics
- Queries Over Time (Line Chart)
- AI Confidence Trend (Line Chart)
- Category Usage (Bar Chart)

### Knowledge Gaps
- Search & Filter
- Status Management (New/Reviewing/Resolved)
- Inline Updates
- Confidence Scores

## 🔐 Backend API Endpoints Needed

Your backend needs to implement these endpoints:

```
POST   /api/admin/login
GET    /api/admin/dashboard
GET    /api/admin/analytics
GET    /api/admin/knowledge-gaps
PUT    /api/admin/knowledge-gap/{id}
```

## 📝 Next Steps

1. ✅ Install Node.js from https://nodejs.org/
2. ✅ Restart PowerShell/CMD
3. ✅ Run `npm install` in frontend folder
4. ✅ Run `npm run dev` to start frontend
5. ✅ Navigate to http://localhost:3000/#/admin/login
6. ✅ Login and explore the admin panel

## 🆘 Still Having Issues?

### Check Node.js Installation
```bash
where node
where npm
```

### Reinstall Node.js
- Uninstall current version
- Download fresh installer
- Make sure "Add to PATH" is checked
- Restart computer if needed

### Alternative: Use NVM (Node Version Manager)
1. Install NVM for Windows
2. Run: `nvm install lts`
3. Run: `nvm use lts`

## 📚 Documentation

- Full docs: `ADMIN_README.md`
- Quick start: `ADMIN_QUICKSTART.md`
- Setup guide: `ADMIN_SETUP.md`

---

**The admin panel is fully built and ready to use once Node.js is installed!**
