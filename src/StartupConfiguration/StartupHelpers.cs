using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace AIWA.API.StartupConfiguration;

public static class StartupHelpers
{
    public static void ConfigureCors(CorsOptions options, bool isDevelopment = false)
    {
        options.AddPolicy
            (
                name: Constants.CORS_POLICY,
                policy =>
                {
                    if (isDevelopment)
                    {
                        policy.AllowAnyMethod();
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Options);
                        policy.WithOrigins(GetRequiredEnvironmentVariable(Constants.CORS_ORIGINS_ENV)!);
                    }

                    policy.WithExposedHeaders(HeaderNames.Location);
                    policy.WithExposedHeaders(HeaderNames.ContentLocation);
                    policy.AllowAnyHeader();
                }
            );
    }


    public static string[] GetRequiredEnvironmentVariable(string arg, char separator = ';')
    {
        var envVar = Environment.GetEnvironmentVariable(arg);
        var envVarArray = envVar?.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if ((envVarArray?.Length ?? 0) == 0)
        {
            throw new InvalidOperationException($"'{arg}' environment variable is not set.");
        }

        return envVarArray!;
    }
}