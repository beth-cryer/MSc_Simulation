using NaughtyAttributes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class InteractableObjectAuthoring : MonoBehaviour
{
    [SerializeField] [Expandable] private ObjectData ObjectData;

    class InteractableObjectBaker : Baker<InteractableObjectAuthoring>
    {
        public override void Bake(InteractableObjectAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Add Interactable component to object Entity
            InteractableObject obj = new()
            {
                Name = new FixedString32Bytes(authoring.ObjectData.Name),
            };
            AddComponent(entity, obj);

            DynamicBuffer<ActionAdvertisementBuffer> actionsAdvertised = AddBuffer<ActionAdvertisementBuffer>(entity);
            DynamicBuffer<NeedAdvertisementBuffer> needsAdvertised = AddBuffer<NeedAdvertisementBuffer>(entity);

            int i = 0;
            foreach (var actionAdvertised in authoring.ObjectData.ActionsAdvertised)
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
                        NeedAdvertised = need.NeedAdvertised,
                        ActionType = need.ActionType,
                        NeedValueChange = need.NeedValueChange,
                        InteractDuration = need.InteractDuration,
                        MinInteractDuration = need.MinInteractDuration,
                        RequiredToCompleteAction = need.RequiredToCompleteAction,
                    });
                    i++;
                }
            }

            // Set object sprite
            if (authoring.ObjectData.Sprite == null)
                return;
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = authoring.ObjectData.Sprite;
        }
    }
}

public struct ActionAdvertisementBuffer: IBufferElementData
{
    public int NeedAdvertisedIndex;
    public int NeedAdvertisedCount;
}