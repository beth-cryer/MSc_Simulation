using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
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

		//EntityCommandBuffer ecb = new(Allocator.TempJob);

		var spawnerEntity = SystemAPI.GetSingletonEntity<WorldSpawner>();
		var worldSpawner = SystemAPI.GetSingleton<WorldSpawner>();
		//var worldSpawnerRW = SystemAPI.GetSingletonRW<WorldSpawner>();
		var prefabBuffer = SystemAPI.GetBuffer<WorldPrefab>(spawnerEntity);

		NativeArray<Entity> worldInstances = new NativeArray<Entity>(worldSpawner.WorldScale*worldSpawner.WorldScale, Allocator.Temp);

		float gridSize = 4.0f;
		float worldSize = worldSpawner.WorldScale * gridSize;
		float3 startPos = new(-worldSize * 0.5f, -worldSize * 0.5f, 0.0f);
		float3 endPos = new(worldSize * 0.5f, worldSize * 0.5f, 0.0f);

		int i = 0;
		//int actionsSpawned = 0;
		for (int x = 0; x < worldSpawner.WorldScale; x++)
		{
			for (int y = 0; y < worldSpawner.WorldScale; y++)
			{
				int randomWorldTile = randomSingleton.ValueRW.Random.NextInt(prefabBuffer.Length);
				Entity entity = state.EntityManager.Instantiate(prefabBuffer[randomWorldTile].ObjectPrefab);

				var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
				transform.ValueRW.Position = startPos + new float3(x * gridSize, y * gridSize, 0.0f);

				//actionsSpawned += SystemAPI.GetComponentRO<InteractableObject>(entity).ValueRO.ActionsAdvertisedCount;
				//var actionsAdvertised = SystemAPI.GetBuffer<ActionAdvertisementBuffer>(entity);
				//actionsSpawned += actionsAdvertised.Length;
				i++;
			}
		}
		//worldSpawnerRW.ValueRW.WorldInteractablesCount += actionsSpawned;

		worldInstances.Dispose();

		//ecb.Playback(state.EntityManager);
		//ecb.Dispose();
	}
}