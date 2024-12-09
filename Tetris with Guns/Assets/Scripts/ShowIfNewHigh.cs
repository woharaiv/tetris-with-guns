using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowIfNewHigh : MonoBehaviour
{
    static bool isNewHighScore;
    bool updatedForNewHigh = false;
    [SerializeField] bool checkEveryFrame;
    private void Start()
    {
        isNewHighScore = (ScoreManager.lastScore >= ScoreManager.highScore);
        GetComponent<TextMeshProUGUI>().enabled = isNewHighScore;
    }
    void Update()
    {
        if (!checkEveryFrame)
            return;

        isNewHighScore = (ScoreManager.lastScore >= ScoreManager.highScore);
        if(isNewHighScore && !updatedForNewHigh)
        {
            GetComponent<TextMeshProUGUI>().enabled = true;
            updatedForNewHigh = true;
        }
    }
}
