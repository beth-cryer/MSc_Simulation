using Unity.Entities;

// InUse: Added to an InteractableObject entity while they are being used by an NPC to perform an Action
public struct InUseTag : IComponentData
{
    public Entity InteractingNPC;
}