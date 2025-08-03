using Unity.Entities;
using UnityEngine;

public class NPCAgentAuthoring: MonoBehaviour
{
    public Need[] NeedsAdvertised;

    class NPCAgentBaker : Baker<NPCAgentAuthoring>
    {
        public override void Bake(NPCAgentAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Add the NPC component to the Entity
            NPC npc = new()
            {
                Name = "ExampleName",
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
        }
    }
}