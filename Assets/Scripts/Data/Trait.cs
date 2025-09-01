using Unity.Entities;

[InternalBufferCapacity(4)]
public struct TraitBuffer: IBufferElementData
{
	public ETrait Type;

	// When running ActionPlanner, modify current value of Need by this amount
	// Eg. a Sleepy NPC would be treated as if they have a lower Sleep value
	public Need NeedModifier;
}

public enum ETrait
{
	None = 0,
	Sleepy,
	Glutton,
	Outdoorsy,
	Shutin,
}