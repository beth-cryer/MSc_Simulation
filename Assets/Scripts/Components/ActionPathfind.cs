using Unity.Entities;
using Unity.Mathematics;

public struct ActionPathfind : IComponentData
{
    public float3 Destination;
    public Entity DestinationEntity;
	public float InteractDistance;
    public bool DestinationReached;
    public int RedirectAttempts;
    public float WaitForTargetToBeFree;
}