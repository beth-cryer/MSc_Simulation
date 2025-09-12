using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Lot of nested iteration over all entities happens here, so we limit it to a less frequent tick rate
[BurstCompile]
[UpdateInGroup(typeof(ActionRefreshSystemGroup))]
public partial struct ActionPlannerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<WorldSpawner>();
		state.RequireForUpdate<BlobSingleton>();
		state.RequireForUpdate<RandomSingleton>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		Debug.Log("AI update tick");

		var worldSpawner = SystemAPI.GetSingletonRW<WorldSpawner>();
		if (worldSpawner.ValueRO.WaitToStartGame < 0.1f)
		{
			worldSpawner.ValueRW.WaitToStartGame += SystemAPI.Time.DeltaTime;
			return;
		}

		// Get Needs Curves from BlobAsset
		BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
		var randomSingleton = SystemAPI.GetSingletonRW<RandomSingleton>();

		EntityCommandBuffer ecb = new(Allocator.TempJob);

		// Iterate over every NPC and process their action planning
		foreach (var (npcTransform, needs, traits, npcEntity)
			in SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<NeedBuffer>, DynamicBuffer<TraitBuffer>>()
			.WithAll<NPC>()
			.WithNone<ActionPathfind, Interaction, SocialRequest>()
			.WithEntityAccess())
		{
			float3 npcPos = npcTransform.ValueRO.Position;

			// TODO: Maintain a count of all interactables in a singleton somewhere
			NativeArray<WeightedAction> weights = new(500, Allocator.TempJob);
			int weightCount = 0;
			float sumOfWeights = 0;

			// Loop through each Interactable, check their Advertised Needs and calculate their Utility weight
			foreach (var (obj, objTransform, objTransformWorld, actionsAdvertised, needsAdvertised, objEntity) in
			SystemAPI.Query<RefRO<InteractableObject>, RefRO<LocalTransform>, RefRO<LocalToWorld>, DynamicBuffer<ActionAdvertisementBuffer>, DynamicBuffer <NeedAdvertisementBuffer>>()
			.WithNone<InUseTag, ActionPathfind>()
			.WithEntityAccess())
			{
				// Can't interact with yourself
				if (objEntity.Equals(npcEntity))
					continue;

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
						Action needAdvertised = needsAdvertised[i].Details;

						// Get the current value from the NPC of the Need that this is advertising
						Nullable<Need> currentNeed = null;
						foreach (NeedBuffer findNeed in needs)
						{
							if (findNeed.Need.Type == needAdvertised.Need.Type)
							{
								currentNeed = findNeed.Need;
								break;
							}
						}

						ref var needData = ref blobAsset.Value.NeedsData[(int)needAdvertised.Need.Type];

						if (!currentNeed.HasValue) continue; //if needAdvertised doesn't exist on the NPC, skip it
						if (currentNeed.Value.Value[0] == needData.MaxValue[0]) continue; //if need is already at max value, skip it

						float3 currentNeedValue = currentNeed.Value.Value;
						float advertisedValue = 0.0f;

						// Mood is special case, here we check how close the current Mood value is to the target Mood value
						if (needAdvertised.Need.Type == ENeed.Mood)
						{
							float3 currentMood = currentNeed.Value.Value;
							float3 advertisedMood = blobAsset.Value.EmotionsData[(int)needAdvertised.Need.EmotionValue].PADValue;

							// Add trait modifiers (nudge currentMood value towards target,
							// for any Trait with same MoodModifyEmotion type as the NeedAdvertised)
							foreach (var trait in traits)
							{
								if (trait.Trait.MoodModifyEmotion != needAdvertised.Need.EmotionValue)
									continue;
								MathHelpers.MoveTowards(ref currentMood, ref advertisedMood, trait.Trait.MoodModifyAmount, out currentMood);
							}

							float moodDistance = math.abs(math.distance(currentMood, advertisedMood));

							if (needAdvertised.InteractDuration == 0)
								advertisedValue = moodDistance;
							else
								advertisedValue = moodDistance / needAdvertised.InteractDuration;

						}
						else
						{
							// Value should be the value per second spent doing the action
							// So get the projected end Need value of the Action, and divide by Duration
							float3 endResult = 0.0f;
							switch (needAdvertised.ActionType)
							{
								case (EActionType.SetNeed):
									if (needAdvertised.InteractDuration == 0) continue; // if this happens then the interaction is configured wrong
									endResult = needAdvertised.Need.Value;
									advertisedValue = endResult[0] / needAdvertised.InteractDuration;
									break;
								case (EActionType.ModifyNeed):
									if (needAdvertised.NeedValueChange[0] == 0) continue; // if this happens then the interaction is configured wrong

									// If Duration is set, check if we'll reach MaxValue by the end of the Action's duration
									if (needAdvertised.InteractDuration != 0f)
									{
										endResult = currentNeedValue + (needAdvertised.NeedValueChange * needAdvertised.InteractDuration);
										if (endResult[0] < needData.MaxValue[0])
										{
											// If we won't, then just use the end result
											advertisedValue = (endResult[0] - currentNeedValue[0]) / needAdvertised.InteractDuration;
											break;
										}
									}

									// Otherwise, calculate how long it will take to reach max value
									// (max - current) / valueChangePerSecond
									float3 amountChanged = needData.MaxValue - currentNeed.Value.Value;
									float secondsToMax = amountChanged[0] / needAdvertised.NeedValueChange[0];
									advertisedValue = (secondsToMax == 0)
										? amountChanged[0]
										: amountChanged[0] / secondsToMax;
									break;
							}

							// Add trait modifiers to current need value
							foreach (var trait in traits)
							{
								if (trait.Trait.NeedModifier.Type != needAdvertised.Need.Type)
									continue;
								currentNeedValue += trait.Trait.NeedModifier.Value;
							}
							currentNeedValue = math.clamp(currentNeedValue, needData.MinValue, needData.MaxValue);

							// Get the advertised Need's scaling Curve, and evaluate position of the NPC's current Need value on the curve
							// (needMax - currentNeed) / needMax
							float modifiedNeedValue = math.clamp((needData.MaxValue - currentNeedValue) / needData.MaxValue, 0f, 1f)[0];
							int curveIndex = (int)math.round(modifiedNeedValue * 99f);
							float curveValue = needData.Curve[curveIndex];

							weightedValue += advertisedValue * curveValue;

							//Debug.Log(string.Format("need = {0}, weighted value = {1}, advertisedValue = {2} currentNeedValue = {3}, curveIndex = {4}, curveValue = {5}",
							//	needAdvertised.Need.Type.ToString(), weightedValue, advertisedValue, currentNeedValue, curveIndex, curveValue));
						}

					}

					// Add distance from NPC
					// Get scaled value of the distance on our Distance Scaling Curve
					float3 targetPos = objTransformWorld.ValueRO.Position;
					float distance = math.max(math.distance(npcPos, targetPos), 1.0f); //don't allow division by 0
					
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
						Action = actionAdvertised,
						Buffer = needsAdvertised,
						Position = targetPos,
						InteractDistance = obj.ValueRO.InteractDistance,
						Weight = weightedValue
					};
					weightCount++;
				}
			}

			//Debug.Log(string.Format("checked {0} possibilites", weightCount));

			if (weightCount == 0)
			{
				weights.Dispose();
				continue;
			}

			// Randomly pick from weighted list
			int randomIndex = PickWeightedValue(ref randomSingleton.ValueRW, ref weights, weightCount, sumOfWeights);

			WeightedAction chosenAction = weights[randomIndex];

			ActionPathfind pathfind = new() {
				DestinationEntity = chosenAction.InteractableObjectEntity,
				Destination = chosenAction.Position,
				InteractDistance = chosenAction.InteractDistance,
				RedirectAttempts = 3,
				WaitForTargetToBeFree = 5.0f,
			};
			ecb.AddComponent(npcEntity, pathfind);  // note: AddComponent overwrites existing component if there is one

			QueuedAction action = new() {
				Name = chosenAction.Action.Name,
				InteractionObject = chosenAction.InteractableObjectEntity,
				InitiatorEmotion = chosenAction.Action.EmotionAdvertised,
				InitiatorReaction = chosenAction.Action.InitiatorReaction,
				TargetEmotion = chosenAction.Action.TargetEmotion,
				TargetReaction = chosenAction.Action.TargetReaction,
			};
			ecb.AddComponent(npcEntity, action);

			// Copy NeedAdvertisementBuffer onto the NPC now
			// so that ActionHandlerSystem can execute the action without having to do another lookup later
			DynamicBuffer<InteractionBuffer> actionBuffer = ecb.AddBuffer<InteractionBuffer>(npcEntity);
			for (int i = chosenAction.Action.NeedAdvertisedIndex; i < chosenAction.Action.NeedAdvertisedIndex + chosenAction.Action.NeedAdvertisedCount; i++)
			{
				actionBuffer.Add(new()
				{
					Details = chosenAction.Buffer[i].Details,
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
	public ActionAdvertisementBuffer Action;
	public float3 Position;
	public float InteractDistance;
	public float Weight;
	public Entity InteractableObjectEntity;
}