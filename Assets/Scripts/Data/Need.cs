using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Need
{
    public ENeed Type;
    public float Value;
}

public enum ENeed
{
    Hunger,
    Shelter,
    Sleep,
    Social
}

[InternalBufferCapacity(4)]
public struct NeedsBuffer : IBufferElementData
{
    public Need Need;
}

[InternalBufferCapacity(4)]
public struct NeedAdvertisementsBuffer : IBufferElementData
{
    public Need Need;
}