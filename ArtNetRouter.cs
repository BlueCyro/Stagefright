using ArtfullySimple;
using FrooxEngine;

namespace Stagefright;

public class ArtNetRouter
{
    // Int dictionary because you can have non-uniformly-spaced universes e.g. 1..3..6 instead of 1..2..3 sequentially
    private readonly Dictionary<int, List<ValueStream<float>>> universes = new();

    public bool Contains(int universe)
    {
        return universes.ContainsKey(universe);
    }

    public List<ValueStream<float>> SetupUniverse(UniverseInfo info, World world)
    {

        Stagefright.Msg("Creating universe streams");
        List<ValueStream<float>> uniStreams = new();
        for (int i = 0; i < info.Count; i++)
        {
            string streamName = $"{UniverseHelper.UNIVERSE_PREFIX}{info.Universe}:{i + 1}"; // 1-indexing the stream names for consistency
            var stream = world.LocalUser.GetStreamOrAdd<ValueStream<float>>(streamName, s => SetStreamParams(s, streamName));
            uniStreams.Add(stream);
        }

        universes.Add(info.Universe, uniStreams);
        return uniStreams;
    }

    public List<ValueStream<float>>? GetUniverseStreams(int universe)
    {
        universes.TryGetValue(universe, out var streams);
        return streams;
    }

    public IEnumerable<int> GetUniverseIndicies()
    {
        return universes.Keys;
    }

    public void DestroyUniverses()
    {
        foreach (int key in GetUniverseIndicies().ToList())
        {
            DestroyUniverse(key);
        }
    }

    public void DestroyUniverse(int universe)
    {
        if (universes.TryGetValue(universe, out var streams))
        {
            foreach (var stream in streams)
                if (stream != null && !stream.IsDisposed && !stream.IsDestroyed)
                    stream.Destroy();
            
            streams.Clear();
            universes.Remove(universe);
        }
    }

    public void Route(ArtDmxPacket msg)
    {
        var data = msg.DMX.AsSpan();
        if (universes.TryGetValue(msg.Universe + 1, out var streams))
        {
            for (int i = 0; i < streams.Count; i++)
            {   
                var b = data[i];
                var stream = streams[i];
                if (stream != null && !stream.IsDestroyed)
                {
                    stream.Value = b / 255.0f;
                    stream.ForceUpdate();
                }
            }
        }
    }

    private static void SetStreamParams(ValueStream<float> stream, string name)
    {
        stream.Name = name;
        stream.SetInterpolation();
        stream.SetUpdatePeriod(0, 0);
        stream.Encoding = ValueEncoding.Quantized;
        stream.FullFrameBits = 8; // 8 bits since we're just expressing each DMX byte as a quantized float
        stream.FullFrameMin = 0f;
        stream.FullFrameMax = 1f;
    }
}