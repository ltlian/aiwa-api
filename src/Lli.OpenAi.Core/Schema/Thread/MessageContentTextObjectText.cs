namespace Lli.OpenAi.Core.Schema.Thread;

public record MessageContentTextObjectText
(
    string Value,
    IMessageContentTextAnnotations[] Annotations
);
