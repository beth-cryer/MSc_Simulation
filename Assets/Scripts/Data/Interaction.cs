using Unity.Entities;

// Interaction: Added to an NPC entity while they are performing an Action.
public struct Interaction: IComponentData
{
    public Entity InteractionObject;
	public EEmotion Emotion;
	public EEmotionIndicator Reaction;
	public float TimeElapsed;
}

// Stores the Needs affected by the Action and the Value so that ActionHandlerSystem can modify them
[InternalBufferCapacity(8)]
public struct InteractionBuffer : IBufferElementData
{
	public Action Details;
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
	public EEmotion EmotionAdvertised;
	public EEmotionIndicator Reaction;
}