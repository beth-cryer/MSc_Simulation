using Unity.Entities;

public struct BlobSingleton: IComponentData
{
    public BlobAssetReference<ObjectsBlobAsset> BlobAssetReference;
}