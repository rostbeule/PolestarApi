using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PolestarApi.Authentication;
using PolestarApi.Contracts.Abstractions;
using Scalar.AspNetCore;

namespace PolestarApi.App;

/// <summary>
/// Represents the startup configuration for the Polestar API application.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Gets the application configuration settings.
    /// </summary>
    public static IConfiguration Configuration { get; set; } = null!;

    /// <summary>
    /// Configures services required for the application.
    /// </summary>
    /// <param name="services">The service collection to which services are added.</param>
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register core framework services
        services.AddControllers();

        // Add API versioning
        services.ConfigureApiVersioning();

        // Add OpenAPI and Swagger services
        services.ConfigureOpenApiDocumentation();

        // Register application-specific services
        services.RegisterApplicationServices();
    }

    /// <summary>
    /// Configures the application middleware and request pipeline.
    /// </summary>
    /// <param name="application">The web application to configure.</param>
    public static void Configure(WebApplication application)
    {
        // Enable HTTPS redirection (optional)
        // application.UseHttpsRedirection();

        // Enable CORS middleware
        application.ConfigureCors();

        // Add routing middleware
        application.UseRouting();

        // Configure Swagger to serve OpenAPI JSON documentation
        application.ConfigureSwaggerMiddleware();

        // Map API controllers to routes
        application.MapControllers();

        // Map Scalar API reference for interactive documentation
        application.ConfigureScalar();
    }

    /// <summary>
    /// Configures API versioning for the application.
    /// </summary>
    /// <param name="services">The service collection to which API versioning is added.</param>
    private static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    /// <summary>
    /// Configures Swagger and OpenAPI services for API documentation generation.
    /// </summary>
    /// <param name="services">
    /// The service collection to which Swagger and OpenAPI services are added.
    /// </param>
    /// <remarks>
    /// This method sets up Swagger for generating interactive API documentation and integrates OpenAPI 
    /// for defining the API's contract. It includes XML comments from the project to enhance the 
    /// documentation and provides metadata such as the API title, version, and description. The OpenAPI 
    /// specification is used to define the API's endpoints, parameters, and responses in a standardized 
    /// format.
    /// </remarks>
    private static void ConfigureOpenApiDocumentation(this IServiceCollection services)
    {
        // Registers OpenAPI services
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            // Include XML comments for enhanced documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            // Add metadata for the OpenAPI document
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Polestar API",
                    Version = "v1",
                    Description = "API documentation for Polestar",
                });
        });
    }

    /// <summary>
    /// Registers application-specific services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which services are added.</param>
    private static void RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuthService, AuthService>();
    }

    /// <summary>
    /// Configures Cross-Origin Resource Sharing (CORS) for the application.
    /// </summary>
    /// <param name="application">The web application to configure.</param>
    private static void ConfigureCors(this WebApplication application)
    {
        application.UseCors(builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }

    /// <summary>
    /// Configures middleware to serve Swagger documentation.
    /// </summary>
    /// <param name="application">The web application to configure.</param>
    private static void ConfigureSwaggerMiddleware(this WebApplication application)
    {
        application.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });
    }

    /// <summary>
    /// Configures the Scalar API for serving interactive API reference documentation.
    /// </summary>
    /// <param name="application">The web application to configure.</param>
    private static void ConfigureScalar(this WebApplication application)
    {
        application.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);
        });
    }
}
