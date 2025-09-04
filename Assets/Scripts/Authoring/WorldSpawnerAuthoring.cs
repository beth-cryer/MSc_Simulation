using Unity.Entities;
using UnityEngine;

public class WorldSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject[] ObjectPrefabs;
	[SerializeField] private int WorldScale;	//size of the grid to spawn world segments on

    class ExampleWorldBaker : Baker<WorldSpawnerAuthoring>
    {
        public override void Bake(WorldSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
			WorldSpawner spawner = new()
			{
				WorldScale = authoring.WorldScale,
			};

			DynamicBuffer<WorldPrefab> worldPrefabs = AddBuffer<WorldPrefab>(entity);
			foreach (var objectPrefab in authoring.ObjectPrefabs)
			{
				worldPrefabs.Add(new()
				{
					ObjectPrefab = GetEntity(objectPrefab, TransformUsageFlags.Dynamic)
				});
			}

			AddComponent(entity, spawner);
        }
    }
}


struct WorldSpawner : IComponentData
{
	public int WorldScale;
}

struct WorldPrefab: IBufferElementData
{
	public Entity ObjectPrefab;
}