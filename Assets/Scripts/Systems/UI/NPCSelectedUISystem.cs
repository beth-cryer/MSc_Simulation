using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct NPCSelectedUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
		bool isSelected = false;

        foreach (var (npc, needs, traits, entity)
            in SystemAPI.Query<RefRO<NPC>, DynamicBuffer<NeedBuffer>, DynamicBuffer<TraitBuffer>>()
			.WithAll<SelectedEntityTag>()
            .WithEntityAccess())
        {
            List<Need> needsList = new();
            for (int i = 0; i < needs.Length; i++)
                needsList.Add(needs[i].Need);

			List<Trait> traitsList = new();
			for (int i = 0; i < traits.Length; i++)
				traitsList.Add(traits[i].Trait);

			string goal  = "None";
            string interactableName = "?";
			List<ENeed> changingNeeds = new() { };

			// Find object we are interacting with
			if (SystemAPI.HasComponent<Interaction>(entity))
			{
				Interaction action = SystemAPI.GetComponent<Interaction>(entity);
				string actionName = action.Name.ToString();

				if (!SystemAPI.HasComponent<NPC>(action.InteractionObject))
					interactableName = SystemAPI.GetComponent<InteractableObject>(action.InteractionObject).Name.ToString();
				else
					interactableName = "NPC #" + action.InteractionObject.Index;

				if (!SystemAPI.HasComponent<ActionPathfind>(entity))
				{
					goal = string.Format("{0} {1}", actionName, interactableName); //Print the action and interactable name

					// Highlight active need UI
					if (SystemAPI.HasBuffer<InteractionBuffer>(entity))
					{
						var interactionBuffer = SystemAPI.GetBuffer<InteractionBuffer>(entity);
						foreach (var interaction in interactionBuffer)
						{
							changingNeeds.Add(interaction.Details.Need.Type);
						}
					}
				}

			}else
			if (SystemAPI.HasComponent<QueuedAction>(entity))
			{
				QueuedAction action = SystemAPI.GetComponent<QueuedAction>(entity);
				bool isPathfinding = SystemAPI.HasComponent<ActionPathfind>(entity);

				if (!SystemAPI.HasComponent<NPC>(action.InteractionObject))
					interactableName = SystemAPI.GetComponent<InteractableObject>(action.InteractionObject).Name.ToString();
				else
					interactableName = "NPC #" + action.InteractionObject.Index;

				if (SystemAPI.HasComponent<SocialRequest>(entity))
				{
					goal = "Being interacted with";
				}else
				// Either moving to the object or performing the action
				if (isPathfinding)
				{
					var pathfind = SystemAPI.GetComponent<ActionPathfind>(entity);
					var objectSocialRequest = SystemAPI.HasComponent<SocialRequest>(action.InteractionObject);
					var objectInUse = SystemAPI.HasComponent<InUseTag>(action.InteractionObject);
					if (pathfind.DestinationReached)
					{
						if (objectSocialRequest || objectInUse) goal = string.Format("Waiting for {0} to be free", interactableName);
					}
					else
						goal = string.Format("Moving to {0}", interactableName);
				}
				else
				{
					goal = string.Format("Interacting with {0}", interactableName); // Shouldn't get here
					Debug.Log("Error; UI showing impossible thing");
				}
			}

			string npcName = "NPC #" + entity.Index.ToString();
            SelectedEntityUI.Instance.UpdateUI(npc.ValueRO, needsList, changingNeeds, traitsList, npcName, goal);

			isSelected = true;
        }

		if (!isSelected)
			SelectedEntityUI.Instance.HideUI();
	}
}