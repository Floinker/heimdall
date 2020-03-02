using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceObject : MonoBehaviour
{
    private Material defenceMat;
    private GameObject[] childObjects;

    private bool isPlaced = false;
    private bool canPlace = false;
    // Start is called before the first frame update
    void Start()
    {
        childObjects = new GameObject[transform.childCount];
        int i = 0;

        foreach(Transform child in transform)
        {
            childObjects[i] = child.gameObject;
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaced)
        {
            foreach (GameObject go in childObjects)
            {
                go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 0f, 0f, 1f));
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        //TODO Check auf Layer
        canPlace = false;
        Debug.Log("enter");
        if (!isPlaced)
        {
            foreach (GameObject go in childObjects)
            {
                go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(1f, 0f, 0f, 0.5f));
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        canPlace = true;
        if (!isPlaced)
        {
            foreach (GameObject go in childObjects)
            {
                go.GetComponent<Renderer>().material.SetColor("Color_Highlight", new Color(0f, 1f, 0f, 0.5f));
            }
        } 
    }

    public void setIsPlaced(bool isPlaced)
    {
        this.isPlaced = isPlaced;
    }

    public bool CanPlace()
    {
        return canPlace;
    }
}
