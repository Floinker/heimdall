﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public GameObject uiPrefab;
    private GameObject uiInstance;
    public GameObject costPrefab;
    private GameObject costinstance;
    private Text costText;
    private int currentPrefab = 0;
    public LayerMask relevantLayer;
    public int defenceLayer;

    private GameObject tower;
    private GameObject selectedObject;

    public Vector3 buildAreaSize;
    public Vector3 buildAreaCenter;

    private Button toggleBuildingModeButton;

    public bool isPlacing = false;

    private float startMouseX;


    // Start is called before the first frame update
    void Start()
    {
        SetupUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            ToggleBuildingMode();
        }
        

        if (isPlacing)
        {
            if (selectedObject != null)
            {
                selectedObject.GetComponent<DefenceObject>().SetSelected(false);
                selectedObject = null;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                if(tower && tower.GetComponent<DefenceObject>().CanPlace() && tower.GetComponent<DefenceObject>().CanAfford())
                {
                    InstantiateHoldingObject();
                }
                else if(tower == null)
                {
                    InstantiateHoldingObject();
                }
                
            }

            if (costinstance != null)
                costinstance.transform.position = Input.mousePosition;
            /*
            if (buildingArea.Contains(tower.transform.position))
            {
                tower.GetComponent<DefenceObject>().SetCanPlace(true);
            }
            else
            {
                tower.GetComponent<DefenceObject>().SetCanPlace(false);
            }
            */
            if (Input.GetButtonDown("Rotate"))
            {
                startMouseX = Input.mousePosition.x;
            }

            if(Input.GetButton("Rotate"))
            {
                if(tower != null)
                {
                    float actualRot = startMouseX - Input.mousePosition.x;
                    tower.transform.Rotate(0f, actualRot, 0f);
                    startMouseX = Input.mousePosition.x;
                }    
            }

            if(Input.GetButtonDown("CycleTowerLeft")){
                if(currentPrefab < 1)
                {
                    currentPrefab = towerPrefabs.Length - 1;
                }
                else
                {
                    currentPrefab--;
                }
                Destroy(tower);
                InstantiateHoldingObject();
            }

            if (Input.GetButtonDown("CycleTowerRight"))
            {
                if(currentPrefab > towerPrefabs.Length - 2)
                {
                    currentPrefab = 0;
                }
                else
                {
                    currentPrefab++;
                } 
                Destroy(tower);
                InstantiateHoldingObject();
            }

            if (tower)
            {
                RaycastHit hit = new RaycastHit();
                if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000f, relevantLayer))
                {
                    tower.transform.position = hit.point;
                }
            }
            else
            {
                
                tower = null;
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                SelectDefenceObject();

            }
        }

        
    }

    protected void SelectDefenceObject()
    {
        if(selectedObject != null)
        {
            selectedObject.GetComponent<DefenceObject>().SetSelected(false);
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if(hit.transform.root.gameObject.layer == defenceLayer)
            {
                selectedObject = hit.transform.root.gameObject;
                selectedObject.GetComponent<DefenceObject>().SetSelected(true);
            }
        }
    }

    void InstantiateHoldingObject()
    {
        if (costinstance != null)
            Destroy(costinstance);

        if (tower && GetComponent<PlayerStats>().playerCoins >= tower.GetComponent<DefenceObject>().upgrades[tower.GetComponent<DefenceObject>().currentLevel].cost)
        {
            Transform playerBase = GameObject.Find("NavMeshTarget").transform;

           
            float distanceToBase = Vector3.Distance(tower.transform.position, playerBase.position);

            AnalyticsHelper.towerPlaced(tower.GetComponent<DefenceObject>().GetTowerType(), distanceToBase, tower.GetComponent<DefenceObject>());
            tower.GetComponent<DefenceObject>().setIsPlaced(true);
        }

        var mousePos = Input.mousePosition;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit, 1000f, relevantLayer))
        {
            if(hit.transform.gameObject.layer != 5)
            {
                Vector3 point = hit.point;

                tower = Instantiate(towerPrefabs[currentPrefab], point, Quaternion.identity);
                costinstance = Instantiate(costPrefab, FindObjectOfType<Canvas>().transform);
                costText = costinstance.GetComponentInChildren<Text>();
                costText.text = tower.GetComponent<DefenceObject>().upgrades[tower.GetComponent<DefenceObject>().currentLevel].cost.ToString();
            }
            
        }

        GameObject[] childObjects = new GameObject[tower.transform.childCount];  
        int i = 0;

        foreach (Transform child in tower.transform)
        {
            childObjects[i] = child.gameObject;
            if (childObjects[i].GetComponent<Renderer>())
            {
                childObjects[i].GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
            }
            
            i++;
        }
    }

    private void SetupUI()
    {
        uiInstance = Instantiate(uiPrefab, FindObjectOfType<Canvas>().transform);
    }

    public void ToggleBuildingMode()
    {
        if (isPlacing)
        {
            Destroy(tower);
            if (costinstance != null)
                Destroy(costinstance);
        }
        else
        {
            InstantiateHoldingObject();
        }
       
        isPlacing = !isPlacing;
    }

    public void OnPointerEnter(BaseEventData data)
    {
        if (tower)
        {
            tower.GetComponent<DefenceObject>().SetCanPlace(false);
        }
    }

    public void OnPointerExit(BaseEventData data)
    {
        if (tower)
        {
            tower.GetComponent<DefenceObject>().SetCanPlace(true);
        } 
    }
}
