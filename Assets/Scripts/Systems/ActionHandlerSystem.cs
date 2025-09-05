using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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

        foreach (var (needs, actions, interaction, entity) in
            SystemAPI.Query<DynamicBuffer<NeedBuffer>, DynamicBuffer<InteractionBuffer>, RefRW<Interaction>>()
			.WithAll<NPC, ShortTermMemoryBuffer>()
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

            // Action is finished if we have reached the target value or time has elapsed for all Needs that are RequiredToCompleteAction
            bool isActionFinished = true; // if there are any needs that aren't at target, we'll set this to false

            // Move need towards target value gradually
            foreach (InteractionBuffer actionBuffer in actions)
            {
				Action action = actionBuffer.Details;

                if (action.RequiredToCompleteAction && !actionBuffer.Complete)
                    isActionFinished = false;

                if (actionBuffer.Complete)
                    break;

                switch (action.ActionType)
                {
                    case (EActionType.ModifyNeed):
                        for (int n = 0; n < newNeeds.Length; n++)
                        {
                            if (newNeeds[n].Need.Type == action.Need.Type)
                            {
                                Need alteredNeed = newNeeds[n].Need;
                                float3 current = newNeeds[n].Need.Value;
                                float3 target = action.Need.Value;

                                ref var needData = ref blobAsset.Value.NeedsData[(int)alteredNeed.Type];

                                alteredNeed.Value = math.clamp(alteredNeed.Value + action.NeedValueChange * SystemAPI.Time.DeltaTime,
                                    needData.MinValue,
                                    needData.MaxValue);

                                if (interaction.ValueRO.TimeElapsed >= action.MinInteractDuration &&
                                    ((action.InteractDuration > 0 && interaction.ValueRO.TimeElapsed >= action.InteractDuration)
                                    || math.all(alteredNeed.Value == needData.MaxValue)
                                    || math.all(alteredNeed.Value == needData.MinValue)))
                                {
                                    // Set as complete in InteractionBuffer
                                    SetComplete(ref ecb, entity, actions, action);
                                }

                                newNeeds[n] = new() { Need = alteredNeed };
                            }
                        }
                        break;
                    case (EActionType.SetNeed):
                        if (interaction.ValueRO.TimeElapsed < action.InteractDuration)
                            break;

                        // Set need to target value
                        for (int i = 0; i < newNeeds.Length; i++)
                        {
                            Need alteredNeed = newNeeds[i].Need;
                            if (alteredNeed.Type == action.Need.Type)
                                alteredNeed.Value = action.Need.Value;
                            newNeeds[i] = new() { Need = alteredNeed };
                        }

                        // Set as complete in InteractionBuffer
                        SetComplete(ref ecb, entity, actions, action);
                        break;

                    /*
                    case (EActionType.MoveTowards):
                        for (int i = 0; i < newNeeds.Length; i++)
                        {
                            if (newNeeds[i].Need.Type == actionBuffer.NeedAction.Type)
                            {
                                Need alteredNeed = newNeeds[i].Need;
                                float3 current = newNeeds[i].Need.Value;
                                float3 target = actionBuffer.NeedAction.Value;

                                if (math.distance(current, target) > 0.01f)
                                    isTargetNeedsValuesReached = false;

                                // If MoveTowardsAmount is set, use that value as the stepAmount-
                                if (!actionBuffer.MoveTowardsAmount.Equals(float3.zero))
                                {
                                    float stepAmount = actionBuffer.MoveTowardsAmount[0] * SystemAPI.Time.DeltaTime;
                                    MathHelpers.MoveTowards(ref current, ref target, stepAmount, out alteredNeed.Value);
                                }
                                else
                                // Else: calculate amount to step to get from startValue to endValue within remaining interaction time-
                                // (TODO: Fix this cause it doesn't seem to work how I expected)
                                {
                                    // stepAmount = ((target value - current value) / timeRemaining) * deltaTime
                                    float timeRemaining = interaction.ValueRO.InteractDuration - interaction.ValueRO.TimeElapsed;
                                    float3 stepAmount = ((actionBuffer.NeedAction.Value - newNeeds[i].Need.Value) / timeRemaining) * SystemAPI.Time.DeltaTime;
                                    MathHelpers.MoveTowards(ref current, ref target, math.abs(stepAmount[0]), out alteredNeed.Value);
                                }
                                newNeeds[i] = new() { Need = alteredNeed };
                            }
                        }
                        break;
                        */
                }
            }

            interaction.ValueRW.TimeElapsed += SystemAPI.Time.DeltaTime;

            // If action is done,
            if (!isActionFinished)
                continue;

			// Add short term Memory
			EEmotion initiatorEmotion = interaction.ValueRO.InitiatorEmotion;
			if (initiatorEmotion != EEmotion.None)
			{
				ShortTermMemoryBuffer newMemoryInitiator = new()
				{
					// Get float3 value of emotion from blobAsset
					Memory = new()
					{
						Type = initiatorEmotion,
						EmotionResponse = blobAsset.Value.EmotionsData[(int)initiatorEmotion].PADValue
					}
				};
				ecb.AppendToBuffer(entity, newMemoryInitiator);
			}

			// If InteractableObject is also an NPC:
			//	Add a short term memory to the target NPC
			//	Remove any existing SocialRequest tag
			if (SystemAPI.HasComponent<NPC>(interaction.ValueRO.InteractionObject))
			{
				EEmotion targetEmotion = interaction.ValueRO.TargetEmotion;
				if (targetEmotion != EEmotion.None)
				{
					ShortTermMemoryBuffer newMemoryTarget = new()
					{
						Memory = new()
						{
							Type = targetEmotion,
							EmotionResponse = blobAsset.Value.EmotionsData[(int)targetEmotion].PADValue,
						}
					};
					ecb.AppendToBuffer(interaction.ValueRO.InteractionObject, newMemoryTarget);
				}
				if (SystemAPI.HasComponent<SocialRequest>(interaction.ValueRO.InteractionObject))
					ecb.RemoveComponent<SocialRequest>(interaction.ValueRO.InteractionObject);
			}

            // Remove all components related to the action from both Entities involved
            //interaction.ValueRW.TimeElapsed = 0.0f;
            ecb.RemoveComponent<Interaction>(entity);
            ecb.RemoveComponent<QueuedAction>(entity);
            ecb.RemoveComponent<InUseTag>(interaction.ValueRO.InteractionObject);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void SetComplete(ref EntityCommandBuffer ecb, Entity entity, DynamicBuffer<InteractionBuffer> actions, Action action)
    {
        DynamicBuffer<InteractionBuffer> newActionBufferSet = ecb.SetBuffer<InteractionBuffer>(entity);
        for (int i = 0; i < actions.Length; i++)
        {
            InteractionBuffer newAction = actions[i];
            if (actions[i].Details.Need.Type == action.Need.Type)
                newAction.Complete = true;

            newActionBufferSet.Add(newAction);
        }
    }
}