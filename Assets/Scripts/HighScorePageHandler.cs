using UnityEngine;
using TMPro;
using System;

public class HighScorePageHandler : MonoBehaviour
{

    public GameObject highscoreObject;
    public TMP_Text answeredText;
    public TMP_Text timeText;

    public GameManager gameManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        gameManager.gameEnded += fillText;
        gameManager.highScoreAchieved += showHighscore;
    }

    private void OnDisable()
    {
        gameManager.gameEnded -= fillText;
        gameManager.highScoreAchieved -= showHighscore;
    }
    private void fillText()
    {
        highscoreObject.SetActive(false);
        answeredText.text = gameManager.correctlyAnsweredQuestionCount.ToString() + " correct answers.";
        TimeSpan timeSpan = TimeSpan.FromSeconds(gameManager.totalGameTime);
        string totalTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
       
        timeText.text = totalTime.ToString();
    }

    private void showHighscore()
    {
        highscoreObject.SetActive(true);    
    }
}
