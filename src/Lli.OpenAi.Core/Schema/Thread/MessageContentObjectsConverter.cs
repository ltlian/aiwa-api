using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class MessageContentObjectsConverter : JsonConverter<IMessageContentObject[]>
{
    public override IMessageContentObject[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var contents = new List<IMessageContentObject>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            Utf8JsonReaderHelpers.EnsureDiscriminatorProperty(ref reader, Utf8JsonReaderHelpers.TypeDiscriminator);
            reader.Read();

            if (reader.ValueTextEquals(MessageContentTextObject.DiscriminatorValue))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("text"))
                {
                    var contentObject = ReadTextContent(ref reader);
                    contents.Add(contentObject);
                }
                else
                {
                    throw new JsonException("Unexpected state");
                }
            }
            else if (reader.ValueTextEquals(MessageContentImageFileObject.DiscriminatorValue))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("image_file"))
                {
                    var contentObject = ReadTextContent(ref reader);
                    contents.Add(contentObject);
                }
                else
                {
                    throw new JsonException("Unexpected state");
                }

                //var imageFile = JsonSerializer.Deserialize<ImageFileDetails>(ref reader, options);
                //contents.Add(new MessageContentImageFileObject(imageFile!));
            }
            else
            {
                throw new JsonException("Unknown content type.");
            }

            //switch (propertyType)
            //{
            //    case "image_file":
            //        var imageFile = JsonSerializer.Deserialize<ImageFileDetails>(ref reader, options);
            //        contents.Add(new MessageContentImageFileObject(imageFile!));
            //        break;
            //    case "text":
            //        var contentObject = ReadTextContent(ref reader);
            //        contents.Add(contentObject);
            //        break;
            //    default:
            //        throw new JsonException("Unknown content type.");
            //}

            // Skip to the end of the current object
            //while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) ;
        }

        return [.. contents];
    }

    private MessageContentTextObject ReadTextContent(ref Utf8JsonReader reader)
    {
        string value = "";
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("value") && reader.Read())
                {
                    value = reader.GetString() ?? throw new JsonException("Text content value is empty.");
                }
                else if (reader.ValueTextEquals("annotations"))
                {
                    reader.Skip();
                }
            }
        }

        reader.Read();

        return new MessageContentTextObject(new MessageContentTextObjectText(value, []));
    }

    private MessageContentTextObject ReadImageContent(ref Utf8JsonReader reader)
    {
        string value = "";
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("file_id") && reader.Read())
                {
                    value = reader.GetString() ?? throw new JsonException("file_id is empty.");
                }
                else
                {
                    throw new JsonException($"Unexpected value: {reader.GetString()}");
                }
            }
        }

        reader.Read();

        return new MessageContentTextObject(new MessageContentTextObjectText(value, []));
    }

    public override void Write(Utf8JsonWriter writer, IMessageContentObject[] value, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewSerializingResponsePayloadsNotSupportedException();
    }
}
