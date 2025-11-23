namespace AIWA.API.StartupConfiguration;

public static class StartupHelpers
{
    public static string[] GetRequiredEnvironmentVariable(string arg, char separator = ';')
    {
        if (Environment.GetEnvironmentVariable(arg) is not string envVar)
        {
            throw new InvalidOperationException($"Environment variable '{arg}' is not set.");
        }

        var envVarArray = envVar.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if (envVarArray.Length == 0)
        {
            throw new InvalidOperationException($"Environment variable '{arg}' is empty.");
        }

        return envVarArray;
    }
}