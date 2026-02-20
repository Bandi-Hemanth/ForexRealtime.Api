-- ============================================
-- CREATE TABLES ONLY (for forex_realtime database)
-- Run this AFTER creating the database in PgAdmin
-- ============================================

-- Step 1: Create price_tick table
CREATE TABLE price_tick (
    "Id" BIGSERIAL PRIMARY KEY,
    "Symbol" VARCHAR(32) NOT NULL,
    "Price" NUMERIC(18,6) NOT NULL,
    "Bid" NUMERIC(18,6) NOT NULL,
    "Ask" NUMERIC(18,6) NOT NULL,
    "Ts" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Step 2: Create subscription_audit table
CREATE TABLE subscription_audit (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" VARCHAR(256) NOT NULL,
    "Symbol" VARCHAR(32) NOT NULL,
    "Action" VARCHAR(32) NOT NULL,
    "At" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Step 3: Create index on price_tick for faster queries
CREATE INDEX "IX_price_tick_Symbol_Ts" ON price_tick ("Symbol", "Ts");

-- Step 4: Create index on subscription_audit for faster queries
CREATE INDEX "IX_subscription_audit_UserId_At" ON subscription_audit ("UserId", "At");

-- Step 5: Verify tables were created successfully
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit')
ORDER BY table_name;
