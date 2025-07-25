using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct SetupBlobAssetSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BlobSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        using BlobBuilder blobBuilder = new(Allocator.Temp);
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
            needsArray[i] = new() { Type = NeedData[i].Type };

            // We'll sample at a fidelity of 25, so every 4 steps in the curve
            int sampleSize = 25;
            BlobBuilderArray<float> curve = blobBuilder.Allocate(ref needsArray[i].Curve, sampleSize);
            AnimationCurveToArray(new AnimationCurve(), ref curve, sampleSize);
        }
        needsArray[0] = new()
        {
            Type = ENeed.Hunger
        };

        // Add Blob Data to Singleton so it can be accessed by systems
        BlobSingleton blobSingleton = SystemAPI.GetSingleton<BlobSingleton>();
        blobSingleton.BlobAssetReference = blobBuilder.CreateBlobAssetReference<ObjectsBlobAsset>(Allocator.Persistent);
        SystemAPI.SetSingleton(blobSingleton);
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