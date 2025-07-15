using Unity.Entities;
using UnityEngine;

public class InteractableObjectAuthoring : MonoBehaviour
{
    [SerializeField] private ObjectData ObjectData;

    class InteractableObjectBaker : Baker<InteractableObjectAuthoring>
    {
        public override void Bake(InteractableObjectAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Add interactable component to object Entity
            var obj = new InteractableObject();
            AddComponent(entity, obj);

            // Add all of the needs advertised by the object to its Entity
            var needsAdvertised = AddBuffer<NeedAdvertisementsBuffer>(entity);
            foreach (var need in authoring.ObjectData.NeedsAdvertised)
            {
                needsAdvertised.Add(new() { Need = need });
            }
        }
    }
}