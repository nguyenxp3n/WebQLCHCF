#!/bin/bash
set -e

: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD environment variable is required}"
SA_PASSWORD="$MSSQL_SA_PASSWORD"

echo "==> Waiting for SQL Server (db) to be ready..."
for i in {1..60}; do
    if /opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "==> SQL Server is ready and accepting connections."
        break
    fi
    echo "    Waiting for SQL Server... ($i/60)"
    sleep 2
done

# Check if database QLCHCF already exists to prevent data loss on container restart
echo "==> Checking if database QLCHCF already exists..."
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'QLCHCF';" | tr -d '[:space:]')

if [ "$DB_EXISTS" = "1" ]; then
    echo "==> Database QLCHCF already exists. Preserving existing transactional data."
else
    echo "==> Initializing fresh database QLCHCF from /database/init.sql..." 
    /opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -b -i /database/init.sql
fi

# Run versioned incremental migrations (safe for both fresh and existing databases)
if [ -d "/database/migrations" ]; then
    for migration_file in /database/migrations/*.sql; do
        if [ -f "$migration_file" ]; then
            echo "==> Applying migration: $migration_file..."
            /opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -b -i "$migration_file"
        fi
    done
fi

echo "==> All database initialization and migration tasks completed successfully!"
exit 0
