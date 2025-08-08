using Unity.Collections;
using Unity.Entities;

public struct InteractableObject: IComponentData
{
    public FixedString32Bytes Name;
}

public struct ObjectActionBuffer : IBufferElementData
{
    public Entity ActionEntity;
}