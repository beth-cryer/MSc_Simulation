// This is object oriented as HECK lmao look at this
// im writing a whole system just for one singletone
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct SelectedEntityTagSystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		foreach (var (circleTransform, circleEntity) in SystemAPI.Query<RefRW<LocalTransform>>()
		.WithEntityAccess()
		.WithAll<SelectCircle>())
		{
			bool targetExists = false;

			foreach (var npcTransform
			in SystemAPI.Query<RefRO<LocalTransform>>()
			.WithAll<NPC, SelectedEntityTag>())
			
			{
				targetExists = true;
				circleTransform.ValueRW.Position = npcTransform.ValueRO.Position;
			}

			// invisible if no target found
			SpriteRenderer renderer = state.EntityManager.GetComponentObject<SpriteRenderer>(circleEntity);
			renderer.color = targetExists ? Color.white : new Color(0, 0, 0, 0);
		}


	}
}