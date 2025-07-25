using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

public interface IActionComponent
{

}

public struct ActionPathfind : IComponentData, IActionComponent
{
    public float3 Destination;
}