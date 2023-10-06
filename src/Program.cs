using AIWA.API.Integrations.GPT4;

namespace AIWA.API;

public class Program
{
    private const string CORS_POLICY = "CORS_POLICY";

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
                        policy.WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Options);
                        policy.WithOrigins("http://127.0.0.1:5173");
                        policy.WithOrigins("https://happy-rock-0ca806f03.3.azurestaticapps.net");
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
