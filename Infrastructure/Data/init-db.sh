#!/bin/bash

# Wait for SQL Server to be ready with retry logic
echo "Waiting for SQL Server to start..."
for i in {1..60}; do
    if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Kioliki24 -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "SQL Server is ready!"
        break
    fi
    echo "Attempt $i: SQL Server not ready yet, waiting 2 seconds..."
    sleep 2
done

# Create QuartzJob database
echo "Creating QuartzJob database..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Kioliki24 -C -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QuartzJob') CREATE DATABASE QuartzJob;"

# Replace the placeholder in quartzSchema.sql and execute it
echo "Running Quartz schema initialization..."
sed 's/\[enter_db_name_here\]/QuartzJob/g' /tmp/quartzSchema.sql | /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Kioliki24 -C -d QuartzJob

echo "Quartz database initialized successfully"
