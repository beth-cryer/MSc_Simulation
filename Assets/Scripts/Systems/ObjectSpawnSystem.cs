using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ObjectSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldSpawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; //this means spawn will only run once on startup

        foreach (var (spawner, data) in SystemAPI.Query<RefRO<WorldSpawner>, DynamicBuffer<NeedsBuffer>>())
        {
            var instances = state.EntityManager.Instantiate(spawner.ValueRO.ObjectPrefab, data.Length, Allocator.Temp);

            for (int i = 0; i < instances.Length; i++)
            {
                var obj = SystemAPI.GetComponentRW<InteractableObject>(instances[i]);
                //obj.ValueRW.Data = data[i].Need;
            }
        }


        
    }
}