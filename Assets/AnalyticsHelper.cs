using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class AnalyticsHelper : MonoBehaviour {

    //@Timo:
    static int fireTowersPlaced;
    static int cannonTowersPlaced;
    static int archersPlaced;
    static int wallsPlaced;
    public static int upgradesMade;

    private static int lastHighscore;
    
    private void Start() {
        lastHighscore = PlayerPrefs.GetInt(ScoreDisplay.first, 0);
    }

    public static void gameOverEvent() {
        var score = ScoreDisplay.score;
        var totalTime = Time.timeSinceLevelLoad;

        AnalyticsEvent.GameOver(SceneManager.GetActiveScene().name, new Dictionary<string, object> {
            {""+AnalyticsSessionInfo.sessionId, new Dictionary<string, object>{
                {"score", score},
                {"lastScore", lastHighscore},
                {"fireTowers", fireTowersPlaced},
                {"cannonTowers", cannonTowersPlaced},
                {"archersTowers", archersPlaced},
                {"walls", wallsPlaced},
                {"upgrades", upgradesMade},
                {"totalTime", totalTime},
                {"fps", Time.renderedFrameCount / totalTime}
        }   }   });

        Analytics.FlushEvents();
    }

    public static void towerPlaced(string towerType, float distanceToBase, DefenceObject tower)
    {
        switch (towerType)
        {
            case "Cannon":
                cannonTowersPlaced++;
                break;
            case "Fire":
                fireTowersPlaced++;
                break;
            case "Wall":
                wallsPlaced++;
                break;
            case "Archer":
                archersPlaced++;
                break;
            default:
                break;
        }
        
        AnalyticsEvent.Custom("towerPlaced", new Dictionary<string, object> {
            {""+AnalyticsSessionInfo.sessionId, new Dictionary<string, object> {
                {tower.getGUID().ToString(), new Dictionary<string, object>{
                    {"towerType", towerType},
                    {"distanceToBase", distanceToBase }
        }   }   }}});
    }

    public static void towerUpgraded(string towerType, int toLevel, DefenceObject tower)
    {
        upgradesMade++;
        AnalyticsEvent.Custom("towerUpgraded", new Dictionary<string, object> {
            {""+AnalyticsSessionInfo.sessionId, new Dictionary<string, object> {
                {tower.getGUID().ToString(), new Dictionary<string, object>{
                    {"towerType", towerType},
                    {"toLevel", toLevel }
                }   
                }   
            }
            }
        });
    }

    public static void introSkipped(float skippedAt) {
        AnalyticsEvent.CutsceneSkip(SceneManager.GetActiveScene().name, new Dictionary<string, object>() {
            {""+AnalyticsSessionInfo.sessionId, new Dictionary<string, object>{
                {"skippedAt", skippedAt}
            }   
            }   
        });
    }

    public static void newHighscore() {
        var score = ScoreDisplay.score;
        var totalTime = Time.timeSinceLevelLoad;
        
        AnalyticsEvent.Custom("highscoreNew", new Dictionary<string, object> {
            {""+AnalyticsSessionInfo.sessionId, new Dictionary<string, object>{
            {"score", score},
            {"lastScore", lastHighscore},
            {"fireTowers", fireTowersPlaced},
            {"cannonTowers", cannonTowersPlaced},
            {"archersTowers", archersPlaced},
            {"walls", wallsPlaced},
            {"upgrades", upgradesMade},
            {"totalTime", totalTime},
            {"fps", Time.renderedFrameCount / totalTime}
        } } });
    }
}