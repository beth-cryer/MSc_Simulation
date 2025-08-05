using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NeedsDecaySystem : ISystem
{
    //TODO OPTIMIZATION:
    // instead of doing this every frame, just run the calculation whenever it's relevant
    // and check the time elapsed since last update to extrapolate the current Need value

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Passively decay NPC needs over time
        foreach (var (npc, needs, entity) in
            SystemAPI.Query<RefRW<NPC>, DynamicBuffer<NeedBuffer>>()
            .WithEntityAccess())
        {
            // TODO: Read InteractionBuffer and don't decay need if it's being fulfilled

            DynamicBuffer<NeedBuffer> buffer = ecb.SetBuffer<NeedBuffer>(entity);

            // For each NPC Need; decay the value and copy that to ECB, then overwrite the NPC Needs buffer using ECB
            foreach (NeedBuffer need in needs)
            {
                Need alteredNeed = need.Need;
                ref NeedsData needsData = ref blobAsset.Value.NeedsData[(int)need.Need.Type];
                alteredNeed.Value = math.clamp(alteredNeed.Value - needsData.DecayRate * SystemAPI.Time.DeltaTime, needsData.MinValue, needsData.MaxValue);

                buffer.Add(new() { Need = alteredNeed });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}