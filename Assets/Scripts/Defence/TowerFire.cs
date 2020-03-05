using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerFire : DefenceObject
{
    // Start is called before the first frame update
    private Transform projectileStart;

    public GameObject fireballPrefab;
    public float fireSpeed = 2f;

    public float fireballSpeed;

    private float currentTime;
    protected override void Start()
    {
        base.Start();
        currentTime = fireSpeed;
        target = GameObject.Find("TestTarget").transform.position;
        foreach (Transform child in transform)
        {
            if (child.name == "ProjectileStart")
            {
                projectileStart = child;
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(target, 1f);
    }

    private void FixedUpdate()
    {
        
        if (IsPlaced())
        {
            currentTime += Time.deltaTime;

            if(currentTime > fireSpeed)
            {
                ShootFireBall();
                currentTime = 0;
            }
        }
        
    }

    void ShootFireBall()
    {
        var projectile = Instantiate(fireballPrefab, new Vector3(projectileStart.position.x, projectileStart.position.y, projectileStart.position.z), Quaternion.identity, transform);

        //var dir = target - this.transform.position;
        projectile.GetComponent<GenericProjectile>().target = target;
    }
}
