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

            // Add all of the needs advertised by the object to its Entity
            DynamicBuffer<NeedAdvertisementBuffer> needsAdvertised = AddBuffer<NeedAdvertisementBuffer>(entity);
            foreach (NeedAdvertisedData need in authoring.ObjectData.NeedsAdvertised)
            {
                needsAdvertised.Add(new()
                {
                    NeedAdvertised = need.NeedAdvertised,
                    ActionType = need.ActionType,
                    NeedValueChange = need.NeedValueChange,
                    InteractDuration = need.InteractDuration,
                    RequiredToCompleteAction = need.RequiredToCompleteAction,
                });
            }

            // Set object sprite
            if (authoring.ObjectData.Sprite == null)
                return;
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = authoring.ObjectData.Sprite;
        }
    }
}