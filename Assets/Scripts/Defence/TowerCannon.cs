using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCannon : DefenceObject
{
    // Start is called before the first frame update
    private GameObject topGameObject;
    
    public GameObject cannonBall;
    private Transform projectileStart;

    public float fireSpeed = 2f;

    private float currentTime;
    protected override void Start()
    {
        base.Start();
        
        currentTime = fireSpeed;
        foreach (Transform child in transform)
        {
            if (child.name == "Top")
            {
                topGameObject = child.gameObject;
            }

            foreach(Transform grandchild in child)
            {
                if (grandchild.name == "ProjectileStart")
                {
                    projectileStart = grandchild;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsPlaced())
        {
            RotateTo(topGameObject, target);

            currentTime += Time.deltaTime;
            
            if (target == Vector3.zero)
                return;
            if(currentTime > fireSpeed)
            {
                currentTime = 0;
                ShootCannonBallBall();
            }
        }
        
    }

    void RotateTo(GameObject obj, Vector3 target)
    {
        var direction = target - obj.transform.position;
        var rotation = Quaternion.LookRotation(direction);
        
        var r = obj.transform.eulerAngles;
        obj.transform.rotation = Quaternion.Euler(r.x, Mathf.LerpAngle(r.y, rotation.eulerAngles.y, Time.deltaTime), r.z);
    }

    void ShootCannonBallBall()
    {
        GameObject cb = Instantiate(cannonBall, new Vector3(projectileStart.position.x, projectileStart.position.y, projectileStart.position.z), Quaternion.identity, projectileStart);

        var direction = target - cb.transform.position;
        var rotation = Quaternion.LookRotation(direction);

        cb.GetComponent<Rigidbody>().AddForce(projectileStart.forward * 1000f);
        cb.GetComponent<GenericProjectile>().target = target;
    }
}
