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
    public class TurretTargetFinder : JobComponentSystem {
        private EntityQuery minionQuery;
        private EntityManager entityManager;

        protected override void OnCreate() {
            minionQuery = GetEntityQuery(typeof(MinionData));
            var defaultworld = World.DefaultGameObjectInjectionWorld;
            entityManager = defaultworld.EntityManager;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            
            var minionCount = minionQuery.CalculateEntityCount();
            var towers = GameObject.FindObjectsOfType<DefenceObject>();

            if (towers.Length == 0) return inputDeps;
            
            var towersData = new NativeArray<towerData>(towers.Length, Allocator.TempJob);

            var jobResults = new NativeHashMap<int, float3>(minionCount, Allocator.TempJob);
            
            //setup data required for job
            for (int i = 0; i < towers.Length; i++) {
                towersData[i] = new towerData() {
                    GameObjectID = towers[i].GetInstanceID(),
                    range = towers[i].range,
                    towerPos = towers[i].transform.position,
                };
            }
            
            inputDeps = new findValidTowers() {
                towerPositions = towersData,
                resultList = jobResults.AsParallelWriter(),
            }.Schedule(this, inputDeps);
            
            inputDeps.Complete();

            foreach (var tower in towers) {
                if (jobResults.ContainsKey(tower.GetInstanceID()))
                    tower.target = jobResults[tower.GetInstanceID()];
            }

            jobResults.Dispose();
            towersData.Dispose();
            
            return inputDeps;
        }

        [BurstCompile]
        private struct findValidTowers : IJobForEachWithEntity<Translation, MinionData> {
            [ReadOnly] internal NativeArray<towerData> towerPositions;
            
            internal NativeHashMap<int, float3>.ParallelWriter resultList;

            public void Execute(Entity entity, int index, ref Translation pos, [ReadOnly] ref MinionData data) {
                Vector3 a = pos.Value;

                for (var i = 0; i < towerPositions.Length; i++) {
                    var towerData = towerPositions[i];
                    var b = towerData.towerPos;
                    if (Vector3.Distance(a, b) < towerData.range) {
                        resultList.TryAdd(towerData.GameObjectID, a);
                    }
                }
            }
        }

        private struct towerData {
            public int GameObjectID;
            public float range;
            public Vector3 towerPos;
        }

//        private struct searchJobResult {
//            public int GameObjectID;
//            public float3 targetPos;
//        }
    }
}