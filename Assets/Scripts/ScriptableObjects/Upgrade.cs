using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Heimdall/Player/Create Upgrade")]
public class Upgrade : ScriptableObject
{
    public string description;
    public GameObject defencePrefab;
}
