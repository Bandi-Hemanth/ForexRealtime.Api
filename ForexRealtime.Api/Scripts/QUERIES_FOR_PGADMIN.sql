-- ============================================
-- COPY AND PASTE THIS ENTIRE SCRIPT INTO PgAdmin QUERY TOOL
-- ============================================

-- Table 1: price_tick (stores real-time forex price data)
CREATE TABLE price_tick (
    "Id" BIGSERIAL PRIMARY KEY,
    "Symbol" VARCHAR(32) NOT NULL,
    "Price" NUMERIC(18,6) NOT NULL,
    "Bid" NUMERIC(18,6) NOT NULL,
    "Ask" NUMERIC(18,6) NOT NULL,
    "Ts" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Table 2: subscription_audit (logs user subscription actions)
CREATE TABLE subscription_audit (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" VARCHAR(256) NOT NULL,
    "Symbol" VARCHAR(32) NOT NULL,
    "Action" VARCHAR(32) NOT NULL,
    "At" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);

-- Index for price_tick (makes queries faster)
CREATE INDEX "IX_price_tick_Symbol_Ts" ON price_tick ("Symbol", "Ts");

-- Index for subscription_audit (makes queries faster)
CREATE INDEX "IX_subscription_audit_UserId_At" ON subscription_audit ("UserId", "At");

-- Verify: Check if tables were created (should return 2 rows)
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit')
ORDER BY table_name;
