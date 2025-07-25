using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NPCSelectionSystem: ISystem
{
    EntityQuery m_query;

    public void OnCreate(ref SystemState state)
    {
        m_query = state.GetEntityQuery(typeof(SelectedEntityTag));
    }

    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetMouseButtonUp(0))
        {
            state.EntityManager.RemoveComponent<SelectedEntityTag>(m_query);

            var buildPhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayStart = ray.origin;
            Vector3 rayEnd = ray.GetPoint(100f);

            if (Raycast(collisionWorld, rayStart, rayEnd, out RaycastHit raycastHit))
            {
                Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;

                if (state.EntityManager.AddComponent<SelectedEntityTag>(hitEntity))
                {
                    Debug.Log(hitEntity.Index.ToString());
                }
        }
        }
    }

    [BurstCompile]
    private bool Raycast(CollisionWorld world, float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
    {
        RaycastInput raycastInput = new()
        {
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint) (1 << 1),
                CollidesWith = (uint) (1 << 0)
            }
        };

        return world.CastRay(raycastInput, out raycastHit);
    }
}