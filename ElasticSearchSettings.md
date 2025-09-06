
# Elastic Search Settings

```
{
  "// ========================================": "",
  "// 1. LOCAL DEVELOPMENT (Docker/Local ES)": "",
  "// ========================================": "",
  
  "Elasticsearch": {
    "ConnectionString": "http://localhost:9200",
    "Username": "elastic",
    "Password": "changeme",
    "DefaultIndex": "my-app-dev",
    "IndexPrefix": "dev-",
    "RequestTimeout": "00:01:00",
    "PingTimeout": "00:00:02",
    "MaxRetries": 3,
    "EnableDebugMode": true,
    "DisableDirectStreaming": true,
    "PrettyJson": true,
    "SkipCertificateValidation": true
  },

  "// =======================================": "",
  "// 2. PRODUCTION (Self-hosted with HTTPS)": "",
  "// =======================================": "",
  
  "Elasticsearch_Production": {
    "Nodes": [
      "https://es-node1.company.com:9200",
      "https://es-node2.company.com:9200",
      "https://es-node3.company.com:9200"
    ],
    "Username": "app-user",
    "Password": "${ES_PASSWORD}",
    "DefaultIndex": "my-app-prod",
    "IndexPrefix": "prod-",
    "RequestTimeout": "00:02:00",
    "PingTimeout": "00:00:05",
    "MaxRetries": 5,
    "EnableDebugMode": false,
    "DisableDirectStreaming": false,
    "PrettyJson": false,
    "SkipCertificateValidation": false,
    "ClientCertificatePath": "/certs/client.p12"
  },

  "// ===================================": "",
  "// 3. ELASTIC CLOUD (Managed Service)": "",
  "// ===================================": "",
  
  "Elasticsearch_Cloud": {
    "CloudId": "my-deployment:dXMtZWFzdC0xLmF3cy5mb3VuZC5pbyQxYjc4NGZiNzBiMmU0OWY5YWQ4NzE2YTZkZTk2MzQyYyQ5ZTdkN2Y4YzNiNGY0NzE5YjE2Njk3YzE3NjZlNjU5Ng==",
    "ApiKey": "VnVhQ2ZHY0JDZGJrUW0tZTVhT3g6dWkybHAyYXhUTm1zeWFrdzl0dk5udw==",
    "DefaultIndex": "my-app",
    "RequestTimeout": "00:01:30",
    "PingTimeout": "00:00:03",
    "MaxRetries": 3,
    "EnableDebugMode": false,
    "PrettyJson": false
  },

  "// ====================================": "",
  "// 4. WITH CERTIFICATE FINGERPRINT": "",
  "// ====================================": "",
  
  "Elasticsearch_Secure": {
    "ConnectionString": "https://elasticsearch.company.com:9200",
    "Username": "app-user",
    "Password": "${ES_PASSWORD}",
    "CertificateFingerprint": "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD",
    "DefaultIndex": "secure-app",
    "RequestTimeout": "00:01:00",
    "MaxRetries": 3
  },

  "// =================================": "",
  "// 5. DEVELOPMENT WITH DEBUG INFO": "",
  "// =================================": "",
  
  "Elasticsearch_Debug": {
    "ConnectionString": "http://localhost:9200",
    "Username": "elastic",
    "Password": "dev-password",
    "DefaultIndex": "debug-app",
    "RequestTimeout": "00:05:00",
    "EnableDebugMode": true,
    "DisableDirectStreaming": true,
    "PrettyJson": true,
    "SkipCertificateValidation": true
  },

  "// ===============================": "",
  "// 6. MULTIPLE ENVIRONMENTS": "",
  "// ===============================": "",
  
  "Development": {
    "Elasticsearch": {
      "ConnectionString": "http://localhost:9200",
      "Username": "elastic",
      "Password": "dev-password",
      "DefaultIndex": "app-dev",
      "EnableDebugMode": true,
      "PrettyJson": true,
      "SkipCertificateValidation": true
    }
  },

  "Staging": {
    "Elasticsearch": {
      "Nodes": [
        "https://es-staging1.company.com:9200",
        "https://es-staging2.company.com:9200"
      ],
      "Username": "stage-user",
      "Password": "${STAGING_ES_PASSWORD}",
      "DefaultIndex": "app-staging",
      "RequestTimeout": "00:01:30",
      "SkipCertificateValidation": false
    }
  },

  "Production": {
    "Elasticsearch": {
      "CloudId": "${ELASTIC_CLOUD_ID}",
      "ApiKey": "${ELASTIC_API_KEY}",
      "DefaultIndex": "app-production",
      "RequestTimeout": "00:02:00",
      "MaxRetries": 5,
      "EnableDebugMode": false
    }
  },

  "// ===============================": "",
  "// 7. WITH CUSTOM INDEX PATTERNS": "",
  "// ===============================": "",
  
  "Elasticsearch_MultiIndex": {
    "ConnectionString": "http://localhost:9200",
    "Username": "elastic",
    "Password": "changeme",
    "DefaultIndex": "logs",
    "IndexPrefix": "myapp-",
    "RequestTimeout": "00:01:00",
    "EnableDebugMode": false
  },

  "// ============================": "",
  "// 8. MINIMAL CONFIGURATION": "",
  "// ============================": "",
  
  "Elasticsearch_Minimal": {
    "ConnectionString": "http://localhost:9200"
  },

  "// ===============================": "",
  "// 9. DOCKER COMPOSE SETUP": "",
  "// ===============================": "",
  
  "Elasticsearch_Docker": {
    "ConnectionString": "http://elasticsearch:9200",
    "Username": "elastic",
    "Password": "docker-password",
    "DefaultIndex": "docker-app",
    "RequestTimeout": "00:01:00",
    "EnableDebugMode": true,
    "SkipCertificateValidation": true
  },

  "// =================================": "",
  "// 10. WITH CONNECTION POOLING": "",
  "// =================================": "",
  
  "Elasticsearch_Cluster": {
    "Nodes": [
      "http://es-master:9200",
      "http://es-data1:9200", 
      "http://es-data2:9200",
      "http://es-data3:9200"
    ],
    "Username": "cluster-user",
    "Password": "cluster-password",
    "DefaultIndex": "cluster-app",
    "RequestTimeout": "00:01:00",
    "PingTimeout": "00:00:02",
    "MaxRetries": 3,
    "EnableDebugMode": false
  },

  "// =============================": "",
  "// LOGGING CONFIGURATION": "",
  "// =============================": "",
  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Elastic.Clients.Elasticsearch": "Information"
    }
  },

  "// =============================": "",
  "// ENVIRONMENT VARIABLES USAGE": "",
  "// =============================": "",
  
  "Elasticsearch_EnvVars": {
    "ConnectionString": "${ELASTICSEARCH_URL:http://localhost:9200}",
    "Username": "${ELASTICSEARCH_USERNAME:elastic}",
    "Password": "${ELASTICSEARCH_PASSWORD:changeme}",
    "ApiKey": "${ELASTICSEARCH_API_KEY}",
    "CloudId": "${ELASTIC_CLOUD_ID}",
    "DefaultIndex": "${ELASTICSEARCH_DEFAULT_INDEX:my-app}",
    "CertificateFingerprint": "${ELASTICSEARCH_CERT_FINGERPRINT}",
    "RequestTimeout": "${ELASTICSEARCH_TIMEOUT:00:01:00}",
    "EnableDebugMode": "${ELASTICSEARCH_DEBUG:false}",
    "SkipCertificateValidation": "${ELASTICSEARCH_SKIP_CERT_VALIDATION:false}"
  }
}
```