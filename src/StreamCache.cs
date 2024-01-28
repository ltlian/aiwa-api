using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API;

public interface IStreamCache
{
    IMemoryCache Cache { get; }
}

public class StreamCache : IStreamCache
{
    public IMemoryCache Cache { get; } = new MemoryCache
    (
        new MemoryCacheOptions()
        //{
        //    SizeLimit = 1024
        //}
    );
}