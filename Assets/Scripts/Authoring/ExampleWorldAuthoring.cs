using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/*
public class ExampleWorldAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject ObjectPrefab;
    [SerializeField] private ObjectData[] ObjectData;

    class ExampleWorldBaker : Baker<ExampleWorldAuthoring>
    {
        public override void Bake(ExampleWorldAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            //Create NativeArray of object data
            DynamicBuffer<ObjectDataBuffer> buffer = new DynamicBuffer<ObjectDataBuffer>();
            var data = authoring.ObjectData.Select(x => x.Data);
            foreach (var obj in data)
                buffer.Append(new ObjectDataBuffer { Data = obj} );

            //Create spawner entity containing prefab and object data for spawner system to read from
            var spawner = new WorldSpawner
            {
                ObjectPrefab = GetEntity(authoring.ObjectPrefab, TransformUsageFlags.Dynamic),
                DataBuffer = buffer
            };

            AddComponent(entity, spawner);
        }
    }
}
*/

struct ObjectDataBuffer : IBufferElementData
{
    public InteractableObjectData Data;
}


struct WorldSpawner : IComponentData
{
    public Entity ObjectPrefab;
}