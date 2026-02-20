# Database Setup Instructions

## Option 1: Using EF Core Migrations (Recommended)

This is the preferred method as it keeps your database schema in sync with your code.

### Prerequisites
- PostgreSQL server running
- Database `forex_realtime` created (see below)
- Connection string configured in `appsettings.json`

### Steps:

1. **Create the database in PgAdmin:**
   ```sql
   CREATE DATABASE forex_realtime;
   ```

2. **Update connection string in `appsettings.json`** if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=forex_realtime;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
   }
   ```

3. **Run migrations from command line:**
   ```bash
   cd "c:\Users\HEMA VATHI\source\repos\ForexRealtime.Api\ForexRealtime.Api"
   dotnet ef database update
   ```

   This will:
   - Create the `price_tick` table
   - Create the `subscription_audit` table
   - Create all indexes
   - Create the `__EFMigrationsHistory` table to track migrations

4. **Verify in PgAdmin:**
   - Right-click on `forex_realtime` database → Refresh
   - Expand Schemas → public → Tables
   - You should see: `price_tick`, `subscription_audit`, and `__EFMigrationsHistory`

---

## Option 2: Manual SQL Script (Alternative)

If you prefer to run SQL directly in PgAdmin:

1. **Create the database:**
   ```sql
   CREATE DATABASE forex_realtime;
   ```

2. **Connect to `forex_realtime` database** in PgAdmin

3. **Open Query Tool** (Tools → Query Tool)

4. **Run the SQL script:**
   - Open `Scripts/create_database.sql` file
   - Copy and paste the contents into Query Tool
   - Execute (F5)

5. **Verify tables:**
   ```sql
   SELECT table_name 
   FROM information_schema.tables 
   WHERE table_schema = 'public' 
   AND table_name IN ('price_tick', 'subscription_audit');
   ```

---

## Database Schema

### Table: `price_tick`
Stores real-time forex price ticks from Finnhub.

| Column | Type | Description |
|--------|------|-------------|
| Id | BIGSERIAL (PK) | Auto-incrementing primary key |
| Symbol | VARCHAR(32) | Forex symbol (e.g., "OANDA:EUR_USD") |
| Price | NUMERIC(18,6) | Last trade price |
| Bid | NUMERIC(18,6) | Bid price |
| Ask | NUMERIC(18,6) | Ask price |
| Ts | TIMESTAMP | Timestamp of the tick |

**Index:** `IX_price_tick_Symbol_Ts` on (Symbol, Ts) for fast queries by symbol and time.

---

### Table: `subscription_audit`
Audit log of user subscription actions (Subscribe/Unsubscribe).

| Column | Type | Description |
|--------|------|-------------|
| Id | BIGSERIAL (PK) | Auto-incrementing primary key |
| UserId | VARCHAR(256) | User identifier |
| Symbol | VARCHAR(32) | Symbol user subscribed/unsubscribed to |
| Action | VARCHAR(32) | "Subscribe" or "Unsubscribe" |
| At | TIMESTAMP | When the action occurred |

**Index:** `IX_subscription_audit_UserId_At` on (UserId, At) for fast queries by user and time.

---

## Troubleshooting

### Connection Error
- Verify PostgreSQL is running
- Check `appsettings.json` connection string matches your PostgreSQL credentials
- Ensure database `forex_realtime` exists

### Migration Error
- Ensure `Npgsql.EntityFrameworkCore.PostgreSQL` package is installed
- Check PostgreSQL version (should be 10+)
- Verify user has CREATE TABLE permissions

### Tables Already Exist
- If tables exist from a previous run, migrations will skip creating them
- To reset: Drop tables manually or use `dotnet ef database drop` (careful: deletes all data)

---

## Next Steps

After database setup:
1. Run the API: `dotnet run`
2. The app will attempt to apply migrations on startup (if not already applied)
3. Test the API endpoints
4. Check tables populate as data flows from Finnhub
