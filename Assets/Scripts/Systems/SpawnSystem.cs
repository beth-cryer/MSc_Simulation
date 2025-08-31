using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; //this means spawn will only run once on startup

        var npcSpawner = SystemAPI.GetSingleton<Spawner>();
        NativeArray<Entity> instances = state.EntityManager.Instantiate(npcSpawner.NPCPrefab, npcSpawner.SpawnAmount, Allocator.Temp);

        Random random = new(123);
        foreach (Entity entity in instances)
        {
            var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            transform.ValueRW.Position = random.NextFloat3(new float3(10, 10, 0)) - new float3(5, 5, 0);
        }

        instances.Dispose();
    }
}