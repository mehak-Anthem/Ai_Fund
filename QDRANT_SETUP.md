# Qdrant Vector Database Setup

## 🚀 Quick Start

### Step 1: Start Qdrant with Docker

```bash
# Navigate to project directory
cd d:\AI_Fund\Ai_Fund

# Start Qdrant
docker-compose -f docker-compose.qdrant.yml up -d
```

### Step 2: Verify Qdrant is Running

Open in browser: `http://localhost:6333/dashboard`

You should see the Qdrant dashboard.

### Step 3: Sync Your Data to Qdrant

**Option A: Using Swagger**
1. Start your API
2. Go to `https://localhost:44328/swagger`
3. Find `POST /api/KnowledgeGap/sync-to-qdrant`
4. Click "Try it out" → "Execute"

**Option B: Using curl**
```bash
curl -X POST https://localhost:44328/api/KnowledgeGap/sync-to-qdrant
```

### Step 4: Verify Data in Qdrant

Check collection status:
```bash
curl http://localhost:6333/collections/ai_fund_knowledge
```

Or visit: `http://localhost:6333/dashboard`

---

## 📊 What Gets Stored

Each knowledge entry becomes:

```json
{
  "id": 1,
  "vector": [768 float numbers],
  "payload": {
    "content": "SIP allows you to invest a fixed amount regularly...",
    "question": "What is SIP?",
    "category": "MutualFund",
    "source": "MutualFundKnowledge"
  }
}
```

---

## 🔄 The Complete Flow

```
SQL (MutualFundKnowledge)
        ↓
Read Active Records
        ↓
Generate Embeddings (768 dimensions)
        ↓
Store in Qdrant
        ↓
Ready for Semantic Search!
```

---

## 🧪 Testing

### Check Qdrant Status
```
GET /api/KnowledgeGap/qdrant-status
```

Response:
```json
{
  "collectionExists": true,
  "collectionName": "ai_fund_knowledge",
  "status": "Ready"
}
```

### Sync Data
```
POST /api/KnowledgeGap/sync-to-qdrant
```

Response:
```json
{
  "message": "Knowledge synced to Qdrant successfully"
}
```

---

## ⚙️ Configuration

In `appsettings.json`:

```json
{
  "Qdrant": {
    "Host": "localhost",
    "Port": "6334",
    "CollectionName": "ai_fund_knowledge"
  }
}
```

---

## 🎯 Important Rules

✅ **DO:**
- Ensure embeddings are exactly 768 dimensions
- Sync only active knowledge (IsActive = 1)
- Skip empty content
- Use normalized questions for embeddings

❌ **DON'T:**
- Manually create vectors
- Store null/empty content
- Change vector dimensions

---

## 🔍 Semantic Search Flow

```
User Query: "What is SIP?"
        ↓
Generate Embedding (768 dimensions)
        ↓
Search Qdrant (Cosine Similarity)
        ↓
Get Top 3 Results
        ↓
Build Context
        ↓
Send to LLM
        ↓
Natural Answer
```

---

## 🛠️ Troubleshooting

**Qdrant not starting?**
```bash
docker ps
docker logs qdrant
```

**Collection not found?**
- Run sync endpoint - it auto-creates collection

**Wrong vector size?**
- Sync service validates and regenerates if needed

---

## 📈 Monitoring

- **Qdrant Dashboard**: `http://localhost:6333/dashboard`
- **Collection Info**: `http://localhost:6333/collections/ai_fund_knowledge`
- **API Status**: `GET /api/KnowledgeGap/qdrant-status`

---

## 🚀 Production Tips

1. Use managed Qdrant Cloud for production
2. Enable authentication
3. Set up backups
4. Monitor collection size
5. Implement incremental sync
