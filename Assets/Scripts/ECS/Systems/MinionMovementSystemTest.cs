using System;
using System.Collections.Generic;
using NavJob.Components;
using NavJob.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;

namespace ECS.Systems {
    public class MinionMovementSystemTest : JobComponentSystem {
        
        private EntityQuery minionQuery;
        private EntityManager entityManager;

        private ParticleSystemUtility particleUtility;

        private ParticleSystemUtility getParticle() {
            if (particleUtility != null) return particleUtility;
            particleUtility = ParticleSystemUtility.getInstance();
            return particleUtility;
        }

        protected override void OnCreate() {
            minionQuery = GetEntityQuery(typeof(MinionData));
            var defaultworld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultworld.EntityManager;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {

            var count = minionQuery.CalculateEntityCount();
            var inputs = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            var hits = new NativeArray<RaycastHit>(count, Allocator.TempJob);
            var toDelete = new NativeArray<Entity>(count, Allocator.TempJob);
            var particlePositions = new NativeArray<float3>(count, Allocator.TempJob);

            var layerName = LayerMask.NameToLayer("Defence");

            inputDeps = new setupRaycasts() {
                commands = inputs,
                layer = layerName
            }.Schedule(this, inputDeps);
            
            inputDeps = RaycastCommand.ScheduleBatch(inputs, hits, 1, inputDeps);
            
            inputDeps = new processResults() {
                hits = hits,
                toDelete = toDelete,
                hitPositions = particlePositions
            }.Schedule(this, inputDeps);
            
            inputDeps.Complete();

            foreach (var elem in particlePositions) {
                if (!elem.Equals(float3.zero))
                    getParticle().doEmit(elem + new float3(0, 1, 0));
            }
            
            entityManager.DestroyEntity(toDelete);

            toDelete.Dispose();
            particlePositions.Dispose();
            inputs.Dispose();

            return inputDeps;
        }

        [BurstCompile]
        private struct setupRaycasts : IJobForEachWithEntity<Translation, Rotation, MinionData> {
            //[NativeDisableParallelForRestriction]
            internal NativeArray<RaycastCommand> commands;
            internal int layer;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation pos, [ReadOnly] ref Rotation rot, [ReadOnly] ref MinionData data) {
                var origin = pos.Value + new float3(0, 1, 0);
                var quat = (Quaternion) rot.Value;
                var direction = quat * Vector3.forward;
                commands[index] = new RaycastCommand(origin, direction, 2.5f);
            }
        }
        
        [BurstCompile]
        private struct processResults : IJobForEachWithEntity<Translation, MinionData, NavAgent> {
            
            [DeallocateOnJobCompletion]
            [ReadOnly]
            internal NativeArray<RaycastHit> hits;
            
            internal NativeArray<float3> hitPositions;
            internal NativeArray<Entity> toDelete;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation trans, ref MinionData minionData, [ReadOnly] ref NavAgent agent) {
                var hit = hits[index];
                if (hit.distance <  5 && hit.distance > 0) {
                    //has close target, do remove
                    toDelete[index] = entity;
                    hitPositions[index] = agent.position;
                }
            }
        }
    }
}