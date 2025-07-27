using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DebugAddComponentSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            EntityCommandBuffer ecb = new(Allocator.TempJob);

            // Get all selected NPCs (should only be the one)
            foreach (var (npc, selected, entity)
               in SystemAPI.Query<RefRO<NPC>, RefRO<SelectedEntityTag>>()
               .WithEntityAccess())
            {
                // Do fun debug stuff
                ActionPathfind pathfind = new()
                {
                    Destination = new float3(10, 10, 10)
                };

                ecb.AddComponent<ActionPathfind>(entity, pathfind);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
