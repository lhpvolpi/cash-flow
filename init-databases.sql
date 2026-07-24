-- Create databases for Transactions and Consolidation services
CREATE DATABASE transactions;
CREATE DATABASE consolidation;

-- Grant privileges
\connect transactions
GRANT ALL PRIVILEGES ON DATABASE transactions TO postgres;

\connect consolidation
GRANT ALL PRIVILEGES ON DATABASE consolidation TO postgres;
