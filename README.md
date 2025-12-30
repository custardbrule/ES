# ES Insfrastructure instruction

## Introduce

This is my sample project for inplementing Event Sourcing. There maybe some problem and I'm happy to hear your opinions for improvement.

Please email me at custardbruleic@gmail.com.

## Tech stack

Event store - KurrentDB since it have many support or you can try with kafka with custom implementation.

Snapshot DB - about anything fine but I will try Elastich Search

Background job [Quartz .NET](https://www.quartz-scheduler.net/)

### noted

- Version of Elastic products 9.0.2 (can choose other but need to shared the same Version)

## TODO

Add "KurrentDB" to appsetting ("kurrentdb://...")

Add ElasticSearchSettings

```
"ElasticSearchSettings": {
    "CertPath":"...",
    "Host":"...",
    "UserName":"...",
    "Password":"...",
}
```

Add [Quartz](https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#using)
Quartz DB [Links](https://github.com/quartznet/quartznet/tree/1644b15832f75042a8f6900af16b5cde652a553e/database/tables)

## Database Migrations

This project uses **EF Core Migrations** for database schema management.

### Working with Migrations

**Create a new migration:**
```bash
cd API/User.Api
dotnet ef migrations add YourMigrationName --project "..\..\Infrastructure\Infras.User.Services\Infras.User.Services.csproj" --startup-project . --context UserDbContext
```

**Apply migrations to database:**
Migrations are automatically applied on application startup via `MigrateAsync()` in Program.cs.

**Remove last migration (if not applied):**
```bash
dotnet ef migrations remove --project "..\..\Infrastructure\Infras.User.Services\Infras.User.Services.csproj" --startup-project . --context UserDbContext
```

**Migration files location:**
`Infrastructure/Infras.User.Services/Migrations/`

Add KafkaSettings

```
"KafkaSettings": {
  "ProducerSetting": {
    "BootstrapServers": "localhost:9092",
    "ClientId": "my-dotnet-app",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "Plain",
    "SaslUsername": "username",
    "SaslPassword": "password",
    "EnableIdempotence": true,
    "MessageTimeoutMs": 30000,
    "RequestTimeoutMs": 30000,
    "RetryBackoffMs": 100,
    "MessageSendMaxRetries": 3,
    "BatchSize": 16384,
    "LingerMs": 5,
    "CompressionType": "snappy"
  }
}
```
