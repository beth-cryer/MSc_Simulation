using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PathfindHandlerSystem : ISystem
{

	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Pathfinding
        foreach (var (npc, npcTransform, pathfinding, action, entity)
            in SystemAPI.Query<RefRO<NPC>, RefRW<LocalTransform>, RefRW<ActionPathfind>, RefRO<QueuedAction>>()
			.WithNone<SocialRequest>()
            .WithEntityAccess())
        {
            if (pathfinding.ValueRO.DestinationReached)
                continue;

            float3 npcPos = npcTransform.ValueRW.Position;
            float3 targetPos = pathfinding.ValueRO.Destination;

            // Keep moving to target until in range for action
            float distanceToTarget = math.distance(npcPos, targetPos);
            if (distanceToTarget > pathfinding.ValueRO.InteractDistance)
            {
                float3 newPos;
                MathHelpers.MoveTowards(ref npcPos, ref targetPos, npc.ValueRO.Speed * SystemAPI.Time.DeltaTime, out newPos);
                npcTransform.ValueRW.Position = newPos;
                continue;
            }

            Debug.Log("At destination");

            // Check if the target moved since Pathfind component was created,
            var targetPositionCurrent = SystemAPI.GetComponent<LocalTransform>(action.ValueRO.InteractionObject);
            //Debug.Log("Me " + entity.Index.ToString() + ", Them " + action.ValueRO.InteractionObject.Index.ToString());

            if (math.distance(targetPos, targetPositionCurrent.Position) > pathfinding.ValueRO.InteractDistance)
            {
                // If so, try pathfind to it again until RedirectionAttempts are all used
                if (pathfinding.ValueRO.RedirectAttempts > 0)
                {
                    Debug.Log("Redirect attempt");
                    pathfinding.ValueRW.RedirectAttempts--;
                    pathfinding.ValueRW.Destination = targetPositionCurrent.Position;
                }
                else
                {
                    // Give up pathfinding after too many attempts
                    ecb.RemoveComponent<ActionPathfind>(entity);
                    ecb.RemoveComponent<QueuedAction>(entity);
                }
                continue;
            }

            // We are at TargetDestination-

            // Set Destination Reached, then either
            // Start the interaction now
            // Or, wait for the target entity to become free
            if (!pathfinding.ValueRO.DestinationReached)
            {
                // If the target has an Interaction component or InUseTag, wait a little while for it to be done
                // And if it's an NPC, add a SocialRequest tag -
                // This halts ActionPlannerSystem from running on it, allowing us to start the social interaction immediately
                bool targetHasInteraction = SystemAPI.HasComponent<Interaction>(action.ValueRO.InteractionObject);
                bool targetHasInUseTag = SystemAPI.HasComponent<InUseTag>(action.ValueRO.InteractionObject);
                bool targetIsNPC = SystemAPI.HasComponent<NPC>(action.ValueRO.InteractionObject);
                if (targetIsNPC)
                {
                    SocialRequest socialRequest = new();
                    ecb.AddComponent(action.ValueRO.InteractionObject, socialRequest);
                }
                else
                {
                    ecb.RemoveComponent<ActionPathfind>(entity);
                    ecb.RemoveComponent<QueuedAction>(entity);
                }

                if (!targetHasInteraction && !targetHasInUseTag)
                {
                    // Add (or update) Interaction component to the NPC
                    // Action Handler System will iterate all Interaction components and execute their function

                    // Add InUse tag to the InteractableObject
                    // Action Planner will ignore it until it is no longer in use

                    // Remove any SocialRequest tag from the InteractableObject (if it is also an NPC)

                    InUseTag inUse = new() { InteractingNPC = entity };
                    ecb.AddComponent(action.ValueRO.InteractionObject, inUse);

                    Interaction npcIsInteracting = new() { InteractionObject = action.ValueRO.InteractionObject };
                    ecb.AddComponent(entity, npcIsInteracting);

                    // (AddComponent won't replace an existing component, so if there does end up being an InUseTag already it's fine_
                    // (we'll just have the ActionHandlerSystem double-check it's the right entity and remove its Action component if not)
                }

                pathfinding.ValueRW.DestinationReached = true;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}