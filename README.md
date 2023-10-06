# AIWA-API

AIWA (AI web application) API is a simple .NET8 webapi which integrates with OpenAI.

## Running locally

The OpenAI integration depends on the configuration value `OpenAIOptions:OpenAIKey` to be resolved by `Microsoft.Extensions.Options` in `Program.cs`.

One alternative is to use user-secrets:

```console
dotnet user-secrets --project src/AIWA.API.csproj init
dotnet user-secrets --project src/AIWA.API.csproj set OpenAIOptions:OpenAIKey "your-key"
```
