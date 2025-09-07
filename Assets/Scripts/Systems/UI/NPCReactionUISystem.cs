
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NPCReactionUISystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (needs, interaction, entity) in
			SystemAPI.Query<DynamicBuffer<NeedBuffer>, RefRW<Interaction>>()
			.WithNone<ActionPathfind>()
			.WithEntityAccess())
		{
			var buffer = state.EntityManager.GetBuffer<Child>(entity);

			if (buffer.Length <= 0)
				continue;

			// Show Reaction Indicator
			SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(buffer[0].Value);

			int reactionIndex = (int)interaction.ValueRO.InitiatorReaction;
			if (reactionIndex < 1)
			{
				spriteRenderer.sprite = null;
				continue;
			}
			Sprite reactionSprite = NPCActionIndicator.Instance.GetIndicator(reactionIndex - 1);
			spriteRenderer.sprite = reactionSprite;

			// TODO: Create entities for each type of indicator, from the list of prefabs provided
			// instead of doin it the hacky non-ECS way here :p
		}

		foreach (var (npc, entity) in
			SystemAPI.Query<RefRO<NPC>>()
			.WithNone<Interaction>()
			.WithEntityAccess())
		{
			var buffer = state.EntityManager.GetBuffer<Child>(entity);

			if (buffer.Length <= 0)
				continue;

			// Hide Reaction Indicator
			SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(buffer[0].Value);
			spriteRenderer.sprite = null;
		}
	}
}