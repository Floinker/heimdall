using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCannon : Tower
{
    // Start is called before the first frame update
    private GameObject topGameObject;

    protected override void Start()
    {
        base.Start();

        foreach(Transform child in transform)
        {
            if(child.name == "Top")
            {
                topGameObject = child.gameObject;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsPlaced())
        {
            RotateTo(topGameObject, target);

            if (target == Vector3.zero)
                return;
        }
    }

    protected override void Shoot()
    {
        GameObject cb = Instantiate(projectile, new Vector3(projectileStart.position.x, projectileStart.position.y, projectileStart.position.z), Quaternion.identity);

        var direction = target - cb.transform.position;
        
        cb.GetComponent<Rigidbody>().AddForce(projectileStart.forward * 1000f);
        cb.GetComponent<GenericProjectile>().target = target;
    }

    void RotateTo(GameObject obj, Vector3 target)
    {
        var direction = target - obj.transform.position;
        var rotation = Quaternion.LookRotation(direction);

        var r = obj.transform.eulerAngles;
        obj.transform.rotation = Quaternion.Euler(r.x, Mathf.LerpAngle(r.y, rotation.eulerAngles.y, Time.deltaTime), r.z);
    }
}
