using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject NPCPrefab;
    public int SpawnAmount = 10;

    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var spawner = new Spawner
            {
                NPCPrefab = GetEntity(authoring.NPCPrefab, TransformUsageFlags.Dynamic),
                SpawnAmount = authoring.SpawnAmount
            };
            AddComponent(entity, spawner);
        }
    }
}

struct Spawner : IComponentData
{
    public Entity NPCPrefab;
    public int SpawnAmount;
}