using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NPCSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
		state.RequireForUpdate<BlobSingleton>();
		state.RequireForUpdate<Spawner>();
		state.RequireForUpdate<RandomSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; //this means spawn will only run once on startup

		BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
		var randomSingleton = SystemAPI.GetSingletonRW<RandomSingleton>();

		EntityCommandBuffer ecb = new(Allocator.TempJob);

		var spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
		var npcSpawner = SystemAPI.GetSingleton<Spawner>();
		var prefabBuffer = SystemAPI.GetBuffer<NPCPrefab>(spawnerEntity);

		NativeArray<Entity> npcInstances = new NativeArray<Entity>(npcSpawner.SpawnAmount, Allocator.Temp);
		int normalGuys = 0;
		for(int i = 0; i < npcSpawner.SpawnAmount; i ++)
		{
			int randomSprite = randomSingleton.ValueRW.Random.NextInt(prefabBuffer.Length - 1);
			Entity entity = state.EntityManager.Instantiate(prefabBuffer[randomSprite].Prefab);

			var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
			transform.ValueRW.Position = randomSingleton.ValueRW.Random.NextFloat3(new float3(-5, -5, 0), new float3(5, 5, 0));

			int randomTrait = randomSingleton.ValueRW.Random.NextInt(blobAsset.Value.TraitsData.Length - 1);
			var randomTraitData = blobAsset.Value.TraitsData[randomTrait].Trait;

			var traitBuffer = ecb.SetBuffer<TraitBuffer>(entity);
			if (normalGuys > (int)(npcSpawner.SpawnAmount * 0.25)) // make 25% of NPCs just normal guys, so we have a baseline
			{
				traitBuffer.Add(new()
				{
					Trait = randomTraitData
				});
			}

			normalGuys++;
		}

		npcInstances.Dispose();

		/*
        NativeArray<Entity> instances = state.EntityManager.Instantiate(npcSpawner.NPCPrefab, npcSpawner.SpawnAmount, Allocator.Temp);
        foreach (Entity entity in instances)
        {
            var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            transform.ValueRW.Position = randomSingleton.ValueRW.Random.NextFloat3(new float3(10, 10, 0)) - new float3(5, 5, 0);

			int randomTrait = (int)(randomSingleton.ValueRW.Random.NextFloat() * (blobAsset.Value.TraitsData.Length - 1));
			var randomTraitData = blobAsset.Value.TraitsData[randomTrait].Trait;

			var traitBuffer = ecb.SetBuffer<TraitBuffer>(entity);
			traitBuffer.Add(new()
			{
				Trait = randomTraitData
			});
        }
        instances.Dispose();
		*/

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}