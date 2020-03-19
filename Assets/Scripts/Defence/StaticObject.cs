using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticObject : DefenceObject, IDamagable
{
    [Header("Static Defence-Object-Settings")]
    public float health = 100f;

    private void FixedUpdate() {
        if (health <= 0) {
            GameObject.Destroy(this.gameObject);
            return;
        }
    }

    void IDamagable.TakeDamage(float amount) {
        this.health -= amount;
    }

    public virtual void onDeath() {
        //to be implemented
    }
}
