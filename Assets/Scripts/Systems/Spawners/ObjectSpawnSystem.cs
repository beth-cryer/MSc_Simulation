using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ObjectSpawnSystem : ISystem
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
        state.Enabled = false; //this means spawn will only run once on startup

		BlobAssetReference<ObjectsBlobAsset> blobAsset = SystemAPI.GetSingleton<BlobSingleton>().BlobAssetReference;
		var randomSingleton = SystemAPI.GetSingletonRW<RandomSingleton>();

		EntityCommandBuffer ecb = new(Allocator.TempJob);

		var spawnerEntity = SystemAPI.GetSingletonEntity<WorldSpawner>();
		var worldSpawner = SystemAPI.GetSingleton<WorldSpawner>();
		var prefabBuffer = SystemAPI.GetBuffer<WorldPrefab>(spawnerEntity);

		NativeArray<Entity> worldInstances = state.EntityManager.Instantiate(prefabBuffer[0].ObjectPrefab, worldSpawner.WorldScale*worldSpawner.WorldScale, Allocator.Temp);

		float gridSize = 4.0f;
		float worldSize = worldSpawner.WorldScale * gridSize;
		float3 startPos = new(-worldSize * 0.5f, -worldSize * 0.5f, 0.0f);
		float3 endPos = new(worldSize * 0.5f, worldSize * 0.5f, 0.0f);

		int i = 0;
		for (int x = 0; x < worldSpawner.WorldScale; x++)
		{
			for (int y = 0; y < worldSpawner.WorldScale; y++)
			{
				Entity entity = worldInstances[i];

				var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
				transform.ValueRW.Position = startPos + new float3(x * gridSize, y * gridSize, 0.0f);

				i++;
			}
		}

		worldInstances.Dispose();
	}
}