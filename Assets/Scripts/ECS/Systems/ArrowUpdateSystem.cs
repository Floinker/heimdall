using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class ArrowUpdateSyste : JobComponentSystem {
    private EntityManager entityManager;
    private EntityQuery arrowQuery;

    private ParticleSystemUtility particleUtility;

    private ParticleSystemUtility getParticle() {
        if (particleUtility != null) return particleUtility;
        particleUtility = ParticleSystemUtility.getInstance();
        return particleUtility;
    }

    protected override void OnCreate() {
        arrowQuery = GetEntityQuery(typeof(ArrowData));
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var count = arrowQuery.CalculateEntityCount();
        var deltaTime = Time.DeltaTime;

        var toDelete = new NativeArray<Entity>(count, Allocator.TempJob);
        var hitEnemies = new NativeArray<Entity>(count, Allocator.TempJob);
        var particlePositions = new NativeArray<Vector3>(count, Allocator.TempJob);

        var buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        inputDeps = new arrowJobUpdate() {
            delta = deltaTime,
            toDelete = toDelete,
            hitEnemies = hitEnemies,
            hitEnemiesPositions = particlePositions,
            collisionWorld = collisionWorld
        }.Schedule(this, inputDeps);


        inputDeps.Complete();

        foreach (var elem in particlePositions) {
            if (elem.Equals(float3.zero)) continue;

            getParticle().doEmit(elem + new Vector3(0, 1, 0));
            ScoreDisplay.score++;
        }

        entityManager.DestroyEntity(toDelete);
        entityManager.DestroyEntity(hitEnemies);

        toDelete.Dispose();
        particlePositions.Dispose();
        hitEnemies.Dispose();

        return inputDeps;
    }

    [BurstCompile]
    private struct arrowJobUpdate : IJobForEachWithEntity<Translation, Rotation, ArrowData> {
        internal float delta;

        [ReadOnly] internal CollisionWorld collisionWorld;

        internal NativeArray<Entity> toDelete;
        internal NativeArray<Entity> hitEnemies;
        internal NativeArray<Vector3> hitEnemiesPositions;

        //formula for height position (from 0-1): y = 2.775558e-16 + 4.166667*x - 4.166667*x^2
        //max height: y = 0.1 - 0.01747059*x + 0.005164706*x^2

        public void Execute(Entity entity, int index, ref Translation pos, ref Rotation rot, ref ArrowData arrowData) {
            arrowData.progress += delta;
            if (arrowData.progress >= 10f)
                toDelete[index] = entity;
            
            if (arrowData.done) return;
            
            var dir = arrowData.endPos - arrowData.startPos;

            //needs 1 sec per 100 meters
            var x = arrowData.progress / (dir.magnitude / 100f);
            var dirScaled = dir * x;

            var maxHeight = (float) (0.1 + 0.005164706f * Mathf.Pow(dir.magnitude, 2));
            maxHeight /= 5f;
            if (maxHeight > 100) maxHeight = 50;
            var height = (float) (4.166667 * x - 4.166667 * Mathf.Pow(x, 2)) * maxHeight;

            var newPos = arrowData.startPos + dirScaled + new Vector3(0, height, 0);

            var currentVelocity = (Vector3) pos.Value - newPos;
            currentVelocity.Normalize();
            if (!currentVelocity.Equals(Vector3.zero)) {
                var newRot = Quaternion.LookRotation(currentVelocity) * Quaternion.Euler(90f, 0, 0);
                rot.Value = newRot;
            }

            pos.Value = newPos;

            //at right height, setup ray
            if (newPos.y < arrowData.maxY && newPos.y > arrowData.minY) {
                RaycastInput input = new RaycastInput {
                    Start = newPos,
                    End = newPos + currentVelocity * 5f,
                    Filter = CollisionFilter.Default
                };

                var hits = new NativeList<RaycastHit>(Allocator.Temp);

                collisionWorld.CastRay(input, ref hits);

                if (hits.Length >= 1) {
                    var hit = hits[0];
                    toDelete[index] = entity;
                    var hitEntity = collisionWorld.Bodies[hit.RigidBodyIndex];
                    hitEnemies[index] = hitEntity.Entity;
                    hitEnemiesPositions[index] = hit.Position;
                }

                hits.Dispose();
            }

            if (newPos.y <= arrowData.minY) {
                arrowData.done = true;
            }
        }
    }
}