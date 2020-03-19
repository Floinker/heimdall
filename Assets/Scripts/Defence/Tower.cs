using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : DefenceObject, IDamagable
{
    [Header("Tower-Settings")]
    public GameObject projectile;
    public Transform projectileStart;
    public float fireRate = 5f;
    public float health = 100f;

    protected float currentTime;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        currentTime = fireRate;
    }

    protected virtual void FixedUpdate()
    {
        if (IsPlaced())
        {
            if (health <= 0) {
                GameObject.Destroy(this.gameObject);
                onDeath();
                return;
            }
            currentTime += Time.deltaTime;
            if (currentTime > fireRate)
            {
                currentTime = 0;
                Shoot();
            }
        }
    }

    protected abstract void Shoot();
    
    void IDamagable.TakeDamage(float amount){
        health -= amount;
    }

    public void onDeath() {
    }
}
