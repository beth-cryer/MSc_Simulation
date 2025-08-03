using Unity.Entities;
using Unity.Mathematics;

public interface IActionComponent
{

}

public struct ActionPathfind : IComponentData, IActionComponent
{
    public bool DestinationReached;
    public float3 Destination;
}