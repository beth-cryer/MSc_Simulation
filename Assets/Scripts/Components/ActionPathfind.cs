using Unity.Entities;
using Unity.Mathematics;

public interface IActionComponent
{

}

public struct ActionPathfind : IComponentData, IActionComponent
{
    public float3 Destination;
    public Entity DestinationEntity;
    public bool DestinationReached;
}