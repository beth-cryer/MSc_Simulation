using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NeedsDecaySystem : ISystem
{
    float3 min, max;
    float3 decayRate;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // 120 seconds to decay from 100 to 0
        decayRate = 60.0f / 120.0f;
        min = 0.0f;
        max = 100.0f;
    }


    //TODO OPTIMIZATION:
    // instead of doing this every frame, just run the calculation whenever it's relevant
    // and check the time elapsed since last update to extrapolate the current Need value

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Passively decay NPC needs over time
        foreach (var (npc, needs, entity) in
            SystemAPI.Query<RefRW<NPC>, DynamicBuffer<NeedBuffer>>()
            .WithEntityAccess())
        {
            DynamicBuffer<NeedBuffer> buffer = ecb.SetBuffer<NeedBuffer>(entity);

            // For each NPC Need; decay the value and copy that to ECB, then overwrite the NPC Needs buffer using ECB
            foreach (NeedBuffer need in needs)
            {
                Need alteredNeed = need.Need;
                alteredNeed.Value = math.clamp(alteredNeed.Value - decayRate * SystemAPI.Time.DeltaTime, min, max);

                buffer.Add(new() { Need = alteredNeed });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}