﻿using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ArcherController : MonoBehaviour {
    
    //bounds for arrow collision
    public float minY;
    public float maxY;

    //gets updated by ArcherTargetFinder system
    public static Dictionary<ArcherController, Vector3> targetPositions = new Dictionary<ArcherController, Vector3>();

    //used to create arrows
    [SerializeField] private GameObject arrowPrefab;
    private float progress;
    private Entity entityPrefab;
    private World defaultWorld;
    private EntityManager entityManager;
    private GameObjectConversionSettings settings;

    private void Start() {
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;

        settings = GameObjectConversionSettings.FromWorld(defaultWorld, new BlobAssetStore());
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(arrowPrefab, settings);
        //targetPositions[this] = Vector3.zero;
    }
    
    private void FixedUpdate() {
        if (!targetPositions.ContainsKey(this)) return;
        progress += Time.deltaTime;

        if (progress > 1) {
            progress = 0;
            spawnArrow(this.transform.position);
        }
    }
    
    private void spawnArrow(float3 position) {
        var entity = entityManager.Instantiate(entityPrefab);

        entityManager.SetComponentData(entity, new Translation {
            Value = position
        });

        entityManager.SetComponentData(entity, new ArrowData() {
            startPos = position,
            endPos = targetPositions[this],
            progress = 0f,
            minY = this.minY,
            maxY = this.maxY
        });
    }
}