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
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var squads = GameObject.FindGameObjectsWithTag("SquadController");

            NativeArray<positionTuple> dataStored = new NativeArray<positionTuple>(squads.Length, Allocator.TempJob);
            for (int i = 0; i < squads.Length; i++) {
                dataStored[i] = new positionTuple() {
                    id = squads[i].GetInstanceID(),
                    position = squads[i].transform.position
                };
            }

            inputDeps = new testJob() {
                dataStored = dataStored
            }.Schedule(this, inputDeps);

            return inputDeps;
        }

        [BurstCompile]
        private struct testJob : IJobForEachWithEntity<Translation, MinionData, NavAgent> {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<positionTuple> dataStored;

            public void Execute(Entity entity, int index, ref Translation trans, ref MinionData minionData,
                ref NavAgent agent) {
                positionTuple? squad = null;

                for (int i = 0; i < dataStored.Length; i++) {
                    if (dataStored[i].id.Equals(minionData.trackedID)) squad = dataStored[i];
                }

                if (squad == null) return;

//                var newPos = squad.Value.position + minionData.initialOffset;
//                trans.Value = newPos;

                //NavAgentSystem.SetDestinationStatic(entity, agent, squad.Value.position);
            }
        }

        private struct positionTuple {
            internal Int32 id;
            internal float3 position;
        }
    }
}