# Simple Checklist: Create Tables in PgAdmin

## ✅ Checklist

- [ ] **Step 1**: Open PgAdmin
- [ ] **Step 2**: Connect to PostgreSQL server (enter password if needed)
- [ ] **Step 3**: Create database `forex_realtime`
- [ ] **Step 4**: Open Query Tool for `forex_realtime` database
- [ ] **Step 5**: Copy SQL from `create_tables_only.sql` and execute
- [ ] **Step 6**: Verify tables exist (should see 2 tables: price_tick, subscription_audit)

---

## Detailed Steps

### ✅ Step 1: Open PgAdmin
- Launch PgAdmin from Start menu

### ✅ Step 2: Connect to Server
- Expand **Servers** in left panel
- Right-click your PostgreSQL server → **Connect Server**
- Enter PostgreSQL password (default: `postgres`)
- Click **OK**

### ✅ Step 3: Create Database
1. Right-click **Databases** → **Create** → **Database...**
2. **Name**: `forex_realtime`
3. **Owner**: `postgres` (default)
4. Click **Save**

### ✅ Step 4: Open Query Tool
1. Expand **Databases** → **forex_realtime**
2. Right-click **forex_realtime** → **Query Tool**
3. A SQL editor window opens

### ✅ Step 5: Run SQL Script
1. Open file: `Scripts/create_tables_only.sql`
2. **Copy ALL the SQL** (from CREATE TABLE to the SELECT statement)
3. **Paste** into Query Tool in PgAdmin
4. Click **Execute** button (or press **F5**)
5. Check the **Messages** tab - should show:
   - `CREATE TABLE`
   - `CREATE TABLE`
   - `CREATE INDEX`
   - `CREATE INDEX`
   - Query returned 2 rows

### ✅ Step 6: Verify
In Query Tool, run:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit');
```

Should return 2 rows.

---

## If You Get Errors

**"relation already exists"**
→ Table already created, skip that command

**"syntax error"**
→ Make sure you copied the ENTIRE SQL (all 5 CREATE statements)

**"permission denied"**
→ Make sure you're connected as `postgres` user

**"database does not exist"**
→ Go back to Step 3 and create the database first

---

## What Gets Created

1. **price_tick** table - stores forex price data
2. **subscription_audit** table - logs user subscriptions
3. **2 indexes** - for faster queries

---

## After Tables Are Created

1. Update `appsettings.json` connection string with your PostgreSQL password
2. Run: `dotnet run`
3. Test at: `https://localhost:7050/index.html`
