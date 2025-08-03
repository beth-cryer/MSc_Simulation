using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/*
public partial struct SetupBlobAssetSystem : ISystem
{
    BlobAssetReference<ObjectsBlobAsset> blobAssetReference;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        blobAssetReference.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        BlobBuilder blobBuilder = new(Allocator.Temp);
        ref ObjectsBlobAsset blobAsset = ref blobBuilder.ConstructRoot<ObjectsBlobAsset>();
        BlobBuilderArray<ObjectsBlobData> objectArray = blobBuilder.Allocate(ref blobAsset.Array, 3);

        objectArray[0] = new()
        {
            ExampleData = "Hey"
        };

        // Load Needs data from ScriptableObjects in the Resources folder
        // https://discussions.unity.com/t/populating-an-array-with-scriptable-objects-directly-through-script/849860/3
        NeedData[] NeedData = Resources.LoadAll<NeedData>("Data/Needs/");
        BlobBuilderArray<NeedsData> needsArray = blobBuilder.Allocate(ref blobAsset.NeedsData, NeedData.Length);

        // Populate Blob Data with Needs and generate arrays from curves
        for (int i = 0; i < NeedData.Length; i++)
        {
            needsArray[i] = new()
            {
                Type = NeedData[i].Type,
                MinValue = NeedData[i].MinValue,
                ZeroValue = NeedData[i].ZeroValue,
                MaxValue = NeedData[i].MaxValue,
                DecayRate = NeedData[i].DecayRate,
            };

            // We'll sample at a fidelity of 25, so every 4 steps in the curve
            int sampleSize = 25;
            BlobBuilderArray<float> curve = blobBuilder.Allocate(ref needsArray[i].Curve, sampleSize+1);
            AnimationCurveToArray(new AnimationCurve(), ref curve, sampleSize);
        }

        // Add Blob Data to Singleton so it can be accessed by systems
        state.EntityManager.CreateSingleton<BlobSingleton>();
        BlobSingleton blobSingleton = SystemAPI.GetSingleton<BlobSingleton>();
        blobAssetReference = blobBuilder.CreateBlobAssetReference<ObjectsBlobAsset>(Allocator.Persistent);
        blobSingleton.BlobAssetReference = blobAssetReference;
        SystemAPI.SetSingleton(blobSingleton);

        blobBuilder.Dispose();
    }

    // NOTE: SAMPLE SIZE MUST BE MULTIPLE OF 100
    // Also assumes that curve axes values will be between 0-1, so clamps them to ensure this
    [BurstCompile]
    private void AnimationCurveToArray(AnimationCurve curve, ref BlobBuilderArray<float> array, float sampleSize)
    {
        //float width = curve.keys.Last().time; //this should be 1
        float sampleRate = 1 / sampleSize;
        for (int i = 0; i < sampleSize; i++)
        {
            array[i] = Mathf.Clamp(curve.Evaluate(i * sampleRate), 0f, 1f);
        }
    }
}
*/