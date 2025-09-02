using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlobAuthoring: MonoBehaviour
{
    [SerializeField] private AnimationCurve DistanceCurve;

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

            // Distance curve (determines how much distance affects an action's utility weight)
            if (authoring.DistanceCurve != null)
            {
                blobAsset.DistanceScalingData = new()
                {
                    MinDistance = authoring.DistanceCurve.keys[0].time,
                    MaxDistance = authoring.DistanceCurve.keys[authoring.DistanceCurve.keys.Length - 1].time,
                };
                BlobBuilderArray<float> distanceCurveArray = blobBuilder.Allocate(ref blobAsset.DistanceScalingData.DistanceCurve, 100);
                AnimationCurveToArray(authoring.DistanceCurve, ref distanceCurveArray, 100, blobAsset.DistanceScalingData.MinDistance, blobAsset.DistanceScalingData.MaxDistance);
            }

            // Load Needs data from ScriptableObjects in the Resources folder
            // https://discussions.unity.com/t/populating-an-array-with-scriptable-objects-directly-through-script/849860/3
            NeedData[] NeedData = Resources.LoadAll<NeedData>("Data/Needs/");
            BlobBuilderArray<NeedsData> needsArray = blobBuilder.Allocate(ref blobAsset.NeedsData, NeedData.Length);

            // Populate Blob Data with Needs and generate arrays from curves
            for (int i = 0; i < NeedData.Length; i++)
            {
                int index = (int)NeedData[i].Type;
                needsArray[index] = new()
                {
                    Type = NeedData[i].Type,
                    MinValue = NeedData[i].MinValue,
                    ZeroValue = NeedData[i].ZeroValue,
                    MaxValue = NeedData[i].MaxValue,
                    DecayRate = NeedData[i].DecayRate,
                };

                int sampleSize = 100;
                BlobBuilderArray<float> curve = blobBuilder.Allocate(ref needsArray[index].Curve, sampleSize);
                AnimationCurveToArray(NeedData[i].Curve, ref curve, sampleSize);
            }

			// Load Traits Data
			TraitData[] TraitData = Resources.LoadAll<TraitData>("Data/Traits/");
			BlobBuilderArray<TraitsData> traitsArray = blobBuilder.Allocate(ref blobAsset.TraitsData, TraitData.Length);
			for (int i = 0; i < TraitData.Length; i++)
			{
				int index = (int)TraitData[i].Type;
				traitsArray[index] = new()
				{
					Trait = new()
					{
						NeedModifier = new()
						{
							Type = TraitData[i].NeedModifiers[0].Need,
							Value = TraitData[i].NeedModifiers[0].Modifier,
						},
						Type = TraitData[i].Type,
					}
				};
			}

			// 

			// Add Blob Data to Singleton so it can be accessed by systems
			var blobReference = blobBuilder.CreateBlobAssetReference<ObjectsBlobAsset>(Allocator.Persistent);
            blobBuilder.Dispose();

            AddBlobAsset(ref blobReference, out var hash);
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            BlobSingleton blobSingleton = new() { BlobAssetReference = blobReference };
            AddComponent(entity, blobSingleton);
        }

        private void AnimationCurveToArray(AnimationCurve curve, ref BlobBuilderArray<float> array, float sampleSize, float min = 0.0f, float max = 1.0f)
        {
            float sampleRate = (max - min) / sampleSize;
            for (int i = 0; i < sampleSize; i++)
            {
                array[i] = Mathf.Clamp01(curve.Evaluate(min + (i * sampleRate)));
            }
        }
    }
}