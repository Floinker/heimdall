﻿using System;
using NavJob.Components;
using NavJob.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawner : MonoBehaviour {
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private GameObject squadLeader;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int countX;
    [SerializeField] private int countY;
    [SerializeField] private float spacing;


    private Entity entityPrefab;
    private World defaultworld;
    private EntityManager entityManager;
    private int counter = 0;

    private void Start() {
        defaultworld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultworld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(defaultworld, null);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(minionPrefab, settings);
    }

    private void FixedUpdate() {
        counter++;
        if (counter == 10) {
            spawnMinionGroup(this.transform.position, countX, countY, spacing);
        }
    }

    private void spawnMinionGroup(float3 startAt, int xCount, int yCount, float spacingBetween) {
        for (int i = 0; i < xCount; i++) {
            for (int j = 0; j < yCount; j++) {
                var newPos = new float3(startAt.x + i * spacingBetween, startAt.y, startAt.z + j * spacingBetween);
                instantiateEntity(newPos);
            }
        }
    }

    private void instantiateEntity(float3 position) {
        var entity = entityManager.Instantiate(entityPrefab);
        var targetPosition = targetObject.transform.position;

        entityManager.SetComponentData(entity, new Translation {
            Value = position
        });

        entityManager.AddComponent(entity, typeof(NavAgent));
        entityManager.SetComponentData(entity, new NavAgent {
            moveSpeed = 10f,
            acceleration = 1,
            position = position,
            stoppingDistance = 1,
            rotationSpeed = 180
        });

        entityManager.AddComponent(entity, typeof(SyncPositionFromNavAgent));
        entityManager.AddComponent(entity, typeof(SyncRotationFromNavAgent));

        entityManager.SetComponentData(entity, new MinionData() {
            trackedID = squadLeader.GetInstanceID(),
            initialOffset = (float3) targetPosition - position
        });

        NavAgentSystem.SetDestinationStatic(entity, entityManager.GetComponentData<NavAgent>(entity), targetPosition, 1);
    }
}