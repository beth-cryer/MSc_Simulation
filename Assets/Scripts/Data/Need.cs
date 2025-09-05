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
    Mood,
    Fun,
}

[InternalBufferCapacity(8)]
public struct NeedBuffer : IBufferElementData
{
    public Need Need;
}

[InternalBufferCapacity(8)]
public struct NeedAdvertisementBuffer : IBufferElementData
{
	public Action Details;
}

public struct Action
{
	public Need Need;
	public EActionType ActionType;
	public float3 NeedValueChange; // amount to move Need value by (per second). ignored if 0
	public EEmotion InitiatorEmotion;
	public EEmotion TargetEmotion;
	public float InteractDuration;
	public float MinInteractDuration;
	public bool RequiredToCompleteAction;
}