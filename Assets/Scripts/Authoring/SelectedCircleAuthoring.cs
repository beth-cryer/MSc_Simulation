using Unity.Entities;
using UnityEngine;

public class SelectedCircleAuthoring : MonoBehaviour
{
	class SelectedCircleBaker : Baker<SelectedCircleAuthoring>
	{
		public override void Bake(SelectedCircleAuthoring authoring)
		{
			Entity entity = GetEntity(authoring, TransformUsageFlags.None);
			AddComponent<SelectCircle>(entity);
		}
	}
}