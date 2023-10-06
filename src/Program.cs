using AIWA.API.Integrations.GPT4;

namespace AIWA.API;

public class Program
{
    private const string CORS_POLICY = "CORS_POLICY";
    private const string CORS_ORIGINS_ENV = "CORS_ORIGINS";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddControllers();
        builder.Services.AddCors
        (
            options => options.AddPolicy
            (
                name: CORS_POLICY,
                policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyMethod();
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        var corsOrigins = Environment.GetEnvironmentVariable(CORS_ORIGINS_ENV);
                        var originsArray = corsOrigins?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        if ((originsArray?.Length ?? 0) == 0)
                        {
                            throw new InvalidOperationException($"'{CORS_ORIGINS_ENV}' environment variable is not set or contains no valid origins.");
                        }

                        policy.WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Options);
                        policy.WithOrigins(originsArray!);
                    }

                    policy.AllowAnyHeader();
                }
            )
        );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .AddScoped<UkesMailCompletion>()
            .AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0
        builder.Services
            .AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseCors(CORS_POLICY);
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/healthz");

        app.Run();
    }
}
