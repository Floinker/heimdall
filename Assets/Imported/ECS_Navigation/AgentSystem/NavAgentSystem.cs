#region

using System.Collections.Concurrent;
using NavJob.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Random = System.Random;
using RaycastHit = Unity.Physics.RaycastHit;

#endregion

namespace NavJob.Systems {
    ////[DisableAutoCreation]
    public class NavAgentSystem : JobComponentSystem {
        private struct AgentData {
            public int index;
            public Entity entity;
            public NavAgent agent;
        }

        private NativeQueue<AgentData> needsWaypoint;
        private ConcurrentDictionary<int, Vector3[]> waypoints = new ConcurrentDictionary<int, Vector3[]>();
        private NativeHashMap<int, AgentData> pathFindingData;

        [BurstCompile]
        private struct DetectNextWaypointJob : IJobForEachWithEntity<NavAgent> {
            public int navMeshQuerySystemVersion;

            public void Execute(Entity entity, int index, ref NavAgent agent) {
                if (agent.remainingDistance - agent.stoppingDistance > 2 || agent.status != AgentStatus.Moving) {
                    return;
                }

                if (agent.nextWaypointIndex != agent.totalWaypoints) {
                    agent.nextWayPoint = true;
                }
                else if (navMeshQuerySystemVersion != agent.queryVersion ||
                         agent.nextWaypointIndex == agent.totalWaypoints) {
                    agent.totalWaypoints = 0;
                    agent.currentWaypoint = 0;
                    agent.status = AgentStatus.Idle;
                }
            }
        }

        [BurstCompile]
        private struct localAvoidanceCheck : IJobForEachWithEntity<NavAgent> {
            internal float dt;
            [ReadOnly] public CollisionWorld collisionWorld;

            public void Execute(Entity entity, int index, ref NavAgent agent) {
                var origin = agent.position + new float3(0, 0.5f, 0);
                var direction = agent.currentWaypoint - origin;

                var doAvoid = true;
                var avoidDir = direction;

                for (int j = 0; j < 4; j++) {
                    var dir = Quaternion.AngleAxis(-30 + j * 30, Vector3.up) * direction;
                    dir.Normalize();
                    dir *= 2f;
                    RaycastInput input = new RaycastInput {
                        Start = origin,
                        End = origin + (float3) dir,
                        Filter = CollisionFilter.Default
                    };

                    var hits = new NativeList<RaycastHit>(Allocator.Temp);

                    collisionWorld.CastRay(input, ref hits);
                    
                    if (hits.Length <= 1) {
                        if (index == 0)
                            doAvoid = false;
                        break;
                    }
                    avoidDir = hits[0].Position - origin;

                    hits.Dispose();
                }

                if (doAvoid) {

                    if (avoidDir.Equals(float3.zero)) {
                        avoidDir = direction;
                    }

                    avoidDir = ((Vector3) avoidDir).normalized;
                    avoidDir.y = 0;
                    avoidDir *= 1.1f;

                    agent.nextPosition = agent.position + avoidDir * agent.currentMoveSpeed * dt;
                }
            }
        }

        private struct SetNextWaypointJob : IJobForEachWithEntity<NavAgent> {
            public void Execute(Entity entity, int index, ref NavAgent agent) {
                if (agent.nextWayPoint) {
                    if (instance.waypoints.TryGetValue(entity.Index, out Vector3[] currentWaypoints)) {
                        agent.currentWaypoint = currentWaypoints[agent.nextWaypointIndex];
                        agent.remainingDistance = Vector3.Distance(agent.position, agent.currentWaypoint);
                        agent.nextWaypointIndex++;
                        agent.nextWayPoint = false;
                    }
                }
            }
        }

        [BurstCompile]
        private struct MovementJob : IJobForEachWithEntity<NavAgent> {
            private readonly float dt;
            private readonly float3 up;
            private readonly float3 one;


            public MovementJob(float dt) {
                this.dt = dt;
                up = Vector3.up;
                one = Vector3.one;
            }

