using Unity.Entities;

public struct ActionSetNeed : IComponentData
{
    public Entity InteractingObject;
    public float Duration;
}