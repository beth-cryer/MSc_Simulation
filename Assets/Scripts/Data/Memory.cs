using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(16)]
public struct ShortTermMemoryBuffer: IBufferElementData
{
    public Memory Memory;
}

[InternalBufferCapacity(8)]
public struct LongTermMemoryBuffer : IBufferElementData
{
    public Memory Memory;
}

public struct Memory
{
	public float3 EmotionResponse;
	public double TimeElapsed;
	public EEmotion Type;
}

public struct ShortTermMemory: IComponentData
{
    public float TimeInterval;
    public float TimeSinceLastInterval;
    public int MemoryLimit;
    public bool QueueLongTermMemory;
}

public struct LongTermMemoryPeriod: IBufferElementData
{
    public float3 MemoryPeriodMood;
	public double MemoryTimeElapsed;
    public int LongTermMemoryIndex;
    public int LongTermMemoryCount;
}