-- Forex Real-time API Database Setup Script
-- Run this script in PgAdmin Query Tool

-- ============================================
-- STEP 1: Create the database (run this FIRST)
-- ============================================
-- Right-click on "Databases" in PgAdmin and select "Create" -> "Database"
-- OR run this command (but you cannot run CREATE DATABASE inside a transaction):
-- CREATE DATABASE forex_realtime;

-- ============================================
-- STEP 2: Connect to forex_realtime database
-- ============================================
-- In PgAdmin, expand "Databases" -> right-click "forex_realtime" -> "Query Tool"
-- Then run the following SQL:

-- Create price_tick table
CREATE TABLE price_tick (
    "Id" BIGSERIAL PRIMARY KEY,
    "Symbol" VARCHAR(32) NOT NULL,
    "Price" NUMERIC(18,6) NOT NULL,
    "Bid" NUMERIC(18,6) NOT NULL,
    "Ask" NUMERIC(18,6) NOT NULL,
    "Ts" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Create subscription_audit table
CREATE TABLE subscription_audit (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" VARCHAR(256) NOT NULL,
    "Symbol" VARCHAR(32) NOT NULL,
    "Action" VARCHAR(32) NOT NULL,
    "At" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Create indexes for better query performance
CREATE INDEX "IX_price_tick_Symbol_Ts" ON price_tick ("Symbol", "Ts");
CREATE INDEX "IX_subscription_audit_UserId_At" ON subscription_audit ("UserId", "At");

-- Verify tables were created
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit')
ORDER BY table_name;
