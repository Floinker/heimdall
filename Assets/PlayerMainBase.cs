using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMainBase : MonoBehaviour, IDamagable {
    
    [Header("Static Defence-Object-Settings")]
    public float health = 1000f;

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