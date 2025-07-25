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
            NPC npc = new();

            // Initialise all of the NPC's basic Needs
            DynamicBuffer<NeedsBuffer> needs = AddBuffer<NeedsBuffer>(entity);
            needs.Add(new() { Need = { Type = ENeed.Hunger, Value = 100.0f } });
            needs.Add(new() { Need = { Type = ENeed.Sleep, Value = 100.0f } });
            needs.Add(new() { Need = { Type = ENeed.Shelter, Value = 100.0f } });
            needs.Add(new() { Need = { Type = ENeed.Social, Value = 100.0f } });

            AddComponent(entity, npc);
        }
    }
}