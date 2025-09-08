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

		foreach (var (needs, memories, entity) in
			SystemAPI.Query<DynamicBuffer<NeedBuffer>, DynamicBuffer<ShortTermMemoryBuffer>>()
			.WithAll<NPC, ShortTermMemoryBuffer>()
			.WithEntityAccess())
		{
			float3 runningTotal = 0f;
			foreach (var memory in memories)
			{
				runningTotal += memory.Memory.EmotionResponse;
			}
			float3 mood = memories.Length > 0 ?
				runningTotal / memories.Length
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