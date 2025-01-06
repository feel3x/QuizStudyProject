using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class HighScoreFiller : MonoBehaviour
{

    public GameObject highScorePrefab;

    public HighScoreHandler highScoreHandler;

    public Transform scoresParent;

    private List<GameObject> createdObjects = new List<GameObject> ();

    private void OnEnable()
    {
        foreach (GameObject obj in createdObjects)
        {
            Destroy(obj);
        }
        createdObjects.Clear ();    
       foreach(HighScoreHandler.HighScore score in highScoreHandler.highScoreList)
        {
            GameObject newScore = Instantiate(highScorePrefab, scoresParent);

          TMP_Text categoryDifficulty =  newScore.transform.Find("CategoryDifficulty").GetComponent<TMP_Text>();
            TMP_Text answers = newScore.transform.Find("Answers").GetComponent<TMP_Text>();
            TMP_Text time = newScore.transform.Find("Time").GetComponent<TMP_Text>();

            categoryDifficulty.text = System.Web.HttpUtility.HtmlDecode(score.categoryName) + " - " + score.difficulty;
            answers.text = score.score + " correct answers.";
            TimeSpan timeSpan = TimeSpan.FromSeconds(score.time);
            string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            time.text = timeText;
            createdObjects.Add (newScore);
        }
    }
}
