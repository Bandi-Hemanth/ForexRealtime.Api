-- Verification Script - Run this after setting up the database
-- This confirms all tables and indexes are created correctly

-- Check if tables exist
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('price_tick', 'subscription_audit', '__EFMigrationsHistory')
ORDER BY table_name;

-- Check columns in price_tick table
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    numeric_precision,
    numeric_scale,
    is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' 
AND table_name = 'price_tick'
ORDER BY ordinal_position;

-- Check columns in subscription_audit table
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' 
AND table_name = 'subscription_audit'
ORDER BY ordinal_position;

-- Check indexes
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
AND tablename IN ('price_tick', 'subscription_audit')
ORDER BY tablename, indexname;

-- Count rows (should be 0 initially)
SELECT 
    'price_tick' as table_name,
    COUNT(*) as row_count
FROM price_tick
UNION ALL
SELECT 
    'subscription_audit' as table_name,
    COUNT(*) as row_count
FROM subscription_audit;
