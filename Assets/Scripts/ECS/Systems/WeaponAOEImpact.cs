using System;
using System.Collections.Generic;
using NavJob.Components;
using NavJob.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;
using Collider = UnityEngine.Collider;

namespace ECS.Systems {
    public class WeaponAOEImpact : JobComponentSystem {
        private EntityQuery minionQuery;
        private EntityManager entityManager;

        private ParticleSystemUtility particleUtility;

        private ParticleSystemUtility getParticle() {
            if (particleUtility != null) return particleUtility;
            particleUtility = ParticleSystemUtility.getInstance();
            return particleUtility;
        }

        public static List<impactData> impactPositions = new List<impactData>();

        protected override void OnCreate() {
            minionQuery = GetEntityQuery(typeof(NavAgent));
            var defaultworld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultworld.EntityManager;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var agentCount = minionQuery.CalculateEntityCount();

            if (impactPositions.Count <= 0 || agentCount <= 0) {
                return inputDeps;
            }

            var data = new NativeArray<impactData>(impactPositions.Count, Allocator.TempJob);
            var impacted = new NativeArray<minionHitData>(agentCount, Allocator.TempJob);

            for (int i = 0; i < impactPositions.Count; i++) {
                data[i] = impactPositions[i];
            }

            inputDeps = new applyImpacts() {
                ImpactPositions = data,
                toDelete = impacted
            }.Schedule(this, inputDeps);

            inputDeps.Complete();

            impactPositions.Clear();

            foreach (var elem in impacted) {
                entityManager.DestroyEntity(elem.entity);
                getParticle().doEmit(elem.minionPos);
            }

            data.Dispose();
            impacted.Dispose();

            return inputDeps;
        }

        [BurstCompile]
        private struct applyImpacts : IJobForEachWithEntity<Translation, MinionData, NavAgent> {
            [ReadOnly] internal NativeArray<impactData> ImpactPositions;
            internal NativeArray<minionHitData> toDelete;

            public void Execute(Entity entity, int index, ref Translation pos, [ReadOnly] ref MinionData data,
                [ReadOnly] ref NavAgent agent) {
                for (int i = 0; i < ImpactPositions.Length; i++) {
                    var dist = Vector3.Distance(pos.Value, ImpactPositions[i].impactPos);
                    if (dist < ImpactPositions[i].range) {
                        toDelete[index] = new minionHitData { entity = entity, impactPos = ImpactPositions[i].impactPos, minionPos = pos.Value};
                    }
                }
            }
        }

        public struct impactData {
            public float range;
            public Vector3 impactPos;
        }

        public struct minionHitData {
            public Vector3 impactPos;
            public Vector3 minionPos;
            public Entity entity;
        }

//        private struct searchJobResult {
//            public int GameObjectID;
//            public float3 targetPos;
//        }
    }
}