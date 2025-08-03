using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

// Lot of nested iteration over all entities happens here, so we limit it to a less frequent tick rate
[UpdateInGroup(typeof(ActionRefreshSystemGroup))]
public partial struct ActionPlannerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BlobSingleton>();
        state.RequireForUpdate<RandomSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("AI update tick");

        // Get Needs Curves from BlobAsset
        BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
        var randomSingleton = SystemAPI.GetSingletonRW<RandomSingleton>();

        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Iterate over every NPC and process their action planning
        foreach (var (npc, npcTransform, needs, npcEntity)
            in SystemAPI.Query<RefRO<NPC>, RefRO<LocalTransform>, DynamicBuffer<NeedBuffer>>()
            .WithNone<Interaction>()
            .WithEntityAccess())
        {
            float3 npcPos = npcTransform.ValueRO.Position;

            // TODO: Maintain a count of all interactables in a singleton somewhere
            NativeArray<WeightedAction> weights = new(20, Allocator.TempJob);
            int weightCount = 0;
            float sumOfWeights = 0;

            // Loop through each Interactable and check their Advertised Needs and calculate their Utility weight
            foreach (var (obj, objTransform, needsAdvertised, objEntity) in
                SystemAPI.Query<RefRO<InteractableObject>, RefRO<LocalTransform>, DynamicBuffer<NeedAdvertisementBuffer>>()
                .WithNone<InUseTag>()
                .WithEntityAccess())
            {
                // For each need advertised:
                // add (Curve Value of (NPC Need)) * Need Advertised
                // add (100 - NPC Need) * Need Advertised,
                // then * result by (1/Distance)
                float weightedValue = 0f;
                foreach (NeedAdvertisementBuffer needAdvertised in needsAdvertised)
                {
                    //  TODO: WRITE SWITCH FOR CALCULATING WEIGHT OF EMOTION-DEPENDENT NEED
                    //  (WILL BE BASED ON CLOSENESS OF CURRENT NEED VALUE TO NEED ADVERTISED)
                    //  NeedRange - Abs(Distance between Mood values) * Intensity

                    // Get the current value from the NPC of the Need that this is advertising
                    int n = 0;
                    foreach (NeedBuffer findNeed in needs)
                    {
                        if (findNeed.Need.Type == needAdvertised.NeedAdvertised.Type)
                        {
                            break;
                        }
                        n++;
                    }
                    if (n > needs.Length) continue;

                    // Find the corresponding Need Curve in global data blob, and evaluate NPC's current position on the Curve
                    float curveValue = 1.0f;
                    for (int i = 0; i < blobAsset.Value.NeedsData.Length; i++)
                    {
                        if (blobAsset.Value.NeedsData[i].Type == needs[i].Need.Type)
                        {
                            // (needMax - need) / needMax
                            float currentNeedValue = math.clamp(
                                (blobAsset.Value.NeedsData[i].MaxValue - needs[i].Need.Value) / blobAsset.Value.NeedsData[i].MaxValue,
                                0f, 1f)[0];
                            int curveIndex = (int)math.round(currentNeedValue * 25f);
                            curveValue = blobAsset.Value.NeedsData[i].Curve[curveIndex];
                            //Debug.Log(string.Format("currentNeedValue = {0}", currentNeedValue));
                            //Debug.Log(string.Format("curveValue = {0}", curveValue));
                            break;
                        }
                    }

                    /* Value without Curve applied
                     * 
                    float curveValue = 1.0f;
                    for (int i = 0; i < blobAsset.Value.NeedsData.Length; i++)
                    {
                        if (blobAsset.Value.NeedsData[i].Type == needs[i].Need.Type)
                        {
                            curveValue = math.clamp(
                                (blobAsset.Value.NeedsData[i].MaxValue - needs[i].Need.Value) / blobAsset.Value.NeedsData[i].MaxValue,
                                0f, 1f)[0];
                            break;
                        }
                    }*/

                    weightedValue += curveValue * needAdvertised.NeedAdvertised.Value[0];
                }

                // Add distance from NPC
                float3 targetPos = objTransform.ValueRO.Position;
                float distance = math.max(math.distance(npcPos, targetPos), 0.1f); //don't allow division by 0
                //todo - maybe some kind of scaling here? we probably want distance to matter less as we get to larger distances
                // perhaps...another curve, hmm?........
                weightedValue *= (1 / distance);
                sumOfWeights += weightedValue;

                weights[weightCount] = new()
                {
                    InteractableObjectEntity = objEntity,
                    InteractableObject = obj.ValueRO,
                    Buffer = needsAdvertised,
                    Position = objTransform.ValueRO.Position,
                    Weight = weightedValue
                };
                weightCount++;
            }

            //Debug.Log(string.Format("checked {0} possibilites", weightCount));

            // Randomly pick from weighted list
            int randomIndex = PickWeightedValue(ref randomSingleton.ValueRW, ref weights, weightCount, sumOfWeights);

            ActionPathfind pathfind = new() {
                Destination = weights[randomIndex].Position,
            };
            ecb.AddComponent(npcEntity, pathfind);  // note: AddComponent overwrites existing component if there is one

            ActionSetNeed action = new() {
                InteractingObject = weights[randomIndex].InteractableObjectEntity,
                Duration = weights[randomIndex].InteractableObject.InteractDuration
            };
            ecb.AddComponent(npcEntity, action);

            // Copy NeedAdvertisementBuffer onto the NPC now
            // so that ActionHandlerSystem can execute the action without having to do another lookup later
            DynamicBuffer<InteractionBuffer> actionBuffer = ecb.AddBuffer<InteractionBuffer>(npcEntity);
            foreach (NeedAdvertisementBuffer needAdvertised in weights[randomIndex].Buffer)
            {
                actionBuffer.Add(new()
                {
                    NeedAction = needAdvertised.NeedAdvertised,
                    ActionType = needAdvertised.ActionType,
                    MoveTowardsAmount = needAdvertised.MoveTowardsAmount,
                });
            }

            weights.Dispose();
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private readonly int PickWeightedValue(ref RandomSingleton random, ref NativeArray<WeightedAction> weights, int weightCount, float sumOfWeights)
    {
        float r = random.Random.NextFloat() * sumOfWeights;
        for (int i = 0; i < weightCount; i++)
        {
            if (r < weights[i].Weight)
                return i;
            r -= weights[i].Weight;
        }

        return 0;
    }
}

struct WeightedAction
{
    public DynamicBuffer<NeedAdvertisementBuffer> Buffer;
    public InteractableObject InteractableObject;
    public float3 Position;
    public float InteractDuration;
    public float Weight;
    public Entity InteractableObjectEntity;
}