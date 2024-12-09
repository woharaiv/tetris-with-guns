using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public static int lastScore;
    public static int highScore;
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public static void AddScore(int points)
    {
        lastScore += points;
        if (lastScore > highScore)
            highScore = lastScore;
    }
}
