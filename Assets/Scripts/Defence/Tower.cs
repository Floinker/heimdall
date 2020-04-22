using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : DefenceObject, IDamagable
{
    [Header("Tower-Settings")]
    public GameObject projectile;
    public Transform projectileStart;
    public Transform rangeObject;
    public float fireRate = 5f;
    public float health = 100f;

    private Vector3 currentTarget;
    private Vector3 lastTarget;

    protected float currentTime;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        currentTime = fireRate;
        rangeObject.localScale = new Vector3(range * 2, 0.02f, range * 2);
        lastTarget = Vector3.zero;
    }

    protected virtual void FixedUpdate()
    {
        if (IsPlaced())
        {
            rangeObject.gameObject.SetActive(false);
            if (health <= 0) {
                GameObject.Destroy(this.gameObject);
                onDeath();
                return;
            }
            if(target != Vector3.zero) {
                currentTarget = target;
                currentTime += Time.deltaTime;
                if (currentTime > fireRate && lastTarget != currentTarget)
                {
                    currentTime = 0;
                    Shoot();
                    if(shootSound != null)
                        shootSound.Play(0);
                    lastTarget = currentTarget;
                }
            }
            if (IsSelected())
            {
                rangeObject.gameObject.SetActive(true);
            }
        }
        else
        {
            rangeObject.gameObject.SetActive(true);
        }
    }

    protected abstract void Shoot();
    
    void IDamagable.TakeDamage(float amount){
        health -= amount;
    }

    public void onDeath() {

    }
}
