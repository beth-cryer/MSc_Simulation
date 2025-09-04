using Unity.Entities;

public struct QueuedAction: IComponentData
{
    public Entity InteractionObject;
	public EEmotion Emotion;
	public EEmotionIndicator Reaction;
}