using UnityEngine;
using System;
using TMPro;

public class GameClock : MonoBehaviour
{

    private GameManager gameManager;

    private TMP_Text clockText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("Scripts").GetComponent<GameManager>();   
        clockText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.realtimeSinceStartup - gameManager.startTime);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        clockText.text = timeText;
    }
}
