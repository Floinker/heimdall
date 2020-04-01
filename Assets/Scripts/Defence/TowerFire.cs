using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerFire : Tower
{
    public float fireballSpeed;

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(target, 1f);
    }

    protected override void Shoot()
    {
        GameObject fb = Instantiate(projectile, new Vector3(projectileStart.position.x, projectileStart.position.y, projectileStart.position.z), Quaternion.identity, transform);

        //var dir = target - this.transform.position;
        fb.GetComponent<GenericProjectile>().target = target;
    }
}
