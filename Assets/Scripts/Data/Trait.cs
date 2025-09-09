using Unity.Entities;

[InternalBufferCapacity(4)]
public struct TraitBuffer: IBufferElementData
{
	public Trait Trait;
}

public struct Trait
{
	public ETrait Type;

	// When running ActionPlanner, modify current value of Need by this amount
	// Eg. a Sleepy NPC would be treated as if they have a lower Sleep value
	public bool ModifyNeed;
	public Need NeedModifier;

	// For Mood-modifying traits, the NeedModifier specifies the target Mood value,
	// then MoodModifyAmount is the distance that the NPC's current Mood travels towards the Target before advertisedValue calculation
	public EEmotion MoodModifyEmotion;
	public float MoodModifyAmount;

	// For traits that add an extra Need to the NPC, this specifies that Need type and start value
	public bool AddNeed;
	public Need AddNeedType;
}

public enum ETrait
{
	Sleepy,
	Glutton,
	Outdoorsy,
	Shutin,
	Annoying,
	Dancer,
	Speedy,
	Slow,
	Angry,
	Cold,
	Positive,
	COUNT,
}