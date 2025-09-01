using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

// Queries for QueuedActions with completed Pathfind components-
// Checks if entity it is trying to interact with is now free to perform the action
// Remove SocialRequest tag from NPCs,
// Add InUseTag to interaction object, if required
[BurstCompile]
public partial struct QueuedActionHandlerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Pathfinding
        foreach (var (npc, npcTransform, pathfinding, action, entity)
            in SystemAPI.Query<RefRO<NPC>, RefRW<LocalTransform>, RefRW<ActionPathfind>, RefRO<QueuedAction>>()
            .WithEntityAccess())
        {
            if (!pathfinding.ValueRO.DestinationReached)
                continue;

			// Check if target entity has become free
			bool hasInteraction = SystemAPI.HasComponent<Interaction>(pathfinding.ValueRO.DestinationEntity);
            bool hasInUseTag = SystemAPI.HasComponent<InUseTag>(pathfinding.ValueRO.DestinationEntity);
            if (!hasInteraction && !hasInUseTag)
            {
                // Add (or update) Interaction component to the NPC
                // Action Handler System will iterate all Interaction components and execute their function

                // Add InUse tag to the InteractableObject
                // Action Planner will ignore it until it is no longer in use

                // Remove any SocialRequest tag from the InteractableObject (if it is also an NPC)

                InUseTag inUse = new() { InteractingNPC = entity };
                ecb.AddComponent(action.ValueRO.InteractionObject, inUse);

                if (SystemAPI.HasComponent<SocialRequest>(pathfinding.ValueRO.DestinationEntity))
                    ecb.RemoveComponent<SocialRequest>(pathfinding.ValueRO.DestinationEntity);

                Interaction npcIsInteracting = new() { InteractionObject = action.ValueRO.InteractionObject };
                ecb.AddComponent(entity, npcIsInteracting);
            }

            // (AddComponent won't replace an existing component, so if there does end up being an InUseTag already it's fine_
            // (we'll just have the ActionHandlerSystem double-check it's the right entity and remove its Action component if not)

            // If we've waited long enough to exhaust our NPC's patience,
            // Give up and remove action from this entity, and remove SocialRequest from the destination entity if relevant
            if (pathfinding.ValueRO.WaitForTargetToBeFree <= 0.0f)
            {
                ecb.RemoveComponent<ActionPathfind>(entity);
                ecb.RemoveComponent<QueuedAction>(entity);

                if (SystemAPI.HasComponent<SocialRequest>(pathfinding.ValueRO.DestinationEntity))
                    ecb.RemoveComponent<SocialRequest>(pathfinding.ValueRO.DestinationEntity);

                continue;
            }
            pathfinding.ValueRW.WaitForTargetToBeFree -= SystemAPI.Time.DeltaTime;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}