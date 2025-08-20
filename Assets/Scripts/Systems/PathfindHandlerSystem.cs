using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            .WithEntityAccess())
        {
            if (pathfinding.ValueRO.DestinationReached)
                continue;

            float3 npcPos = npcTransform.ValueRW.Position;
            float3 targetPos = pathfinding.ValueRO.Destination;

            // If we have reached destination, remove the ActionPathfind component
            float distanceToTarget = math.distance(npcPos, targetPos);
            if (distanceToTarget <= 0.01f)
            {
                ecb.RemoveComponent<ActionPathfind>(entity);

                if (action.ValueRO.InteractionObject == Entity.Null)
                    continue;
                // If the target already has InUse component, remove this entity's QueuedAction components (someone else got there first)
                if (SystemAPI.HasComponent<InUseTag>(action.ValueRO.InteractionObject))
                {
                    ecb.RemoveComponent<QueuedAction>(entity);
                    continue;
                }

                // Otherwise:
                // Add (or update) Interaction component to the NPC
                // Action Handler System will iterate all Interaction components and execute their function

                // Add InUse tag to the InteractableObject
                // Action Planner will ignore it until it is no longer in use

                InUseTag inUse = new() { InteractingNPC = entity };
                ecb.AddComponent(action.ValueRO.InteractionObject, inUse);

                Interaction npcIsInteracting = new() { InteractionObject = action.ValueRO.InteractionObject };
                ecb.AddComponent(entity, npcIsInteracting);

                // (AddComponent won't replace an existing component, so if there does end up being an InUseTag already it's fine_
                // (we'll just have the ActionHandlerSystem double-check it's the right entity and remove its Action component if not)

                continue;
            }

            float3 newPos;
            MathHelpers.MoveTowards(ref npcPos, ref targetPos, npc.ValueRO.Speed * SystemAPI.Time.DeltaTime, out newPos);
            npcTransform.ValueRW.Position = newPos;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}