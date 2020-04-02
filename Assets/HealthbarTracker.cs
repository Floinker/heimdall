using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarTracker : MonoBehaviour {

    public PlayerMainBase mainBase;
    private float maxHealth;
    public HealthBar bar;

    private void Start() {
        maxHealth = mainBase.health;
    }

    private void FixedUpdate() {
        var hp = mainBase.health;
        var hpPercent = hp / maxHealth;
        bar.setProgress(hpPercent);
    }
}
