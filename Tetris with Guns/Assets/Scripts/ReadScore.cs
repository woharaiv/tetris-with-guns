using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ReadScore : MonoBehaviour
{
    enum ScoreToRead
    {
        Last,
        High
    }
    [SerializeField] ScoreToRead scoreToRead;
    TextMeshProUGUI text;
    int score 
    { 
        get 
        { 
            switch(scoreToRead)
            {
                case ScoreToRead.High:
                    return ScoreManager.highScore;
                default:
                    return ScoreManager.lastScore;
            }
                
        }
    }
    int cachedScore = -1;
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if(score != cachedScore)
        {
            cachedScore = score;
            text.text = cachedScore.ToString();
        }
    }
}
