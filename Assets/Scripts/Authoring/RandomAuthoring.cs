using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class RandomAuthoring : MonoBehaviour
{
    [SerializeField] private uint randomSeed = 100;

    private class Baker : Baker<RandomAuthoring>
    {
        public override void Bake(RandomAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            RandomSingleton random = new()
            {
                Random = Random.CreateFromIndex(authoring.randomSeed)
            };

            AddComponent(entity, random);
        }
    }
}