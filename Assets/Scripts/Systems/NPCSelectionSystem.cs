using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NPCSelectionSystem: ISystem
{
    public void OnCreate(ref SystemState state)
    {       
    }

    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetMouseButtonUp(0))
        {
            state.EntityManager.RemoveComponent<SelectedEntityTag>(state.GetEntityQuery(typeof(SelectedEntityTag)));

            var buildPhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayStart = ray.origin;
            var rayEnd = ray.GetPoint(100f);

            if (Raycast(collisionWorld, rayStart, rayEnd, out var raycastHit))
            {
                var hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;

                if (state.EntityManager.AddComponent<SelectedEntityTag>(hitEntity))
                {
                    Debug.Log(hitEntity.Index.ToString());
                }
        }
        }
    }

    private bool Raycast(CollisionWorld world, float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
    {
        var raycastInput = new RaycastInput
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