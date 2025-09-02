using Unity.Entities;
using UnityEngine;

public class NPCAgentAuthoring: MonoBehaviour
{
    public ActionAdvertised[] ActionsAdvertised;
    public Need[] NeedsAdvertised;

    class NPCAgentBaker : Baker<NPCAgentAuthoring>
    {
        public override void Bake(NPCAgentAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            string npcName = string.Format("NPC #{0}", entity.Index);

            // Add the NPC component to the Entity
            NPC npc = new()
            {
                Name = npcName,
                Speed = 5.0f
            };

            DynamicBuffer<NeedBuffer> needs = AddBuffer<NeedBuffer>(entity);
            foreach (var need in authoring.NeedsAdvertised)
            {
                needs.Add(new()
                {
                    Need =
                    {
                        Type = need.Type,
                        Value = need.Value,
                    }
                });
            }

            AddComponent(entity, npc);

			DynamicBuffer<TraitBuffer> traits = AddBuffer<TraitBuffer>(entity);

			// Add Social Actions
			DynamicBuffer<ActionAdvertisementBuffer> actionsAdvertised = AddBuffer<ActionAdvertisementBuffer>(entity);
            DynamicBuffer<NeedAdvertisementBuffer> needsAdvertised = AddBuffer<NeedAdvertisementBuffer>(entity);
            int i = 0;
            foreach (var actionAdvertised in authoring.ActionsAdvertised)
            {
                actionsAdvertised.Add(new()
                {
                    NeedAdvertisedCount = actionAdvertised.NeedAdvertised.Length,
                    NeedAdvertisedIndex = i,
                });

                // Add all of the needs advertised by the object to its Entity
                foreach (NeedAdvertisedData need in actionAdvertised.NeedAdvertised)
                {
                    needsAdvertised.Add(new()
                    {
						Details = new()
						{
							Need = need.NeedAdvertised,
							ActionType = need.ActionType,
							NeedValueChange = need.NeedValueChange,
							InteractDuration = need.InteractDuration,
							MinInteractDuration = need.MinInteractDuration,
							RequiredToCompleteAction = need.RequiredToCompleteAction,
						}
                    });
                    i++;
                }
            }

			InteractableObject interactable = new()
			{
				Name = npcName,
				InteractDistance = 0.7f
			};
            AddComponent(entity, interactable);
        }
    }
}