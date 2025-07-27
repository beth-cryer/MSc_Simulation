using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct ActionHandlerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Look for NPC Entities with certain Action Tags and execute those actions

        EntityCommandBuffer ecb = new(Allocator.TempJob); //using ECB to queue structural changes

        // ActionSetNeed

        // ActionSocial

        // Pathfinding
        foreach (var (npc, npcTransform, pathfinding, entity)
            in SystemAPI.Query<RefRO<NPC>, RefRW<LocalTransform>, RefRO<ActionPathfind>>()
            .WithEntityAccess())
        {
            /*
            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IActionComponent).IsAssignableFrom(p));
            */

            float3 npcPos = npcTransform.ValueRW.Position;
            float3 targetPos = pathfinding.ValueRO.Destination;

            // If we have reached destination, remove the ActionPathfind component
            float distanceToTarget = math.distance(npcPos, targetPos);
            if (distanceToTarget <= 0.01f)
            {
                ecb.RemoveComponent<ActionPathfind>(entity);
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