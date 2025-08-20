using Unity.Entities;
using Unity.Mathematics;

// Interaction: Added to an NPC entity while they are performing an Action.
public struct Interaction: IComponentData
{
    public Entity InteractionObject;
    public float TimeElapsed;
}

// Stores the Needs affected by the Action and the Value so that ActionHandlerSystem can modify them
[InternalBufferCapacity(8)]
public struct InteractionBuffer : IBufferElementData
{
    public Need NeedAction;
    public EActionType ActionType;
    public float3 NeedValueChange; // amount to move Need value by (per second). ignored if 0
    public float InteractDuration;
    public float MinInteractDuration;
    public bool RequiredToCompleteAction;
    public bool Complete;
}

public enum EActionType
{
    SetNeed,    // (only set value at end of action) eg. social actions
    //MoveTowards, // (move towards value by x amount per second)
    ModifyNeed, // (modify need value by x amount per second) eg. sleeping, eating
}

public struct ActionAdvertisementBuffer : IBufferElementData
{
    public int NeedAdvertisedIndex;
    public int NeedAdvertisedCount;
}