
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NPCReactionUISystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (needs, actions, interaction, entity) in
					SystemAPI.Query<DynamicBuffer<NeedBuffer>, DynamicBuffer<InteractionBuffer>, RefRW<Interaction>>()
					.WithAll<NPC, ShortTermMemoryBuffer>()
					.WithNone<ActionPathfind>()
					.WithEntityAccess())
		{
			// Show Reaction Indicator
			//NPCActionIndicator reactionUI = state.EntityManager.GetComponentObject<NPCActionIndicator>(entity);
			//reactionUI.SetIndicator((int)interaction.ValueRO.Reaction);


			// TODO: Create entities for each type of indicator, from the list of prefabs provided
			// instead of doin it the hacky non-ECS way here :p
		}
	}
}