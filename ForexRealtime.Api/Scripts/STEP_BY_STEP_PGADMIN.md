# Step-by-Step Guide: Creating Tables in PgAdmin

## Prerequisites
- PgAdmin installed and running
- PostgreSQL server running (usually on localhost:5432)
- You know your PostgreSQL password (default is often `postgres`)

---

## Step 1: Open PgAdmin

1. Launch **PgAdmin** from your Start menu or desktop shortcut
2. Enter your **master password** if prompted (this is PgAdmin's password, not PostgreSQL's)

---

## Step 2: Connect to PostgreSQL Server

1. In the left panel, expand **Servers**
2. You should see your PostgreSQL server (e.g., "PostgreSQL 15" or "localhost")
3. If it's not connected, right-click it → **Connect Server**
4. Enter your PostgreSQL password when prompted (default: `postgres`)
5. Click **OK**

---

## Step 3: Create the Database

1. In the left panel, right-click on **Databases**
2. Select **Create** → **Database...**
3. In the **General** tab:
   - **Database name**: `forex_realtime`
   - **Owner**: Leave as `postgres` (or select your PostgreSQL user)
4. Click **Save**
5. You should now see `forex_realtime` under Databases in the left panel

---

## Step 4: Open Query Tool

1. In the left panel, expand **Databases**
2. Expand **forex_realtime**
3. Right-click on **forex_realtime** database
4. Select **Query Tool**
5. A new tab/window will open with a SQL editor

---

## Step 5: Run the SQL Script

### Option A: Copy-Paste Method

1. Open the file `Scripts/create_database.sql` in Notepad or any text editor
2. **IMPORTANT**: Skip the CREATE DATABASE line (lines 1-7) - we already created the database
3. Copy everything from line 11 onwards (starting with `-- Create price_tick table`)
4. Paste into the Query Tool in PgAdmin
5. Click the **Execute** button (or press **F5**)
6. You should see messages like:
   - `CREATE TABLE` (for price_tick)
   - `CREATE TABLE` (for subscription_audit)
   - `CREATE INDEX` (for both indexes)
   - A result showing 2 rows (the two table names)

### Option B: Run Commands One by One

If you prefer to run commands individually, execute these one at a time:

**Command 1: Create price_tick table**
```sql
CREATE TABLE price_tick (
    "Id" BIGSERIAL PRIMARY KEY,
    "Symbol" VARCHAR(32) NOT NULL,
    "Price" NUMERIC(18,6) NOT NULL,
    "Bid" NUMERIC(18,6) NOT NULL,
    "Ask" NUMERIC(18,6) NOT NULL,
    "Ts" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
```

**Command 2: Create subscription_audit table**
```sql
CREATE TABLE subscription_audit (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" VARCHAR(256) NOT NULL,
    "Symbol" VARCHAR(32) NOT NULL,
    "Action" VARCHAR(32) NOT NULL,
    "At" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
```

**Command 3: Create index on price_tick**
```sql
CREATE INDEX "IX_price_tick_Symbol_Ts" ON price_tick ("Symbol", "Ts");
```

**Command 4: Create index on subscription_audit**
```sql
CREATE INDEX "IX_subscription_audit_UserId_At" ON subscription_audit ("UserId", "At");
```

**Command 5: Verify tables exist**
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit')
ORDER BY table_name;
```

---

## Step 6: Verify Tables Were Created

### Method 1: Using SQL Query
Run this in Query Tool:
```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit')
ORDER BY table_name;
```

You should see 2 rows:
- `price_tick`
- `subscription_audit`

### Method 2: Using PgAdmin UI
1. In the left panel, expand **Databases** → **forex_realtime** → **Schemas** → **public** → **Tables**
2. You should see:
   - `price_tick`
   - `subscription_audit`
3. Right-click on `price_tick` → **View/Edit Data** → **All Rows** to see the table structure

---

## Step 7: Verify Table Structure

### Check price_tick columns:
```sql
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'price_tick'
ORDER BY ordinal_position;
```

Expected columns:
- `Id` (bigint, PRIMARY KEY, auto-increment)
- `Symbol` (varchar 32, NOT NULL)
- `Price` (numeric 18,6, NOT NULL)
- `Bid` (numeric 18,6, NOT NULL)
- `Ask` (numeric 18,6, NOT NULL)
- `Ts` (timestamp, NOT NULL)

### Check subscription_audit columns:
```sql
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'subscription_audit'
ORDER BY ordinal_position;
```

Expected columns:
- `Id` (bigint, PRIMARY KEY, auto-increment)
- `UserId` (varchar 256, NOT NULL)
- `Symbol` (varchar 32, NOT NULL)
- `Action` (varchar 32, NOT NULL)
- `At` (timestamp, NOT NULL)

---

## Common Errors and Solutions

### Error: "relation already exists"
**Solution**: The table already exists. You can either:
- Drop it first: `DROP TABLE price_tick CASCADE;` (careful: deletes all data)
- Or skip creating it if it already exists

### Error: "syntax error at or near..."
**Solution**: 
- Make sure you're connected to the `forex_realtime` database (not `postgres`)
- Check for typos in column names
- Ensure all quotes match (use double quotes `"` for identifiers)

### Error: "permission denied"
**Solution**: 
- Make sure you're logged in as a user with CREATE TABLE permissions
- Try connecting as `postgres` superuser

### Error: "database does not exist"
**Solution**: 
- Go back to Step 3 and create the `forex_realtime` database first
- Make sure you're connected to the correct database in Query Tool

---

## Next Steps

After tables are created:

1. **Update connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=forex_realtime;Username=postgres;Password=YOUR_PASSWORD"
   }
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Test**: Open `https://localhost:7050/index.html` and subscribe to symbols

---

## Quick Reference: Table Structures

### price_tick
- Stores real-time forex price data
- Auto-increments `Id`
- Indexed on `(Symbol, Ts)` for fast queries

### subscription_audit
- Logs user subscription actions
- Auto-increments `Id`
- Indexed on `(UserId, At)` for fast queries
