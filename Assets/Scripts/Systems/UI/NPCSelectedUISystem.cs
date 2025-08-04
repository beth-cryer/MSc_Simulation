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
            string interactableName = "?";

            // Find object we are interacting with
            if (SystemAPI.HasComponent<QueuedAction>(entity))
            {
                var action = SystemAPI.GetComponent<QueuedAction>(entity);
                interactableName = SystemAPI.GetComponent<InteractableObject>(action.InteractionObject).Name.ToString();

                // Either moving to the object or performing the action
                if (SystemAPI.HasComponent<ActionPathfind>(entity))
                    goal = string.Format("Moving to {0}", interactableName);
                else
                    goal = string.Format("Interacting with {0}", interactableName);
            }

            SelectedEntityUI.Instance.UpdateUI(npc.ValueRO, needsList, goal);
        }
    }
}