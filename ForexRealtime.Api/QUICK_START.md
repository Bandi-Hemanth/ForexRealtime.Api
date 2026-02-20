# Quick Start Guide - Database Setup

## Step-by-Step Setup

### 1. Create Database in PgAdmin

1. Open **PgAdmin**
2. Connect to your PostgreSQL server (usually `localhost` on port `5432`)
3. Right-click on **Databases** → **Create** → **Database**
4. Name: `forex_realtime`
5. Owner: `postgres` (or your PostgreSQL user)
6. Click **Save**

### 2. Update Connection String

Edit `appsettings.json` and update the connection string if your PostgreSQL credentials differ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=forex_realtime;Username=postgres;Password=YOUR_PASSWORD"
}
```

**Common connection string formats:**
- Default PostgreSQL: `Host=localhost;Database=forex_realtime;Username=postgres;Password=postgres`
- Custom port: `Host=localhost;Port=5433;Database=forex_realtime;Username=postgres;Password=postgres`
- SSL required: `Host=localhost;Database=forex_realtime;Username=postgres;Password=postgres;SSL Mode=Require`

### 3. Apply Database Schema

**Option A: Using EF Migrations (Recommended)**

Open terminal/PowerShell in the project folder:
```bash
cd "c:\Users\HEMA VATHI\source\repos\ForexRealtime.Api\ForexRealtime.Api"
dotnet ef database update
```

**Option B: Manual SQL in PgAdmin**

1. In PgAdmin, right-click on `forex_realtime` database → **Query Tool**
2. Open `Scripts/create_database.sql`
3. Copy all SQL and paste into Query Tool
4. Click **Execute** (or press F5)

### 4. Verify Setup

Run `Scripts/verify_setup.sql` in PgAdmin Query Tool to confirm:
- Tables exist (`price_tick`, `subscription_audit`)
- Columns are correct
- Indexes are created

### 5. Run the Application

```bash
dotnet run
```

The app will:
- Connect to PostgreSQL
- Verify migrations (or apply them if needed)
- Start Finnhub WebSocket ingestion
- Begin broadcasting price updates every 500ms

### 6. Test

1. Open browser: `https://localhost:7050/index.html`
2. Login with any username (e.g., `testuser`)
3. Subscribe to a symbol (e.g., `OANDA:EUR_USD`)
4. Watch real-time price updates appear every ~500ms

---

## Troubleshooting

**"Connection refused" or "Database does not exist"**
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Ensure database `forex_realtime` exists

**"Table already exists"**
- This is fine if you ran migrations before
- Tables will be reused, no data loss

**"Permission denied"**
- Ensure your PostgreSQL user has CREATE TABLE permissions
- Try running as `postgres` superuser

**No price updates appearing**
- Check Finnhub API key in `appsettings.json`
- Verify symbols are correct (e.g., `OANDA:EUR_USD`)
- Check application logs for WebSocket connection errors

---

## Database Tables Overview

### `price_tick`
Stores real-time price data from Finnhub. Rows are inserted every 500ms per symbol.

### `subscription_audit`
Logs every Subscribe/Unsubscribe action by users. Useful for analytics and debugging.

---

## Next Steps

- Monitor tables in PgAdmin as data flows
- Query recent ticks: `SELECT * FROM price_tick ORDER BY "Ts" DESC LIMIT 100;`
- View subscription history: `SELECT * FROM subscription_audit ORDER BY "At" DESC;`
