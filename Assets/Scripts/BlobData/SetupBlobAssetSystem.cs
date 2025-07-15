using Unity.Collections;
using Unity.Entities;

public partial struct SetupBlobAssetSystem : ISystem
{
    private ObjectsBlobData Data;

    public void OnCreate(ref SystemState state)
    {
        var entity = SystemAPI.GetSingletonEntity<ObjectsBlobData>();
        Data = state.EntityManager.GetComponentData<ObjectsBlobData>(entity);

        using var blobBuilder = new BlobBuilder(Allocator.Temp);
        ref var blobAsset = ref blobBuilder.ConstructRoot<ObjectsBlobAsset>();
        var objectArray = blobBuilder.Allocate(ref blobAsset.Array, 3);

    }

    public void OnUpdate(ref SystemState state)
    {
    }
}