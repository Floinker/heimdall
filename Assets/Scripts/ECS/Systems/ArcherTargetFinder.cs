using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ArcherTargetFinder : JobComponentSystem {

    private EntityQuery minionQuery;
    private int updates = 0;

    protected override void OnCreate() {
        minionQuery = GetEntityQuery(typeof(MinionData));
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps) {

        updates++;
        if (updates % 30 != 0) return inputDeps;

        var basePos = PlayerMainBase.basePosition;

        var count = minionQuery.CalculateEntityCount();
        var results = new NativeArray<minionResult>(count, Allocator.TempJob);

        inputDeps = new checkEntitesDistance() {
            basePos = basePos,
            results = results
        }.Schedule(this, inputDeps);
        
        inputDeps.Complete();

        var sortedResults = results.ToList().OrderBy(x => x.distance).ToList();
        
        var useResults = sortedResults.Count() > 30 ? 30 : sortedResults.Count();
        
        var newDict = new Dictionary<ArcherController, Vector3>();
        
        foreach (var elem in ArcherController.targetPositions) {
            var index = UnityEngine.Random.Range(0, useResults - 1);
            newDict[elem.Key] = sortedResults[index].position + Vector3.up + getRandOffset();
        }

        ArcherController.targetPositions = newDict;

        results.Dispose();

        return inputDeps;
    }

    private Vector3 getRandOffset() {
        return new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f));
    }

    [BurstCompile]
    private struct checkEntitesDistance : IJobForEachWithEntity<Translation, MinionData> {

        internal NativeArray<minionResult> results;
        [ReadOnly]
        internal Vector3 basePos;
        
        public void Execute(Entity entity, int index, ref Translation pos, ref MinionData data) {
            var dist = Vector3.Distance(basePos, pos.Value);
            var res = new minionResult() {position = pos.Value, distance = dist};
            results[index] = res;
        }
    }

    private struct minionResult {
        internal Vector3 position;
        internal float distance;
    }
}