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

                if (!SystemAPI.HasComponent<NPC>(action.InteractionObject))
                    interactableName = SystemAPI.GetComponent<InteractableObject>(action.InteractionObject).Name.ToString();
                else
                    interactableName = "NPC #" + action.InteractionObject.Index;

                // Either moving to the object or performing the action
                if (SystemAPI.HasComponent<ActionPathfind>(entity))
                {
                    var pathfind = SystemAPI.GetComponent<ActionPathfind>(entity);
                    if (pathfind.DestinationReached)
                        goal = string.Format("Waiting for {0} to be free", interactableName);
                    else
                        goal = string.Format("Moving to {0}", interactableName);
                }
                else
                    goal = string.Format("Interacting with {0}", interactableName);
            }
            else
            if (SystemAPI.HasComponent<Interaction>(entity))
            {
                var action = SystemAPI.GetComponent<Interaction>(entity);

                if (!SystemAPI.HasComponent<NPC>(action.InteractionObject))
                    interactableName = SystemAPI.GetComponent<InteractableObject>(action.InteractionObject).Name.ToString();
                else
                    interactableName = "NPC #" + action.InteractionObject.Index;

                if (!SystemAPI.HasComponent<ActionPathfind>(entity))
                {
                    goal = string.Format("Interacting with {0}", interactableName);
                }
            }

            string npcName = "NPC #" + entity.Index.ToString();
            SelectedEntityUI.Instance.UpdateUI(npc.ValueRO, needsList, npcName, goal);

        }

    }
}