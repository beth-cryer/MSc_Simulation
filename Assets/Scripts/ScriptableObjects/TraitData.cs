using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Trait", menuName = "Data/Trait")]
public class TraitData : ScriptableObject
{
	public string Name;
	public ETrait Type;
	public List<TraitModifierData> NeedModifiers;
	public List<TraitModifierData> AddNeed;
	public MoodModifierData ModifyMood;
}

[Serializable]
public class MoodModifierData
{
	public EEmotion MoodModifyType;
	public float MoodModifyAmount;
}

[Serializable]
public class TraitModifierData
{
	public ENeed Need;
	public float3 Modifier;
}