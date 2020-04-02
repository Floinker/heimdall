using System;
 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
using UnityEngine.UI;

public class Highscoredisplay : MonoBehaviour {

     private int lastScore;
     private Text text;
     
     private void Start() {
         //display score
         text = this.GetComponent<Text>();
         lastScore = PlayerPrefs.GetInt(ScoreDisplay.first, 0);
         text.text = "Highscore: " + lastScore;
     }
 
     private void FixedUpdate() {
         var score = ScoreDisplay.score;
         if (score > lastScore) {
             text.text = "New Highscore!";
         }
     }
 }