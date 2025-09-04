using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Emotion", menuName = "Data/Emotion")]
public class EmotionData : ScriptableObject
{
	public string Name;
	public EEmotion Type;
	public float3 PADValue;
}