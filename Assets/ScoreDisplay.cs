using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour {

    public Text textComponent;
    public static int score;
    public ParticleSystem firework;
    public LayerMask relevantLayer;

    private bool newHighscore = false;

    public static readonly string first = "1st";
    private Camera camera1;

    private void Update() {
        textComponent.text = score.ToString();
    }

    private void FixedUpdate() {
        writeScore();
    }

    private void Start() {
        camera1 = Camera.main;
        var lastScore = PlayerPrefs.GetInt(first, 0);
        Debug.Log("last highscore: " + lastScore);
    }

    private void writeScore() {
        var lastScore = PlayerPrefs.GetInt(first, 0);
        if (score > lastScore) {
            PlayerPrefs.SetInt(first, score);
            if (!newHighscore) {
                newHighscore = true;
                firework.gameObject.SetActive(true);

                var transform1 = camera1.transform;
                Physics.Raycast(transform1.position, transform1.forward, out var hit, 1000f,
                    relevantLayer);

                firework.transform.position = hit.point;
                
                firework.Clear(true);
                firework.Play(true);
            }
        }
    }
}
