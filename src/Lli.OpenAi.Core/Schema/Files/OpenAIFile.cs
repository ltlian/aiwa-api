namespace Lli.OpenAi.Core.Schema.Files;

public record OpenAIFile
(
    string Id,
    long Bytes,
    long CreatedAt,
    string Filename,
    string Object,
    string Purpose
);
