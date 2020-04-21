using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AI;

public class DefenceObject2 : MonoBehaviour {
    [Header("Configuration")] public Material highlightMat;
    public Material dissolveMat;
    public GameObject uiPrefab;
    public NavMeshSurface surface;
    public float range;
    public Vector3 target;

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

    private PlayerStats playerStats;

    private bool isPlaced = false;
    private bool isDissolved = false;
    private bool canPlace = false;
    private bool isSelected = false;

    private bool canAfford = false;

    // Start is called before the first frame update
    protected virtual void Start() {
        int childCount = 0;
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
        //tooltipText.transform.parent.gameObject.SetActive(false);
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (isPlaced) {
            if (!isDissolved)
                ReverseDissolve();
            if (isDissolved)
            {
                if (!destroyButton)
                    SetupUI();
                uiInstance.transform.position =
                    Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0f, 0f));
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
                temp.GetComponent<DefenceObject>().setIsPlaced(true);

                AnalyticsHelper.towerUpgraded(GetTowerType(), currentLevel);

                DestroyObject();
            }
        }
    }

    void ReverseDissolve()
    {
        foreach(GameObject go in childObjects)
        {
            Texture baseTexture = materialMapping[go].mainTexture;
            go.GetComponent<Renderer>().material = dissolveMat;
            go.GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
        }

        float time = 1f;
        while(time > 0)
        {
            time -= Time.deltaTime;
            foreach(GameObject go in childObjects)
            {
                go.GetComponent<Renderer>().material.SetFloat("Dissolve", time);
            }
        }
        isDissolved = true;
        Debug.Log("Done Dissolving Tower");
    }

    void DestroyObject() {
        Destroy(uiInstance);
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other) {
        //TODO Check auf Layer
        canPlace = false;

        if (!isPlaced) {
            foreach (GameObject go in childObjects) {
                if (go.GetComponent<Renderer>()) {
                    //go.GetComponent<Renderer>().material = highlightMat;
                    go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));
                }
            }
        }
    }
     

    private void OnTriggerExit(Collider other) {
        canPlace = true;
        if (!isPlaced) {
            foreach (GameObject go in childObjects) {
                if (go.GetComponent<Renderer>() != null) {
                    //go.GetComponent<Renderer>().material = highlightMat;
                    go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
                }
            }
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
}