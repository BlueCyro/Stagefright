using FrooxEngine;
using ArtfullySimple;

namespace Stagefright;

public class Universe
{
    public bool Destroyed { get; private set; }
    public readonly int Address;
    public readonly World World;
    public readonly int ChannelCount;
    public readonly ValueStream<float>[] Streams;
    private EventHandler<ArtDmxPacket> handlerDelegate;
    private readonly ReaderWriterLockSlim slimLock;



    public Universe(World world, int numChannels, int address)
    {
        World = world;
        ChannelCount = numChannels;
        Address = address;
        Streams = new ValueStream<float>[ChannelCount];
        slimLock = new();
        handlerDelegate = SetupRoutingCallback(address); // Assign the callback to a delegate so we can unsubscribe it on destruction
        
        for (int i = 0; i < ChannelCount; i++)
        {
            string streamName = $"{StageHelper.STAGE_PREFIX}{Address}:{i + 1}"; // 1-indexing the stream names for consistency
            Streams[i] = World.LocalUser.GetStreamOrAdd<ValueStream<float>>(streamName, s => SetStreamParams(s, streamName));
        }
        
        
        world.WorldManager.WorldRemoved += Destroy;
        ArtNetBridge.DMXRoute += handlerDelegate;
    }


    // Just so we can match the signature for the world removed event
    public void Destroy(World world)
    {
        Destroy();
    }



    public void Destroy()
    {
        // Acquire exclusive access with write lock
        slimLock.EnterWriteLock();

        // Be careful
        try
        {
            // Destroy all streams
            foreach (var stream in Streams)
            {
                stream.Destroy();
            }

            // Remove this entry from the hashset
            ArtNetBridge.Universes.Remove(this);

            // Unsubscribe our delegate from the DMX routing event. Always unsubscribe your events, kids.
            ArtNetBridge.DMXRoute -= handlerDelegate;
        }
        catch (Exception e)
        {
            Stagefright.Error($"Exception destroying universe for {World.Name}! Exception: {e}");
        }
        finally // Exit the write lock
        {
            slimLock.ExitWriteLock();
        }

        // Now the GC can have it's fun...
        World.WorldManager.WorldRemoved -= Destroy;
    }



    private EventHandler<ArtDmxPacket> SetupRoutingCallback(int address)
    {
        Stagefright.Msg($"Setting up routing callback for address {address}!");
        // Define an inline function that captures 'address' so we don't have to worry about it.
        void routingCallback(object sender, ArtDmxPacket dmx)
        {
            // Check if the packet was meant for us
            if (dmx.Universe + 1 == address)
            {
                // Enter the read lock
                slimLock.EnterReadLock();
                

                // Use a try-catch-finally statement so that we never have a chance to indefinitely hold the read lock
                try
                {
                    // If the universe was already destroyed, just abort the work since the class is either shutting down or is already shut down
                    if (Destroyed)
                        return;
                    

                    // Interpret as a span so we can apply [] indexing
                    Span<byte> dmxData = dmx.DMX.AsSpan();
                    
                    for (int i = 0; i < Streams.Length; i++)
                    {

                        // Make that if we somehow get to this point, that we don't try to operate on invalid streams
                        ValueStream<float> curStream = Streams[i];
                        if (curStream == null || curStream.IsDisposed || curStream.IsDestroyed)
                            return;
                        

                        // Convert the dmxData to a 0.0 - 1.0 float for ease-of-use in DMX fixtures.
                        curStream.Value = dmxData[i] / 255.0f;
                    }
                }
                catch (Exception e)
                {
                    Stagefright.Error($"Exception updating DMX streams in {World.Name}! Exception: {e}");
                }
                finally
                {
                    // Exit the read lock despite any exceptions so that we don't hold it indefinitely
                    slimLock.ExitReadLock();
                }
            }
        }
        
        return routingCallback;
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