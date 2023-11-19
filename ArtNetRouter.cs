using Elements.Core;
using FrooxEngine;

namespace Stagefright;

public class ArtNetRouter
{
    // Int dictionary because you can have non-uniformly-spaced universes e.g. 1..3..6 instead of 1..2..3 sequentially
    private readonly Dictionary<int, List<ValueStream<float>>> universes = new();
    private readonly byte[] prevDmx = new byte[512];

    public World World
    {
        get
        {
            ArtNetBridge.routers.TryGetFirst(this, out World world);
            world = world ?? throw new NullReferenceException($"World is null!! This router is not part of {typeof(ArtNetBridge).GetNiceName()}");
            return world; 
        }
    }

    public bool ContainsUniverse(int universe)
    {
        return universes.ContainsKey(universe);
    }

    public bool SetupUniverse(UniverseInfo info, out List<ValueStream<float>> uniStreams, bool destroyExisting = true)
    {
        if (destroyExisting)
            DestroyUniverse(info.Universe);

        Stagefright.Msg($"Router instantiating universe {info.Universe} with {info.Count} streams");
        bool alreadyExists = universes.TryGetValue(info.Universe, out uniStreams);
        uniStreams ??= new();

        if (alreadyExists)
        {
            Stagefright.Msg("Universe already exists");
            return alreadyExists;
        }
        else
        {
            Stagefright.Msg("Creating universe streams");
            for (int i = 0; i < info.Count; i++)
            {
                string streamName = $"{ArtNetBridge.UNIVERSE_PREFIX}{info.Universe}:{i + 1}"; // 1-indexing the stream names for consistency
                var stream = World.LocalUser.GetStreamOrAdd<ValueStream<float>>(streamName, s => SetStreamParams(s, streamName));
                uniStreams.Add(stream);
            }

            universes.Add(info.Universe, uniStreams);
            return alreadyExists;
        }
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

    public void Route(ArtNetMessage msg)
    {
        if (msg.IsValid && msg.Op == ArtOpCode.OpDmx)
        {
            Span<byte> data = msg.DMXData;

            if (universes.TryGetValue(msg.Universe, out var streams))
            {
                for (int i = 0; i < streams.Count; i++)
                {
                    if (streams[i] != null && !streams[i].IsDestroyed)
                    {
                        streams[i].Value = data[i] / 255.0f;
                        streams[i].ForceUpdate();
                    }
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