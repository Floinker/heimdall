using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

    public RectTransform greenBar;
    public RectTransform redBar;

    public float maxWidth = 600;

    // in %
    public void setProgress(float val) {
        var size = maxWidth * val;
        greenBar.sizeDelta = new Vector2(size, greenBar.sizeDelta.y);
    }
}
