using Unity.Collections;
using Unity.Entities;

public struct QueuedAction: IComponentData
{
	public FixedString32Bytes Name;
    public Entity InteractionObject;
	public EEmotion InitiatorEmotion;
	public EEmotion TargetEmotion;
	public EEmotionIndicator InitiatorReaction;
	public EEmotionIndicator TargetReaction;
}