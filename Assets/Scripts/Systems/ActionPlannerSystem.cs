using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Lot of nested iteration over all entities happens here, so we limit it to a less frequent tick rate
[UpdateInGroup(typeof(ActionRefreshSystemGroup))]
public partial struct ActionPlannerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BlobSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("AI update tick");

        // Get Needs Curves from BlobAsset
        BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;

        // Iterate over every NPC and process their action planning
        foreach (var (npc, npcTransform, needs)
            in SystemAPI.Query<RefRO<NPC>, RefRO<LocalTransform>, DynamicBuffer<NeedsBuffer>>())
        {
            float3 npcPos = npcTransform.ValueRO.Position;

            // TEMPORARY: create fixed weights list here to store object weights
            // this will need to be big enough for every InteractableObject in the game......
            NativeArray<WeightedAction> weights = new(10, Allocator.Temp);
            int weightCount = 0;

            foreach (var (obj, objTransform, needsAdvertised, objEntity) in
                SystemAPI.Query<RefRO<InteractableObject>, RefRO<LocalTransform>, DynamicBuffer<NeedAdvertisementsBuffer>>()
                .WithNone<InUseTag>()
                .WithEntityAccess())
            {
                // For each need advertised:
                // add (Curve Value of (NPC Need))
                // add (100 - NPC Need) * Need Advertised,
                // then * result by (1/Distance)
                float weightedValue = 0f;
                foreach (NeedAdvertisementsBuffer need in needsAdvertised)
                {
                    // Get the current value from the NPC of the Need that this is advertising
                    Nullable<Need> currentNeed = null;
                    foreach (NeedsBuffer n in needs)
                    {
                        if (n.Need.Type == need.Need.Type)
                            currentNeed = n.Need;
                    }
                    if (currentNeed == null) continue;

                    float currentNeedValue = currentNeed.Value.Value / 100f;
                    int curveIndex = (int)(currentNeedValue * 25.0f);
                    float curveValue = 0;

                    // Find the corresponding Need Curve in global data blob, and evaluate NPC's current position on the Curve
                    for (int i = 0; i < blobAsset.Value.NeedsData.Length; i++)
                    {
                        if (blobAsset.Value.NeedsData[i].Type == currentNeed.Value.Type)
                        {
                            curveValue = blobAsset.Value.NeedsData[i].Curve[curveIndex];
                        }
                    }

                    weightedValue += curveValue;
                    //weightedValue += (100.0f - currentNeed.Value.Value) * need.Need.Value;
                }

                //Add distance from NPC
                float3 targetPos = objTransform.ValueRO.Position;
                float distance = math.distance(npcPos, targetPos);
                //todo - maybe some kind of scaling here? we probably want distance to matter less as we get to larger distances
                // perhaps...another curve........
                weightedValue *= (1 / distance);

                weights[weightCount] = new() {
                    InteractableObject = objEntity,
                    Weight = weightedValue
                };
                weightCount++;

            }

            // NOTE: IGNORE THIS - ONLY ADD INTERACTION + INUSE WHEN THE NPC HAS FINISHED TRAVELLING TO THE OBJECT

            // Add (or update) Interaction component to the NPC
            // Action Handler System will iterate all Interaction components and execute their function

            // Add InUse tag to the InteractableObject
            // Action Planner will ignore it until it is no longer in use
        }
    }
}

struct WeightedAction
{
    public Entity InteractableObject;
    public float Weight;
}