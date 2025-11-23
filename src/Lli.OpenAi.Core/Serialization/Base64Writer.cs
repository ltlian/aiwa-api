namespace Lli.OpenAi.Core.Serialization;

public static class Base64Writer
{
    public static string GetBase64String(Stream fs)
    {
        using BinaryReader br = new(fs);
        var imageBytes = br.ReadBytes((int)fs.Length);
        return Convert.ToBase64String(imageBytes);
    }
}
