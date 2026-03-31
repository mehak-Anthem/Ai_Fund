# FundAI

FundAI is a full-stack AI-assisted mutual fund and market insights application with:

- ASP.NET Core backend API
- React + Vite frontend
- Admin panel for analytics and knowledge gap management
- SQL Server storage
- Qdrant vector search integration

## Project Structure

```text
Ai_Fund/    Backend API
frontend/   Frontend and admin panel
render.yaml Render deployment config
```

## Requirements

- .NET 10 SDK/runtime
- Node.js 18+
- SQL Server database
- Optional: Qdrant for vector search features

## Backend Setup

Set these values in environment variables or local development settings:

```env
ConnectionStrings__DefaultConnection=...
Jwt__Key=...
Jwt__Issuer=AiFundIssuer
Jwt__Audience=AiFundAudience
Gemini__ApiKey=...
Groq__ApiKey=...
Groq__Model=llama-3.1-8b-instant
MarketAux__ApiKey=...
Qdrant__Host=...
Qdrant__Port=6334
Qdrant__CollectionName=ai_fund_knowledge
Qdrant__ApiKey=...
Voyage__ApiKey=...
```

Run the backend:

```bash
cd Ai_Fund
dotnet run
```

Default local URLs:

- `https://localhost:44328`
- `https://localhost:7128`
- `http://localhost:5227`

Swagger:

- `https://localhost:44328/swagger`
- `https://localhost:7128/swagger`

## Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

Default frontend URL:

- `http://localhost:3000`

## Admin Panel

Admin routes:

- `/admin/login`
- `/admin/dashboard`
- `/admin/analytics`
- `/admin/knowledge-gaps`

The Knowledge Gaps admin page includes:

- gap list and status updates
- Qdrant collection status
- one-click sync to Qdrant

## Deployment

The repository includes `render.yaml` for deployment.

Backend:

- Docker-based service from `Ai_Fund/Dockerfile`

Frontend:

- static site from `frontend`

## Notes

- Protected API endpoints require JWT Bearer authentication unless explicitly marked anonymous.
- Keep secrets out of git and provide them through environment variables in production.
- Qdrant-related features require valid Qdrant configuration.
