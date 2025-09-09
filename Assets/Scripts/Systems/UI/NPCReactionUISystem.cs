
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
			.WithNone<ActionPathfind, SocialRequest, InUseTag>()
			.WithEntityAccess())
		{
			// Show Reaction Indicator
			var buffer = state.EntityManager.GetBuffer<Child>(entity);
			if (buffer.Length > 0)
			{
				SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(buffer[0].Value);
				int reactionIndex = (int)interaction.ValueRO.InitiatorReaction;
				if (reactionIndex < 1)
				{
					spriteRenderer.sprite = null;
					continue;
				}
				Sprite reactionSprite = NPCActionIndicator.Instance.GetIndicator(reactionIndex - 1);
				spriteRenderer.sprite = reactionSprite;
			}

			// Show Target Reaction Indicator
			if (interaction.ValueRO.TargetReaction == EEmotionIndicator.None)
				continue;

			var targetBuffer = state.EntityManager.GetBuffer<Child>(interaction.ValueRO.InteractionObject);
			if (targetBuffer.Length > 0)
			{
				SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(targetBuffer[0].Value);
				int reactionIndex = (int)interaction.ValueRO.TargetReaction;
				if (reactionIndex < 1)
				{
					spriteRenderer.sprite = null;
					continue;
				}
				Sprite reactionSprite = NPCActionIndicator.Instance.GetIndicator(reactionIndex - 1);
				spriteRenderer.sprite = reactionSprite;
			}
		}

		// Clear the Reaction sprite of all NPCs outside of interactions,
		// and with no SocialRequest (which indicates they should be set to TargetReaction sprite)
		foreach (var (npc, entity) in
			SystemAPI.Query<RefRO<NPC>>()
			.WithNone<Interaction, SocialRequest, InUseTag>()
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