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

            // Loop through each Interactable, check their Advertised Needs and calculate their Utility weight
            foreach (var (obj, objTransform, actionsAdvertised, needsAdvertised, objEntity) in
                SystemAPI.Query<RefRO<InteractableObject>, RefRO<LocalTransform>, DynamicBuffer<ActionAdvertisementBuffer>, DynamicBuffer <NeedAdvertisementBuffer>>()
                .WithNone<InUseTag, ActionPathfind>()
                .WithEntityAccess())
            {
                // For each need advertised:
                // add (Curve Value of (NPC Need)) * Need Advertised
                // add (100 - NPC Need) * Need Advertised,
                // then * result by (1/Distance)
                foreach (ActionAdvertisementBuffer actionAdvertised in actionsAdvertised)
                {
                    float weightedValue = 0f;

                    //Loop through the Needs advertised by the Action
                    for (int i = actionAdvertised.NeedAdvertisedIndex;
                        i < actionAdvertised.NeedAdvertisedIndex + actionAdvertised.NeedAdvertisedCount;
                        i++)
                    {
                        NeedAdvertisementBuffer needAdvertised = needsAdvertised[i];

                        // Get the current value from the NPC of the Need that this is advertising
                        Nullable<Need> currentNeed = null;
                        foreach (NeedBuffer findNeed in needs)
                        {
                            if (findNeed.Need.Type == needAdvertised.NeedAdvertised.Type)
                            {
                                currentNeed = findNeed.Need;
                                break;
                            }
                        }

                        ref var needData = ref blobAsset.Value.NeedsData[(int)needAdvertised.NeedAdvertised.Type];

                        if (!currentNeed.HasValue) continue; //if needAdvertised doesn't exist on the NPC, skip it
                        if (currentNeed.Value.Value[0] == needData.MaxValue[0]) continue; //if need is already at max value, skip it

                        // Value should be the value per second spent doing the action
                        // So get the projected end Need value of the Action, and divide by Duration
                        float advertisedValue = 0.0f;
                        float3 endResult = 0.0f;
                        switch (needAdvertised.ActionType)
                        {
                            case (EActionType.SetNeed):
                                if (needAdvertised.InteractDuration == 0) continue; // if this happens then the interaction is configured wrong
                                endResult = needAdvertised.NeedAdvertised.Value;
                                advertisedValue = endResult[0] / needAdvertised.InteractDuration;
                                break;
                            case (EActionType.ModifyNeed):
                                if (needAdvertised.NeedValueChange[0] == 0) continue; // if this happens then the interaction is configured wrong

                                // If Duration is set, check if we'll reach MaxValue by the end of the Action's duration
                                if (needAdvertised.InteractDuration != 0f)
                                {
                                    endResult = currentNeed.Value.Value + (needAdvertised.NeedValueChange * needAdvertised.InteractDuration);
                                    if (endResult[0] < needData.MaxValue[0])
                                    {
                                        // If we won't, then just use the end result
                                        advertisedValue = endResult[0] / needAdvertised.InteractDuration;
                                        break;
                                    }
                                }

                                // Otherwise, calculate how long it will take to reach max value
                                // (max - current) / valueChangePerSecond
                                float3 amountChanged = needData.MaxValue - currentNeed.Value.Value;
                                float secondsToMax = amountChanged[0] / needAdvertised.NeedValueChange[0];
                                advertisedValue = (secondsToMax == 0)
                                    ? needData.MaxValue[0]
                                    : needData.MaxValue[0] / secondsToMax;
                                break;


                                //  TODO: WRITE SWITCH FOR CALCULATING WEIGHT OF EMOTION-DEPENDENT NEED
                                //  (WILL BE BASED ON CLOSENESS OF CURRENT NEED VALUE TO NEED ADVERTISED)
                                //  NeedRange - Abs(Distance between Mood values) * Intensity
                        }

                        // Get the advertised Need's scaling Curve, and evaluate position of the NPC's current Need value on the curve
                        // (needMax - currentNeed) / needMax
                        float currentNeedValue = math.clamp((needData.MaxValue - currentNeed.Value.Value) / needData.MaxValue, 0f, 1f)[0];
                        int curveIndex = (int)math.round(currentNeedValue * 99f);
                        float curveValue = needData.Curve[curveIndex];

                        weightedValue += advertisedValue * curveValue;

                        /* Can't format strings unless we turn off BurstCompile :)
                         * 
                        Debug.Log(string.Format("need = {0}, weighted value = {1}, advertisedValue = {2} currentNeedValue = {3}, curveIndex = {4}, curveValue = {5}",
                            needAdvertised.NeedAdvertised.Type.ToString(), weightedValue, advertisedValue, currentNeedValue, curveIndex, curveValue));
                        */
                    }

                    // Add distance from NPC
                    float3 targetPos = objTransform.ValueRO.Position;
                    float distance = math.max(math.distance(npcPos, targetPos), 1.0f); //don't allow division by 0
                    //Scale distance on curve
                    ref DistanceScalingData distanceScalingData = ref blobAsset.Value.DistanceScalingData;
                    float min = distanceScalingData.MinDistance;
                    float max = distanceScalingData.MaxDistance;

                    distance = math.clamp(distance, min, max);
                    int distanceCurveIndex = (int)(((distance - min) / max) * 99.0f);
                    float distanceScaled = distanceScalingData.DistanceCurve[distanceCurveIndex];

                    weightedValue *= distanceScaled;
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
            }

            //Debug.Log(string.Format("checked {0} possibilites", weightCount));

            // Randomly pick from weighted list
            int randomIndex = PickWeightedValue(ref randomSingleton.ValueRW, ref weights, weightCount, sumOfWeights);

            ActionPathfind pathfind = new() {
                Destination = weights[randomIndex].Position,
            };
            ecb.AddComponent(npcEntity, pathfind);  // note: AddComponent overwrites existing component if there is one

            QueuedAction action = new() {
                InteractionObject = weights[randomIndex].InteractableObjectEntity,
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
                    NeedValueChange = needAdvertised.NeedValueChange,
                    InteractDuration = needAdvertised.InteractDuration,
                    MinInteractDuration = needAdvertised.MinInteractDuration,
                    RequiredToCompleteAction = needAdvertised.RequiredToCompleteAction,
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
    public float Weight;
    public Entity InteractableObjectEntity;
}