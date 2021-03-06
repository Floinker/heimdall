﻿using System.Collections;
using System.Collections.Generic;
using ECS.Systems;
using UnityEngine;

public class GenericProjectile : MonoBehaviour {
    [Header("Projectile-Settings")] public float speed;
    public Vector3 target;
    public GameObject impactPrefab;
    public AudioSource impactSound;
    public List<GameObject> trails;


    protected Rigidbody rb;

    // Start is called before the first frame update
    protected virtual void Start() {
        RotateTo(gameObject, target);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate() {
        if (speed != 0 && rb != null) // && transform.parent.gameObject.GetComponent<Tower>().IsPlaced())
        {
            RotateTo(gameObject, target);
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
    }

    void RotateTo(GameObject obj, Vector3 target) {
        var direction = target - obj.transform.position;
        var rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1 * Time.deltaTime);
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        speed = 0;

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        if(impactSound != null)
            impactSound.Play(0);

        if (impactPrefab != null) {
            var impactFX = Instantiate(impactPrefab, pos, rot) as GameObject;
            Destroy(impactFX, 5);
        }

        if (trails.Count > 0) {
            for (int i = 0; i < trails.Count; i++) {
                trails[i].transform.parent = null;
                var ps = trails[i].GetComponent<ParticleSystem>();
                if (ps != null) {
                    ps.Stop();
                    Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
        }

        WeaponAOEImpact.impactPositions.Add(new WeaponAOEImpact.impactData {impactPos = contact.point, range = 4f});

        Destroy(gameObject);
    }
}