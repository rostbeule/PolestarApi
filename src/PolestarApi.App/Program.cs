using PolestarApi.App;

var builder = WebApplication.CreateBuilder(args);

// Provide configuration to Startup class
Startup.Configuration = builder.Configuration;

// Call ConfigureServices from Startup to register services
Startup.ConfigureServices(builder.Services);

var webApplication = builder.Build();

// Call Configure from Startup to set up the middleware pipeline
Startup.Configure(webApplication);

webApplication.Run();