            public void Execute(Entity entity, int index, ref NavAgent agent) {
                if (agent.status != AgentStatus.Moving) {
                    return;
                }

                if (!agent.dontWarp) {
                    agent.nextPosition = agent.position;
                    agent.dontWarp = true;
                    //agent.remainingDistance = 0;
                }

                if (agent.remainingDistance > 0) {
                    agent.currentMoveSpeed =
                        Mathf.Lerp(agent.currentMoveSpeed, agent.moveSpeed, dt * agent.acceleration);
                    // todo: deceleration
                    if (!float.IsPositiveInfinity(agent.nextPosition.x)) {
                        agent.position = agent.nextPosition;
                    }

                    var heading = (Vector3) (agent.currentWaypoint - agent.position);
                    agent.remainingDistance = heading.magnitude;
                    if (agent.remainingDistance > 0.001f) {
                        var targetRotation = Quaternion.LookRotation(heading, up).eulerAngles;
                        targetRotation.x = targetRotation.z = 0;
                        if (agent.remainingDistance < 1) {
                            agent.rotation = Quaternion.Euler(targetRotation);
                        }
                        else {
                            agent.rotation = Quaternion.Slerp(agent.rotation, Quaternion.Euler(targetRotation),
                                dt * agent.rotationSpeed);
                        }
                    }

                    var forward = math.forward(agent.rotation) * agent.currentMoveSpeed * dt;
                    agent.nextPosition = agent.position + forward;
                }
                else if (agent.nextWaypointIndex == agent.totalWaypoints) {
                    agent.nextPosition = new float3 {x = Mathf.Infinity, y = Mathf.Infinity, z = Mathf.Infinity};
                    agent.status = AgentStatus.Idle;
                }
            }
        }

        private struct InjectData {
            [ReadOnly] public NativeArray<Entity> Entities;
            public NativeArray<NavAgent> Agents;
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var dt = Time.DeltaTime;
            var count = navAgentQuery.CalculateEntityCount();
            var buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            inputDeps = new DetectNextWaypointJob {navMeshQuerySystemVersion = querySystem.Version}.Schedule(this, inputDeps);
            inputDeps = new SetNextWaypointJob().Schedule(this, inputDeps);
            inputDeps = new localAvoidanceCheck {dt = dt, collisionWorld = collisionWorld}.Schedule(this, JobHandle.CombineDependencies(inputDeps, buildPhysicsWorld.FinalJobHandle));
            inputDeps = new MovementJob(dt).Schedule(this, inputDeps);
            return inputDeps;
        }

        /// <summary>
        /// Used to set an agent destination and start the pathfinding process
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="agent"></param>
        /// <param name="destination"></param>
        public void SetDestination(Entity entity, NavAgent agent, Vector3 destination, int areas = -1) {
            if (pathFindingData.TryAdd(entity.Index,
                new AgentData {index = entity.Index, entity = entity, agent = agent})) {
                agent.status = AgentStatus.PathQueued;
                agent.destination = destination;
                agent.queryVersion = querySystem.Version;
                EntityManager.SetComponentData(entity, agent);
                querySystem.RequestPath(entity.Index, agent.position, agent.destination, areas);
            }
        }

        /// <summary>
        /// Static counterpart of SetDestination
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="agent"></param>
        /// <param name="destination"></param>
        public static void SetDestinationStatic(Entity entity, NavAgent agent, Vector3 destination, int areas = -1) {
            instance.SetDestination(entity, agent, destination, areas);
        }

        protected static NavAgentSystem instance;
        private NavMeshQuerySystem querySystem;
        private EntityQuery navAgentQuery;

        protected override void OnCreate() {
            querySystem = EntityManager.World.GetOrCreateSystem<NavMeshQuerySystem>();
            instance = this;
            querySystem.RegisterPathResolvedCallback(OnPathSuccess);
            querySystem.RegisterPathFailedCallback(OnPathError);
            needsWaypoint = new NativeQueue<AgentData>(Allocator.Persistent);
            pathFindingData = new NativeHashMap<int, AgentData>(0, Allocator.Persistent);
            navAgentQuery = GetEntityQuery(typeof(NavAgent));
        }

        protected override void OnDestroy() {
            needsWaypoint.Dispose();
            pathFindingData.Dispose();
        }

        private void SetWaypoint(Entity entity, NavAgent agent, Vector3[] newWaypoints) {
            waypoints[entity.Index] = newWaypoints;
            agent.status = AgentStatus.Moving;
            agent.nextWaypointIndex = 1;
            agent.totalWaypoints = newWaypoints.Length;
            agent.currentWaypoint = newWaypoints[0];
            agent.remainingDistance = Vector3.Distance(agent.position, agent.currentWaypoint);
            EntityManager.SetComponentData(entity, agent);
        }

        private void OnPathSuccess(int index, Vector3[] waypoints) {
            if (pathFindingData.TryGetValue(index, out AgentData entry)) {
                SetWaypoint(entry.entity, entry.agent, waypoints);
                pathFindingData.Remove(index);
            }
        }

        private void OnPathError(int index, PathfindingFailedReason reason) {
            if (pathFindingData.TryGetValue(index, out AgentData entry)) {
                entry.agent.status = AgentStatus.Idle;
                EntityManager.SetComponentData(entry.entity, entry.agent);
                pathFindingData.Remove(index);
            }
        }
    }
}