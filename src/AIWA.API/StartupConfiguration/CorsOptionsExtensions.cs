using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace AIWA.API.StartupConfiguration;

public static class CorsOptionsExtensions
{
    extension(CorsOptions options)
    {
        public void ConfigureCors(string name, bool isDevelopment = false)
        {
            options.AddPolicy(
                name,
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
                        policy.WithOrigins(StartupHelpers.GetRequiredEnvironmentVariable(Constants.CORS_ORIGINS_ENV));
                    }

                    policy.WithHeaders(HeaderNames.ContentType);
                }
            );
        }
    }
}
