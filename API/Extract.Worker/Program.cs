using Infras.Extract.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.ConfigExtractInfras(builder.Configuration);
builder.Build().Run();
