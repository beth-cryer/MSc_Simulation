using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject[] NPCPrefabs;
    public int SpawnAmount = 10;

    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            Spawner spawner = new()
            {
                SpawnAmount = authoring.SpawnAmount
            };

			DynamicBuffer<NPCPrefab> npcPrefabs = AddBuffer<NPCPrefab>(entity);
			foreach(var npcPrefab in authoring.NPCPrefabs)
			{
				npcPrefabs.Add(new()
				{
					Prefab = GetEntity(npcPrefab, TransformUsageFlags.Dynamic)
				});
			}

            AddComponent(entity, spawner);
        }
    }
}

struct Spawner : IComponentData
{
    public int SpawnAmount;
}

struct NPCPrefab: IBufferElementData
{
	public Entity Prefab;
}