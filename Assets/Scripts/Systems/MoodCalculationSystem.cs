using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// Look for NPC Entities and calculate Mood based on Short Term Memories (plus smaller influence from long term)
[UpdateInGroup(typeof(ActionRefreshSystemGroup))]
public partial struct MoodCalculationSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
		EntityCommandBuffer ecb = new(Allocator.TempJob);

		foreach (var (needs, memories, longTermPeriods, entity) in
			SystemAPI.Query<DynamicBuffer<NeedBuffer>, DynamicBuffer<ShortTermMemoryBuffer>, DynamicBuffer<LongTermMemoryPeriod>>()
			.WithAll<NPC, ShortTermMemoryBuffer>()
			.WithEntityAccess())
		{
			double timeElapsedCurrent = SystemAPI.Time.ElapsedTime;

			float runningWeightedLength = 0f;
			float3 runningTotal = 0f;
			foreach (var memory in memories)
			{
				float timeElapsed = (float)math.clamp(timeElapsedCurrent - memory.Memory.TimeElapsed, 0f, 60f);
				runningWeightedLength += 1 - (timeElapsed / 60.0f);
				runningTotal += memory.Memory.EmotionResponse;
			}

			// Add long term memory periods (using mood snapshots saved at that time)
			int longTermAge = longTermPeriods.Length;
			foreach (var longTermPeriod in longTermPeriods)
			{
				float timeElapsed = (float)math.clamp(timeElapsedCurrent - longTermPeriod.MemoryTimeElapsed, 0f, 240f);
				runningWeightedLength += 1 - (timeElapsed / 240.0f);
				runningTotal += longTermPeriod.MemoryPeriodMood;

				longTermAge--;
			}

			float3 mood = (runningWeightedLength > 0f) ?
				runningTotal / runningWeightedLength
				: new (0.0f, 0.0f, 0.0f);

			// Update Mood need value
			DynamicBuffer<NeedBuffer> buffer = ecb.SetBuffer<NeedBuffer>(entity);
			foreach (NeedBuffer need in needs)
			{
				Need alteredNeed = need.Need;
				if (need.Need.Type == ENeed.Mood)
					alteredNeed.Value = mood;

				buffer.Add(new() { Need = alteredNeed });
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}