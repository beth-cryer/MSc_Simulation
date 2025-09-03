using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PublicDeadlockSystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		EntityCommandBuffer ecb = new(Allocator.TempJob);

		// Pathfinding
		foreach (var (socialRequest, pathfinding, action, entity)
			in SystemAPI.Query<RefRO<SocialRequest>, RefRW<ActionPathfind>, RefRO<QueuedAction>>()
			.WithAll<NPC>()
			.WithEntityAccess())
		{
			if (!SystemAPI.HasComponent<SocialRequest>(action.ValueRO.InteractionObject))
				continue;

			if (socialRequest.ValueRO.DeadlockResolved)
				continue;

			ecb.RemoveComponent<SocialRequest>(entity);
			var otherSocialRequest = SystemAPI.GetComponent<SocialRequest>(action.ValueRO.InteractionObject);
			otherSocialRequest.DeadlockResolved = true;

		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}