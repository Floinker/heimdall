﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUIHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(BaseEventData data)
    {
        Text text = GetComponentInChildren<Text>();
        text.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        text.transform.parent.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.39f);
    }

    public void OnPointerExit(BaseEventData data)
    {

        Text text = GetComponentInChildren<Text>();
        text.color = new Color(0, 0, 0, 0);
        text.transform.parent.gameObject.GetComponent<Image>().color = new Color(0,0,0,0); 
    }
}