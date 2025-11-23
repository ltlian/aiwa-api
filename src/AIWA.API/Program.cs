using AIWA.API.Data;
using AIWA.API.Integrations.GPT;
using AIWA.API.StartupConfiguration;
using AIWA.EntityFrameworkCore;

using Lli.OpenAi.Core.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

namespace AIWA.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        builder.Services.AddHealthChecks();

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHostFiltering(o =>
            {
                o.AllowedHosts = StartupHelpers.GetRequiredEnvironmentVariable(Constants.ALLOWED_HOSTS_ENV);
            });
        }

        builder.Services.AddCors(s => s.ConfigureCors(Constants.CORS_POLICY, builder.Environment.IsDevelopment()));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddScoped<IVisionService, VisionService>()
            .AddScoped<ISpeechService, SpeechService>()
            .AddScoped<IUkesmailCompletion, UkesmailCompletionServiceFake>();

        builder.Services.AddPooledDbContextFactory<AiwaDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
        builder.Services.AddScoped<IDataStore, AiwaEFDataStore>();

        builder.Services.AddHttpClient<IOpenAiHttpClient, OpenAiHttpClient>();

        builder.Services
            .AddAntiforgery()
            .AddOptions<OpenAIHttpClientOptions>()
            .BindConfiguration(nameof(OpenAIHttpClientOptions));

        builder.Services.AddSingleton<IStreamCache, StreamCache>();

        builder.Services
            .AddScoped<IChatCompletion, ChatCompletionServiceFake>()
            .AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseCors(Constants.CORS_POLICY);

        app.MapHealthChecks("healthz", new HealthCheckOptions
        {
            ResponseWriter = (httpContext, report) =>
            {
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            }
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.MapControllers();

        app.Run();
    }
}
