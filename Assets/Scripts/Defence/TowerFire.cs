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
        target = GameObject.Find("TestTarget");
        foreach (Transform child in transform)
        {
            if (child.name == "ProjectileStart")
            {
                projectileStart = child;
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
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

    public GameObject GetTarget()
    {
        return this.target;
    }

    void ShootFireBall()
    {
        Instantiate(fireballPrefab, new Vector3(projectileStart.position.x, projectileStart.position.y, projectileStart.position.z), Quaternion.identity, transform);
    }
}
