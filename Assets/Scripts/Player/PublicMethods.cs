using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerUpgrade
{
    public Upgrade upgrade;
    public int cost;

    public PlayerUpgrade(Upgrade upgrade, int cost)
    {
        this.upgrade = upgrade;
        this.cost = cost;
    }
}
