using System;
using Unity.Entities;

struct InteractableObject: IComponentData
{
    public InteractableObjectData Data;
}

[Serializable]
public struct InteractableObjectData
{
    
}

[InternalBufferCapacity(4)]
public struct NeedsBuffer: IBufferElementData
{
    public Need Need;
}

[InternalBufferCapacity(4)]
public struct NeedAdvertisementsBuffer : IBufferElementData
{
    public Need Need;
}

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