using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AI;

public class DefenceObject : MonoBehaviour {
    [Header("Configuration")] public Material highlightMat;
    public Material dissolveMat;
    public GameObject uiPrefab;
    public NavMeshSurface surface;
    public float range;
    public Vector3 target;

    [Header("Defence Upgrades")] public List<PlayerUpgrade> upgrades = new List<PlayerUpgrade>();

    public int currentLevel = 0;

    //
    private GameObject[] childObjects;
    private GameObject uiInstance;

    private List<GameObject> upgradePrefabs;
    private Dictionary<GameObject, Material> highlightMaterialMapping;

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

        childObjects = new GameObject[childCount];
        highlightMaterialMapping = new Dictionary<GameObject, Material>();

        highlightMat.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));

        int i = 0;

        foreach (Transform child in transform) {
            if (child.gameObject.GetComponent<Renderer>()) {
                childObjects[i] = child.gameObject;
                highlightMaterialMapping.Add(childObjects[i], childObjects[i].GetComponent<Renderer>().material);
                Texture baseTexture = childObjects[i].GetComponent<Renderer>().material.mainTexture;
                childObjects[i].GetComponent<Renderer>().material = highlightMat;
                childObjects[i].GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);

                i++;
            }

            int j = 0;
            foreach (Transform grandchild in child.transform) {
                if (grandchild.gameObject.GetComponent<Renderer>()) {
                    childObjects[i + j] = grandchild.gameObject;
                    highlightMaterialMapping.Add(childObjects[i + j], childObjects[i + j].GetComponent<Renderer>().material);
                    Texture baseTexture = childObjects[i + j].GetComponent<Renderer>().material.mainTexture;
                    childObjects[i + j].GetComponent<Renderer>().material = highlightMat;
                    childObjects[i + j].GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
                    j++;
                }
            }
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

            if(!isDissolved)
                ReverseDissolve();

            if (isDissolved)
            {


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
                            Texture baseTexture = highlightMaterialMapping[go].mainTexture;
                            go.GetComponent<Renderer>().material = highlightMat;
                            go.GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
                            go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 0f, 1f, 0.5f));
                        }
                        else
                        {
                            uiInstance.SetActive(false);
                            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            go.GetComponent<Renderer>().material = highlightMaterialMapping[go];
                        }
                    }
                }
            }
        }
        else {
            
                foreach (GameObject go in childObjects)
                {
                    if (canPlace && canAfford && isDissolved)
                    {
                        go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
                    }
                    else if (isDissolved)
                    {
                        go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));
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
                temp.GetComponent<DefenceObject>().setIsPlaced(true);
                DestroyObject();
            }
        }
    }

    void ReverseDissolve()
    {
        foreach(GameObject go in childObjects)
        {
            Texture baseTexture = highlightMaterialMapping[go].mainTexture;
            go.GetComponent<Renderer>().material = dissolveMat;
            go.GetComponent<Renderer>().material.SetTexture("BaseTexture", baseTexture);
        }

        float time = 10f;
        while(time > 0)
        {
            time -= Time.deltaTime;
            foreach(GameObject go in childObjects)
            {
                go.GetComponent<Renderer>().material.SetFloat("Dissolve", time);
            }
        }
        isDissolved = true;
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
}