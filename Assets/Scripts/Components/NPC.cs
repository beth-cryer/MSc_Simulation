
using Unity.Collections;
using Unity.Entities;

public struct NPC : IComponentData
{
    public FixedString32Bytes Name;
    public float Speed;
    public Entity Location;
}