using System.Net.Mime;
using System.Net.WebSockets;
using AIWA.API.Audio;
using AIWA.API.Data;
using AIWA.API.Hubs;
using AIWA.API.Integrations.GPT;
using AIWA.API.Integrations.GPT4;
using AIWA.API.StartupConfiguration;
using Lli.OpenAi.Core.Client;
using MessagePack;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;

namespace AIWA.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        //builder.Services.AddHostedService<DebugHostedService>();

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        #region SignalR

        builder.Services
            .AddSignalR()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    //.WithResolver(new CustomResolver())
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            }); ;

        #endregion

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHostFiltering(o =>
            {
                o.AllowedHosts = StartupHelpers.GetRequiredEnvironmentVariable(Constants.ALLOWED_HOSTS_ENV);
            });
        }
        else
        {
            builder.Services.AddHttpLogging(ConfigureHttpLoggingOptions);
        }

        builder.Services.AddCors(s => StartupHelpers.ConfigureCors(s, builder.Environment.IsDevelopment()));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IVisionService, VisionService>();
        builder.Services.AddScoped<ISpeechService, SpeechService>();

        //builder.Services.AddScoped<IDataStore, SqLiteStore>();
        builder.Services.AddSingleton<IDataStore, InMemoryStore>();

        builder.Services.AddHttpClient<OpenAiHttpClient>();

        builder.Services
            .AddAntiforgery()
            .AddOptions<OpenAIHttpClientOptions>()
            .BindConfiguration(nameof(OpenAIHttpClientOptions));

        builder.Services.AddSingleton<IStreamCache, StreamCache>();

        builder.Services
            .AddScoped<IChatCompletion, ChatCompletionService>()
            .AddOptions<OpenAIOptions>()
            .BindConfiguration(nameof(OpenAIOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0
        builder.Services.AddHealthChecks();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseCors(Constants.CORS_POLICY);
        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.UseStaticFiles(); // Enables static file serving

        app.MapControllers();

        // SignalR
        //app.MapHub<ChatHub>("/chatHub");
        //app.MapHub<AudioHub>("/audioHub");

        // Websockets
        //app.UseWebSockets();

        //app.Use(async (context, next) =>
        //{
        //    if (context.WebSockets.IsWebSocketRequest)
        //    {
        //        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //        //await HandleWebSocketAsync(context, webSocket);
        //        await HandleWebSocketSimpleAsync(context, webSocket);
        //    }
        //    else
        //    {
        //        await next();
        //    }
        //});

        app.MapHealthChecks("/healthz");
    }

    //private static async Task HandleWebSocketSimpleAsync(HttpContext context, WebSocket webSocket)
    //{
    //    var buffer = new byte[4096]; // Define buffer size
    //    Stream audioStream = AudioHub.GetAudioStream(); // Your method to get the audio stream

    //    while (audioStream.CanRead)
    //    {
    //        int bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length);
    //        if (bytesRead == 0) break; // End of stream

    //        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, true, CancellationToken.None);
    //    }

    //    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stream end", CancellationToken.None);
    //}

    private static async Task HandleWebSocketSimpleAsync(HttpContext context, WebSocket webSocket)
    {
        Stream audioStream = AudioHub.GetAudioStream(); // Your method to get the audio stream
        while (audioStream.CanRead)
        {
            var frame = await Mp3Frame.LoadFromStreamAsync(audioStream, true);
            if (frame == null || frame.FileOffset == 0)
            {
                break; // End of stream
            }

            var asgm = new ArraySegment<byte>(frame.RawData, 0, frame.FrameLength);
            
            await webSocket.SendAsync(asgm, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        //byte[] headerBytes = new byte[4];
        //int bytesRead = audioStream.Read(headerBytes, 0, headerBytes.Length);
        //if (bytesRead < headerBytes.Length)
        //{
        //    // TODO end stream
        //}

        //var buffer = new byte[4096]; // Define buffer size

        //while (audioStream.CanRead)
        //{
        //    int bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length);
        //    if (bytesRead == 0) break; // End of stream

        //    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), WebSocketMessageType.Binary, true, CancellationToken.None);
        //}

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stream end", CancellationToken.None);
    }

    //private static async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
    //{
    //    var buffer = new byte[4096]; // Define buffer size
    //    Stream audioStream = AudioHub.GetAudioStream(); // Your method to get the audio stream

    //    while (audioStream.CanRead && !webSocket.CloseStatus.HasValue)
    //    {
    //        // Attempt to read the header
    //        //int headerBytesRead = await audioStream.ReadAsync(buffer.AsMemory(0, 4));
    //        //if (headerBytesRead < 4) break; // End of stream

    //        //var frame = Mp3Frame.LoadFromStream(audioStream, false) ?? throw new InvalidOperationException("Invalid frame");
    //        //if (frame.FileOffset > int.MaxValue)
    //        //{
    //        //    throw new InvalidOperationException("Invalid frame offset");
    //        //}

    //        //int frameLength = frame.FrameLength;

    //        //// Ensure the buffer can hold the entire frame
    //        //if (frameLength > buffer.Length)
    //        //{
    //        //    // Resize buffer if needed (this should be done with caution and possibly with a maximum size check)
    //        //    buffer = new byte[frameLength];
    //        //}

    //        //// Read the rest of the frame
    //        ////var mem = buffer.AsMemory(0, frameLength);
    //        ////var mem = buffer.AsMemory((int)frame.FileOffset, frameSize);
    //        ////int frameBytesRead = await audioStream.ReadAsync(mem, context.RequestAborted);
    //        //int frameBytesRead = await audioStream.ReadAsync(buffer, 0, frameLength, context.RequestAborted);
    //        //var seg = new ArraySegment<byte>(buffer, 0, frameLength);
    //        //// Send the frame
    //        //await webSocket.SendAsync(seg, WebSocketMessageType.Binary, true, CancellationToken.None);
    //        //if (frameBytesRead < frameLength - 4) break; // End of stream or read error
    //    }

    //    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stream end", CancellationToken.None);
    //}

    // This method needs to be fully implemented based on the MP3 frame header structure
    //private static int CalculateFrameSize(byte[] header)
    //{
    //    // Parse the header to calculate frame size
    //    // This involves interpreting the MPEG version, layer, bit rate index, sample rate index, padding bit, etc.
    //    // Refer to the `Mp3Frame.cs` logic for detailed implementation
    //    return 0; // placeholder
    //}

    private static void ConfigureHttpLoggingOptions(HttpLoggingOptions options)
    {
        // Log basic request and response information
        options.LoggingFields =
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseHeaders;

        // Specify headers to log
        options.RequestHeaders.Add(HeaderNames.ContentType);
        options.ResponseHeaders.Add(HeaderNames.ContentType);

        // Optionally log request and response bodies for specific content types
        options.MediaTypeOptions.AddText(MediaTypeNames.Application.Json);
        options.RequestBodyLogLimit = 4096; // Limit body size to log
        options.ResponseBodyLogLimit = 4096;
    }
}
