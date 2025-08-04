using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlobAuthoring: MonoBehaviour
{
    private class Baker : Baker<BlobAuthoring>
    {
        public override void Bake(BlobAuthoring authoring)
        {
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
                needsArray[(int)NeedData[i].Type] = new()
                {
                    Type = NeedData[i].Type,
                    MinValue = NeedData[i].MinValue,
                    ZeroValue = NeedData[i].ZeroValue,
                    MaxValue = NeedData[i].MaxValue,
                    DecayRate = NeedData[i].DecayRate,
                };

                // We'll sample at a fidelity of 25, so every 4 steps in the curve
                int sampleSize = 25;
                BlobBuilderArray<float> curve = blobBuilder.Allocate(ref needsArray[i].Curve, sampleSize + 1);
                AnimationCurveToArray(NeedData[i].Curve, ref curve, sampleSize);
            }

            // Add Blob Data to Singleton so it can be accessed by systems

            var blobReference = blobBuilder.CreateBlobAssetReference<ObjectsBlobAsset>(Allocator.Persistent);
            blobBuilder.Dispose();

            AddBlobAsset(ref blobReference, out var hash);
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            BlobSingleton blobSingleton = new() { BlobAssetReference = blobReference };
            AddComponent(entity, blobSingleton);
        }

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
}