using Unity.Entities;

// Interaction: Added to an NPC entity while they are performing an Action.
public struct InteractionTag: IComponentData
{
    //public Entity InteractionObject; //maybe not needed
}

// Stores the Needs affected by the Action and the Value to modify them by each simulation tick.
[InternalBufferCapacity(4)]
public struct InteractionBuffer : IBufferElementData
{
    public Need InteractionNeed;
    public float InteractionValue;
}