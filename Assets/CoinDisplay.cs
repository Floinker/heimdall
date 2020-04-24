using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinDisplay : MonoBehaviour
{
    public PlayerStats stats;

    private Text coinCount;
    private void Start()
    {
        coinCount = transform.GetChild(0).GetComponent<Text>();
    }
    // Update is called once per frame
    void Update() {
        var text = (int) stats.playerCoins;
        coinCount.text = "" + text;
    }
}
