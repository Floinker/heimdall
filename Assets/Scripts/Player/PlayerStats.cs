using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Main PlayerStats")]
    public string playerName = "TestName";
    public int playerHealth = 100;
    public float playerCoins = 0;

    private static PlayerStats instance;

    private void Start() {
        instance = this;
    }

    public static PlayerStats getInstance() {
        return instance;
    }
}
