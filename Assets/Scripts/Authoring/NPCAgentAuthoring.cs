using Unity.Entities;
using UnityEngine;

public class NPCAgentAuthoring: MonoBehaviour
{
    public ActionAdvertised[] ActionsAdvertised;
    public Need[] NPCNeeds;

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
            foreach (var need in authoring.NPCNeeds)
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

			// Add empty dynamic buffers
			AddBuffer<TraitBuffer>(entity);
			AddBuffer<ShortTermMemoryBuffer>(entity);
			AddBuffer<LongTermMemoryBuffer>(entity);
			AddBuffer<LongTermMemoryPeriod>(entity);

			ShortTermMemory shortTermMemory = new()
			{
				TimeInterval = 60.0f,
				MemoryLimit = 10,
			};
			AddComponent(entity, shortTermMemory);

			// Add Social Actions
			DynamicBuffer<ActionAdvertisementBuffer> actionsAdvertised = AddBuffer<ActionAdvertisementBuffer>(entity);
            DynamicBuffer<NeedAdvertisementBuffer> needsAdvertised = AddBuffer<NeedAdvertisementBuffer>(entity);
            int i = 0;
            foreach (var actionAdvertised in authoring.ActionsAdvertised)
            {
                actionsAdvertised.Add(new()
                {
					Name = actionAdvertised.Name,
                    NeedAdvertisedCount = actionAdvertised.NeedAdvertised.Length,
                    NeedAdvertisedIndex = i,
					EmotionAdvertised = actionAdvertised.InitiatorEmotion,
					TargetEmotion = actionAdvertised.TargetEmotion,
					InitiatorReaction = actionAdvertised.InitiatorReaction,
					TargetReaction = actionAdvertised.TargetReaction,
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