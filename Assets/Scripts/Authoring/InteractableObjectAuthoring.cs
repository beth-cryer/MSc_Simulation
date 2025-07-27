using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class InteractableObjectAuthoring : MonoBehaviour
{
    [SerializeField] private ObjectData ObjectData;

    class InteractableObjectBaker : Baker<InteractableObjectAuthoring>
    {
        public override void Bake(InteractableObjectAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Add Interactable component to object Entity
            InteractableObject obj = new()
            {
                Name = new FixedString32Bytes(authoring.ObjectData.name)
            };
            AddComponent(entity, obj);

            // Add all of the needs advertised by the object to its Entity
            DynamicBuffer<NeedAdvertisementsBuffer> needsAdvertised = AddBuffer<NeedAdvertisementsBuffer>(entity);
            foreach (Need need in authoring.ObjectData.NeedsAdvertised)
            {
                needsAdvertised.Add(new() { Need = need });
            }

            // Set object sprite
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = authoring.ObjectData.Sprite;
        }
    }
}