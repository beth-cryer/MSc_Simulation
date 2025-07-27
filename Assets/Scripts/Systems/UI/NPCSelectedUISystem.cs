using System.Collections.Generic;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NPCSelectedUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (npc, needs, selected, entity)
            in SystemAPI.Query<RefRO<NPC>, DynamicBuffer<NeedsBuffer>, RefRO<SelectedEntityTag>>()
            .WithEntityAccess())
        {
            List<Need> needsList = new();
            for (int i = 0; i < needs.Length; i++)
                needsList.Add(needs[i].Need);

            SelectedEntityUI.Instance.UpdateUI(npc.ValueRO, needsList);
        }
    }
}