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
{
    "CertPath":"...",
    "Host":"...",
    "UserName":"...",
    "Password":"...",
}
```

Add [Quartz](https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#using)
Quartz DB [Links](https://github.com/quartznet/quartznet/tree/1644b15832f75042a8f6900af16b5cde652a553e/database/tables)
