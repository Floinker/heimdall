using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : StaticObject {

    public float GoldPerSecond;
    
    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (!IsPlaced()) return;
        var generated = GoldPerSecond * Time.deltaTime;
        PlayerStats.getInstance().playerCoins += generated;
    }
}
