using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour {

    public GameObject target;
    
    // Start is called before the first frame update
    void Start() {
        this.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
    }
}