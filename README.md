# ES Insfrastructure instruction

## Introduce

This is my sample project for inplementing Event Sourcing. There maybe some problem and I'm happy to hear your opinions for improvement.

Please email me at custardbruleic@gmail.com.

## Tech stack

Event store - KurrentDB since it have many support or you can try with kafka with custom implementation.

Snapshot DB - about anything fine but I will try Elastich Search

Queue/job - anything seem fit

### noted
- Version of Elastic products 9.0.2 (can choose other but need to shared the same Version)


## TODO

Add "KurrentDB" to appsetting ("kurrentdb://...")

Add ElasticSearchSettings
~~~
{
    "CertPath":"...",
    "Host":"...",
    "UserName":"...",
    "Password":"...",
}
~~~

