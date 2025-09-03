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

    // this will actually naturally mean that needs don't decay while an action is updating them,
    // as the timeSinceLastSet can be set to 0 when updated
    // just have to remember to actually do the update in the action planner since Need values must be current there

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Passively decay NPC needs over time
        foreach (var (needs, entity) in
            SystemAPI.Query< DynamicBuffer<NeedBuffer>>()
			.WithAll<NPC>()
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