using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ActionPlannerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (npc, needs) in SystemAPI.Query<RefRO<NPC>, DynamicBuffer<NeedsBuffer>>())
        {
            foreach (var (obj, needsAdvertised) in SystemAPI.Query<RefRO<InteractableObject>, DynamicBuffer<NeedAdvertisementsBuffer>>())
            {
                //Get distance from NPC

                //for each need advertised, do (100-Need) * Need Advertised * (1/Distance)

                
            }
        }
    }
}