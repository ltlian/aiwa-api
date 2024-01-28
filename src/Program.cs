using System.Net.Mime;
using AIWA.API.Data;
using AIWA.API.Data.EF;
using AIWA.API.Integrations.GPT;
using AIWA.API.StartupConfiguration;
using Lli.OpenAi.Core.Client;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace AIWA.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddApplicationInsightsTelemetry();

        //builder.Services.AddHostedService<DebugHostedService>();

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHostFiltering(o =>
            {
                o.AllowedHosts = StartupHelpers.GetRequiredEnvironmentVariable(Constants.ALLOWED_HOSTS_ENV);
            });
        }
        else
        {
            builder.Services.AddHttpLogging(ConfigureHttpLoggingOptions);
        }

        builder.Services.AddCors(s => StartupHelpers.ConfigureCors(s, builder.Environment.IsDevelopment()));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IVisionService, VisionService>()
            .AddScoped<ISpeechService, SpeechService>()
            .AddScoped<IUkesmailCompletion, UkesmailCompletionService>();

        builder.Services.AddDbContext<AiwaSQLiteContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString(nameof(AiwaSQLiteContext))));
        builder.Services.AddScoped<IDataStore, AiwaEFDataStore>();

        builder.Services.AddHttpClient<IOpenAiHttpClient, OpenAiHttpClient >();

        builder.Services
            .AddAntiforgery()
            .AddOptions<OpenAIHttpClientOptions>()
            .BindConfiguration(nameof(OpenAIHttpClientOptions));

        builder.Services.AddSingleton<IStreamCache, StreamCache>();

        builder.Services
            .AddScoped<IChatCompletion, ChatCompletionService>()
            .AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0
        builder.Services.AddHealthChecks();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseCors(Constants.CORS_POLICY);
        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapHealthChecks("/healthz");

        // Initialize database on application start
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiwaSQLiteContext>();
        dbContext.Database.Migrate();
    }

    private static void ConfigureHttpLoggingOptions(HttpLoggingOptions options)
    {
        // Log basic request and response information
        options.LoggingFields =
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseHeaders;

        // Specify headers to log
        options.RequestHeaders.Add(HeaderNames.ContentType);
        options.ResponseHeaders.Add(HeaderNames.ContentType);

        // Optionally log request and response bodies for specific content types
        options.MediaTypeOptions.AddText(MediaTypeNames.Application.Json);
        options.RequestBodyLogLimit = 4096; // Limit body size to log
        options.ResponseBodyLogLimit = 4096;
    }
}
