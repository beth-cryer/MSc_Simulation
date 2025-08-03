using System.Collections.Generic;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NPCSelectedUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (npc, needs, selected, entity)
            in SystemAPI.Query<RefRO<NPC>, DynamicBuffer<NeedBuffer>, RefRO<SelectedEntityTag>>()
            .WithEntityAccess())
        {
            List<Need> needsList = new();
            for (int i = 0; i < needs.Length; i++)
                needsList.Add(needs[i].Need);

            string goal  = "None";
            if (SystemAPI.HasComponent<ActionSetNeed>(entity))
                goal = "Interacting with some object lol";

            if (SystemAPI.HasComponent<ActionPathfind>(entity))
                goal = "Pathfinding";

            SelectedEntityUI.Instance.UpdateUI(npc.ValueRO, needsList, goal);
        }
    }
}