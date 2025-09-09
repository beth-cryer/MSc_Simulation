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
		state.RequireForUpdate<Spawner>();
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

		var spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
		var npcSpawner = SystemAPI.GetSingleton<Spawner>();
		var prefabBuffer = SystemAPI.GetBuffer<NPCPrefab>(spawnerEntity);

		var worldSpawner = SystemAPI.GetSingleton<WorldSpawner>();

		int spawnAmount = npcSpawner.SpawnAmount;
		float spawnScale = worldSpawner.WorldScale * 4.0f * 0.5f;
		NativeArray<Entity> npcInstances = new NativeArray<Entity>(spawnAmount, Allocator.Temp);
		int amountSpawned = 0;
		//int actionsSpawned = 0;
		for(int i = 0; i < spawnAmount; i ++)
		{
			int randomSprite = randomSingleton.ValueRW.Random.NextInt(prefabBuffer.Length);
			Entity entity = state.EntityManager.Instantiate(prefabBuffer[randomSprite].Prefab);

			var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
			transform.ValueRW.Position = randomSingleton.ValueRW.Random.NextFloat3(new float3(-spawnScale, -spawnScale, 0), new float3(spawnScale, spawnScale, 0));

			int randomTrait = randomSingleton.ValueRW.Random.NextInt(blobAsset.Value.TraitsData.Length - 1);
			var randomTraitData = blobAsset.Value.TraitsData[randomTrait].Trait;

			var traitBuffer = ecb.SetBuffer<TraitBuffer>(entity);
			if (amountSpawned > (int)(spawnAmount * 0.3)) // make 30% of NPCs just normal guys, so we have a baseline to compare to
			{
				traitBuffer.Add(new()
				{
					Trait = randomTraitData
				});

				// Add Need to NPC
				if (randomTraitData.AddNeed)
				{
					NeedBuffer traitNeed = new()
					{
						Need = new ()
						{
							Type = randomTraitData.AddNeedType.Type,
							Value = randomTraitData.AddNeedType.Value,
						}
					};
					ecb.AppendToBuffer(entity, traitNeed);
				}

				// Modify NPC properties
				switch(randomTraitData.Type)
				{
					case (ETrait.Speedy):
						SystemAPI.GetComponentRW<NPC>(entity).ValueRW.Speed = 5.0f;
						break;

					case (ETrait.Slow):
						SystemAPI.GetComponentRW<NPC>(entity).ValueRW.Speed = 1.5f;
						break;
				}
			}

			//actionsSpawned += SystemAPI.GetComponentRO<InteractableObject>(entity).ValueRO.ActionsAdvertisedCount;

			//var actionsAdvertised = SystemAPI.GetBuffer<ActionAdvertisementBuffer>(entity);
			//actionsSpawned += actionsAdvertised.Length;
			amountSpawned++;
		}
		//worldSpawner.ValueRW.WorldInteractablesCount += actionsSpawned;

		npcInstances.Dispose();

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}