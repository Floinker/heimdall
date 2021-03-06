﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.Jobs;
using System;

public class DefenceObject : MonoBehaviour {
    [Header("Configuration")]
    public AudioSource placeSound;
    public AudioSource shootSound;
    public Material highlightMat;
    public Material dissolveMat;
    public GameObject uiPrefab;
    public NavMeshSurface surface;
    public float range;
    public Vector3 target;

    public GameObject objectPlacement;

    private Bounds buildingArea;

    [Header("Defence Upgrades")] public List<PlayerUpgrade> upgrades = new List<PlayerUpgrade>();

    public int currentLevel = 0;

    //
    private List<GameObject> childObjects;
    private GameObject uiInstance;

    private List<GameObject> upgradePrefabs;
    private Dictionary<GameObject, Material> materialMapping;

    protected Button levelUpButton;
    protected Button destroyButton;
    protected Text tooltipText;
    protected Image coinImage;
    protected Text coinCount;

    private PlayerStats playerStats;

    private bool isPlaced = false;
    private bool isDissolved = false;
    private bool canPlace = false;
    private bool isSelected = false;

    private bool canAfford = false;

    private float lerpTime = 0.0f;
    private int collisionCount;

    private bool playerCharged = false;

    //Analytics
    private Guid guid;

    // Start is called before the first frame update
    protected virtual void Start() {
        guid = System.Guid.NewGuid();

        buildingArea = new Bounds(objectPlacement.GetComponent<ObjectPlacement>().buildAreaCenter, objectPlacement.GetComponent<ObjectPlacement>().buildAreaSize);

        int childCount = 0;
        collisionCount = 0;
        playerStats = FindObjectOfType<PlayerStats>();
        upgradePrefabs = new List<GameObject>();


        foreach (PlayerUpgrade upgrade in upgrades) {
            upgradePrefabs.Add(upgrade.upgrade.defencePrefab);
        }

        foreach (Transform child in transform) {
            if (child.GetComponent<Renderer>()) {
                childCount++;
            }

            foreach (Transform grandchild in child.transform) {
                if (grandchild.GetComponent<Renderer>()) {
                    childCount++;
                }
            }
        }

        childObjects = new List<GameObject>();
        materialMapping = new Dictionary<GameObject, Material>();

        highlightMat.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));

        GetAllChildObjects(transform, childObjects);
    }

    private void GetAllChildObjects(Transform parent, List<GameObject> childList)
    {
        foreach(Transform child in parent)
        {
            Renderer r = null;
            if ((r = child.GetComponent<Renderer>()) != null)
            {
                childList.Add(child.gameObject);
                materialMapping.Add(child.gameObject, r.material);
                Texture baseTex = r.material.mainTexture;
                r.material = highlightMat;
                r.material.SetTexture("BaseTexture", baseTex);
            }
            GetAllChildObjects(child, childList);
        }
    }

    private void SetupUI() {
        uiInstance = Instantiate(uiPrefab, FindObjectOfType<Canvas>().transform);

        levelUpButton = uiInstance.GetComponentsInChildren<Button>()[0];
        levelUpButton.onClick.AddListener(LevelUp);
        destroyButton =
            new List<Button>(levelUpButton.GetComponentsInChildren<Button>()).Find(img => img != levelUpButton);
        destroyButton.onClick.AddListener(DestroyObject);
        tooltipText = uiInstance.GetComponentInChildren<Text>();
        if (currentLevel < upgrades.Capacity - 1) {
            tooltipText.text = upgrades[currentLevel + 1].upgrade.description;
        }
        else {
            tooltipText.text = "Max Level reached";
        }

        tooltipText.color = new Color(0, 0, 0, 0);
        tooltipText.transform.parent.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        coinImage = tooltipText.transform.GetComponentInChildren<Image>();
        coinCount = tooltipText.transform.GetComponentsInChildren<Text>()[1];

        if (currentLevel < upgrades.Capacity - 1 )
        {
            coinCount.text = upgrades[currentLevel + 1].cost.ToString();
        }
        else
        {
            coinCount.gameObject.SetActive(false);
            coinImage.gameObject.SetActive(false);
        }
            
        //tooltipText.transform.parent.gameObject.SetActive(false);
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (isPlaced) {

            if (placeSound != null)
            {
                placeSound.Play(0);
                placeSound = null;
            }

            if (!playerCharged)
            {
                playerStats.playerCoins -= upgrades[currentLevel].cost;
                playerCharged = true;
            }

            if (!isDissolved)
            {
                lerpTime += 0.5f * Time.deltaTime;
                float lerpVal = 0; ;
                foreach (GameObject go in childObjects)
                {
                    
                    Texture baseTexture = materialMapping[go].mainTexture;
                    go.GetComponent<Renderer>().material = dissolveMat;
                    go.GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
                    lerpVal = Mathf.Lerp(1, 0, lerpTime);
                    go.GetComponent<Renderer>().material.SetFloat("Dissolve", lerpVal);
                }

                if (lerpVal == 0)
                    isDissolved = true;
               
            }
               

            if (isDissolved) { 
                if (!destroyButton)
                {
                    SetupUI();
                }

                uiInstance.transform.position =
                    Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0f, 0f));

                foreach (GameObject go in childObjects)
                {
                    if (go.GetComponent<Renderer>())
                    {
                        if (isSelected)
                        {
                            uiInstance.SetActive(true);
                            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            Texture baseTexture = materialMapping[go].mainTexture;
                            go.GetComponent<Renderer>().material = highlightMat;
                            go.GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
                            go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 0f, 1f, 0.2f));
                            go.GetComponent<Renderer>().material.SetFloat("alphaValue", .3f);
                        }
                        else
                        {
                            uiInstance.SetActive(false);
                            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            go.GetComponent<Renderer>().material = materialMapping[go];
                            go.GetComponent<Renderer>().material.SetFloat("alphaValue", 1f);
                        }
                    }
                }
            }
        }
        else {
            if (collisionCount < 1 && buildingArea.Contains(transform.position)){
                canPlace = true;
                foreach (GameObject go in childObjects)
                {
                    if (go.GetComponent<Renderer>() != null)
                    {
                        //go.GetComponent<Renderer>().material = highlightMat;
                        go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
                        go.GetComponent<Renderer>().material.SetFloat("alphaValue", .3f);
                    }
                }
            }
            else{
                canPlace = false;
                foreach (GameObject go in childObjects)
                {
                    if (go.GetComponent<Renderer>())
                    {
                        //go.GetComponent<Renderer>().material = highlightMat;
                        go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));
                        go.GetComponent<Renderer>().material.SetFloat("alphaValue", .3f);
                    }
                }
            }

            foreach (GameObject go in childObjects)
            {
                if (canPlace && canAfford && isDissolved)
                {
                    go.GetComponent<Renderer>().material = highlightMat;
                    go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
                    go.GetComponent<Renderer>().material.SetFloat("alphaValue", .3f);
                }
                else if (isDissolved)
                {
                    go.GetComponent<Renderer>().material = highlightMat;
                    go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));
                    go.GetComponent<Renderer>().material.SetFloat("alphaValue", .3f);
                }
            }

            if (upgrades[currentLevel].cost <= playerStats.playerCoins)
            {
                canAfford = true;
            }
            else
            {
                canAfford = false;
            }
        }
        
    }

    void LevelUp() {
        if (currentLevel < upgrades.Capacity - 1) {
            if (playerStats.playerCoins >= upgrades[currentLevel + 1].cost && currentLevel < upgrades.Capacity - 1) {
                Debug.Log("Level Up!" + transform.name);
                currentLevel++;
                playerStats.playerCoins -= upgrades[currentLevel].cost;
                GameObject temp = Instantiate(upgradePrefabs[currentLevel], transform.position, Quaternion.identity);
                temp.GetComponent<DefenceObject>().playerCharged = true;
                temp.GetComponent<DefenceObject>().setIsPlaced(true);

                AnalyticsHelper.towerUpgraded(GetTowerType(), currentLevel, this);

                DestroyObject();
            }
        }
    }

    void DestroyObject() {
        Destroy(uiInstance);
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other) {
        collisionCount++;
        //TODO Check auf Layer
        canPlace = false;

        if (!isPlaced) {
            
        }
    }


    private void OnTriggerExit(Collider other) {
        collisionCount--;
        canPlace = true;
        if (!isPlaced) {
            
        }
    }

    public virtual void setIsPlaced(bool isPlaced) {
        this.isPlaced = isPlaced;
    }

    public bool IsPlaced() {
        return this.isPlaced;
    }

    public bool CanPlace() {
        return canPlace;
    }

    public void SetCanPlace(bool canPlace) {
        this.canPlace = canPlace;
    }

    public bool CanAfford() {
        return canAfford;
    }

    public Vector3 GetTarget() {
        return target;
    }

    public void SetSelected(bool isSelected) {
        this.isSelected = isSelected;
    }

    public bool IsSelected()
    {
        return this.isSelected;
    }
    public string GetTowerType()
    {
        string type = transform.name;

        if (type.Contains("Cannon"))
        {
            type = "Cannon";
        }
        else if (type.Contains("Fire"))
        {
            type = "Fire";
        }
        else if (type.Contains("Wall"))
        {
            type = "Wall";
        }
        else if (type.Contains("Archer"))
        {
            type = "Archer";
        }
        else
        {
            type = "type not recognized";
        }

        return type;
    }

    public Guid getGUID()
    {
        return this.guid;
    }
}