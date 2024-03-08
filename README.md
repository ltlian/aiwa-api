# AIWA-API

AIWA (AI web application) API is a simple .NET8 webapi which integrates with OpenAI.

## Running locally

This project depends on the `Lli.Org.OpenAi.Core` package which is currently unavailable.

The OpenAI integration depends on the configuration value `OpenAIHttpClientOptions:OpenAIKey` to be resolved by `Microsoft.Extensions.Options` in `Program.cs`.

One alternative is to use user-secrets:

```console
dotnet user-secrets --project src/AIWA.API.csproj init
dotnet user-secrets --project src/AIWA.API.csproj set OpenAIHttpClientOptions:OpenAIKey "your-key"
```
