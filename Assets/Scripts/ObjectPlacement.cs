using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacement : MonoBehaviour
{
    public GameObject towerPrefab;
    public LayerMask relevantLayer;

    private GameObject tower;

    public bool isPlacing = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isPlacing = !isPlacing;
        }
        

        if (isPlacing)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if(tower && tower.GetComponent<DefenceObject>().CanPlace())
                {
                    InstantiateHoldingObject();
                }
                else if(tower == null)
                {
                    InstantiateHoldingObject();
                }
                
            }

            if (tower)
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000f, relevantLayer))
                {
                    tower.transform.position = hit.point;
                }
            }
            else
            {
                
                tower = null;
            }
        }

        
    }

    void InstantiateHoldingObject()
    {

        if (tower)
        {
            tower.GetComponent<DefenceObject>().setIsPlaced(true);
        }

        var mousePos = Input.mousePosition;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit, 1000f, relevantLayer))
        {
            Vector3 point = hit.point;

            Debug.Log(point);
            tower = Instantiate(towerPrefab, point, Quaternion.identity);
        }

        GameObject[] childObjects = new GameObject[tower.transform.childCount];  
        int i = 0;

        foreach (Transform child in tower.transform)
        {
            childObjects[i] = child.gameObject;
            childObjects[i].GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f,1f,0f,0.5f));
            i++;
        }
    }
}
