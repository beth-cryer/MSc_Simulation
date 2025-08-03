using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Need
{
    public ENeed Type;
    public float3 Value;
}

public enum ENeed
{
    Hunger,
    Shelter,
    Sleep,
    Social,
    Mood
}

[InternalBufferCapacity(8)]
public struct NeedBuffer : IBufferElementData
{
    public Need Need;
}

[InternalBufferCapacity(8)]
public struct NeedAdvertisementBuffer : IBufferElementData
{
    public Need NeedAdvertised;
    public EActionType ActionType;
    public float3 MoveTowardsAmount;
    public float InteractDuration;
}