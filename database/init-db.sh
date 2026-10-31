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

echo "==> Initializing database QLCHCF from /database/init.sql..."
/opt/mssql-tools18/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -i /database/init.sql

echo "==> All database initialization tasks completed successfully!"
exit 0
