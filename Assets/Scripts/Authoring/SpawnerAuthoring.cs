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
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            Spawner spawner = new()
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