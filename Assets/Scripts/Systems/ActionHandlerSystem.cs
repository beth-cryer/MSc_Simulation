using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.GraphicsBuffer;

// Look for NPC Entities with an Action component and execute those actions

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct ActionHandlerSystem : ISystem
{
    [BurstCompile]
    public void OnStart(ref SystemState state)
    {
        state.RequireForUpdate<BlobSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        foreach (var (npc, needs, actionLabel, action, interaction, entity) in
            SystemAPI.Query<RefRO<NPC>, DynamicBuffer<NeedBuffer>, RefRO<ActionSetNeed>, DynamicBuffer<InteractionBuffer>, RefRW<Interaction>>()
            .WithNone<ActionPathfind>()
            .WithEntityAccess())
        {
            /* This is silly, I think we can used ComponentGroups to accomplish what i was thinking with this.
             * TODO; look into that
             * 
            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IActionComponent).IsAssignableFrom(p)); */

            // Create NeedBuffer so that we can keep replacing Need values as we go through this iteration,
            // then Playback the ECB at the end
            DynamicBuffer<NeedBuffer> newNeeds = ecb.SetBuffer<NeedBuffer>(entity);
            for (int i = 0; i < needs.Length; i++)
            {
                newNeeds.Add(new() { Need = needs[i].Need });
            }

            bool isTargetNeedsValuesReached = true; // if there are any needs that aren't at target, we'll set this to false

            // Move need towards target value gradually
            foreach (InteractionBuffer actionBuffer in action)
            {
                if (actionBuffer.ActionType == EActionType.MoveTowards)
                {
                    for (int i = 0; i < newNeeds.Length; i++)
                    {
                        if (newNeeds[i].Need.Type == actionBuffer.NeedAction.Type)
                        {
                            Need alteredNeed = newNeeds[i].Need;
                            float3 current = newNeeds[i].Need.Value;
                            float3 target = actionBuffer.NeedAction.Value;

                            if (math.distance(current,target) > 0.01f)
                                isTargetNeedsValuesReached = false;

                            // If MoveTowardsAmount is set, use that value as the stepAmount-
                            if (!actionBuffer.MoveTowardsAmount.Equals(float3.zero))
                            {
                                float stepAmount = actionBuffer.MoveTowardsAmount[0] * SystemAPI.Time.DeltaTime;
                                MathHelpers.MoveTowards(ref current, ref target, stepAmount, out alteredNeed.Value);
                            }
                            else
                            // Else: calculate amount to step to get from startValue to endValue within remaining interaction time-
                            {
                                // stepAmount = ((target value - current value) / timeRemaining) * deltaTime
                                float timeRemaining = interaction.ValueRO.InteractDuration - interaction.ValueRO.TimeElapsed;
                                float3 stepAmount = ((actionBuffer.NeedAction.Value - newNeeds[i].Need.Value) / timeRemaining) * SystemAPI.Time.DeltaTime;
                                MathHelpers.MoveTowards(ref current, ref target, math.abs(stepAmount[0]), out alteredNeed.Value);
                                //alteredNeed.Value = actionBuffer.NeedAction.Value + stepAmount;
                            }
                            newNeeds[i] = new() { Need = alteredNeed };
                        }
                    }
                }
                else
                {
                    // If there are any EActionType.SetNeed actions, then we don't care if the MoveTowards actions are all done-
                    // we still have to wait for Duration so that SetNeed actions can complete fully
                    isTargetNeedsValuesReached = false;
                }
            }

            interaction.ValueRW.TimeElapsed += SystemAPI.Time.DeltaTime;

            // Action is done if we have reached the target value for all Needs,
            // or the time elapsed is greater than the action duration
            bool isActionFinished = isTargetNeedsValuesReached
                || (interaction.ValueRO.InteractDuration != 0 && interaction.ValueRO.TimeElapsed >= interaction.ValueRO.InteractDuration);

            // If action is done,
            if (isActionFinished)
            {
                // Set need to target value
                foreach (InteractionBuffer actionBuffer in action)
                {
                    if (actionBuffer.ActionType == EActionType.SetNeed)
                    {
                        for (int i = 0; i < newNeeds.Length; i++)
                        {
                            Need alteredNeed = newNeeds[i].Need;
                            if (alteredNeed.Type == actionBuffer.NeedAction.Type)
                                alteredNeed.Value = actionBuffer.NeedAction.Value;
                            newNeeds[i] = new() { Need = alteredNeed };
                        }
                    }
                }

                // Remove all components related to the action from both Entities involved
                ecb.RemoveComponent<ActionSetNeed>(entity);
                ecb.RemoveComponent<Interaction>(entity);
                ecb.RemoveComponent<InUseTag>(interaction.ValueRO.InteractionObject);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}