using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HighScoreHandler : MonoBehaviour
{
    public string savePath = "highscores";
    public List<HighScore> highScoreList = new List<HighScore>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Load Highscores 
        LoadHighScores();
    }


    public bool CheckForHighscore(string category, string difficulty, int score, float time)
    {
        if(score < 1)
            return false;

        foreach (HighScore highScore in highScoreList)
        {
            if (highScore.categoryName == category && highScore.difficulty == difficulty)
            {
                if (highScore.score < score)
                {
                    //new HighScore
                    highScore.score = score;
                    highScore.time = time;
                    SaveHighScores();
                    return true;
                }
                else if (highScore.score == score && highScore.time > time)
                {
                    //new Highscore time
                    highScore.time = time;
                    SaveHighScores();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        //No Highscore saved yet
        highScoreList.Add(new HighScore(category, difficulty, time, score));
        SaveHighScores();
        return true;
    }

    private void LoadHighScores()
    {
        highScoreList.Clear();
        //Retrieve Files
        string path = Path.Combine(Application.persistentDataPath, savePath);

        if (!Directory.Exists(path))
        {

            //Install Initial Offlien Qustion Files

            return;
        }
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            //convert JsonText to question object
            string jsonString = File.ReadAllText(file.FullName);
            HighScore newScore = JsonUtility.FromJson<HighScore>(jsonString);

            highScoreList.Add(newScore);
        }
    }

    private void SaveHighScores()
    {

        foreach (HighScore s in highScoreList)
        {
            string fileName = s.categoryName + "-" + s.difficulty;
            fileName = String.Concat(fileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");
            string jsonString = JsonUtility.ToJson(s);
            string destination = Path.Combine(Application.persistentDataPath, savePath, fileName+".score");

            //Save question File
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            if (!Directory.Exists(Path.GetDirectoryName(destination)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
            }
            File.WriteAllText(destination, jsonString);
        }
       
       
        LoadHighScores();
    }

    public class HighScore
    {
        public string categoryName;
        public string difficulty;
        public float time;
        public int score;

        public HighScore(string categoryName, string difficulty, float time, int score)
        {
            this.categoryName = categoryName;
            this.difficulty = difficulty;
            this.time = time;
            this.score = score;
        }
    }
}
