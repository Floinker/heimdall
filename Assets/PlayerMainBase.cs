using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMainBase : MonoBehaviour, IDamagable {

    public static Vector3 basePosition;
    
    [Header("Static Defence-Object-Settings")]
    public float health = 1000f;

    private void Start() {
        basePosition = this.transform.position;
    }

    private void FixedUpdate() {
        if (health <= 0) {
            onDeath();
            Destroy(gameObject);
        }
    }

    void IDamagable.TakeDamage(float amount) {
        health -= amount;
    }

    public virtual void onDeath() {
        AnalyticsHelper.gameOverEvent();
        GameOverScreen.score = ScoreDisplay.score;
        SceneManager.LoadScene("GameOver");
        //to be implemented
    }
    
}