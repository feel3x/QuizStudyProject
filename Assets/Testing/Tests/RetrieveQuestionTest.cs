using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class RetrieveQuestionTest
{
    public GameManager gameManager;

    private bool testInitialized = false;

    //Set Up the Testing Environment
    [OneTimeSetUp]
    public void SetUp()
    {
        testInitialized = false;
    }

    [UnitySetUp]
    public IEnumerator UnitySetup()
    {
        if (!testInitialized)
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            while (GameManager.Instance == null)
            {
                yield return null;
            }
            getReferences();
            testInitialized = true;
        }
    }

    //Testing Existing Questions
    [Test]
    public void CheckQuestionCategoryAndDifficultyExistingQuestions()
    {
        Debug.Log("Start Testing Retrieval of existing Questions");

        List<string> addedCategories = new List<string>();
        List<string> addedDifficulties = new List<string>();

        //Get all offline categories
        foreach (KeyValuePair<int, Question> q in gameManager.questionDatabase.allQuestions)
        {
            if (q.Value.correctlyAnswered)
                continue;

            if (!addedCategories.Contains(q.Value.category))
            {
                addedCategories.Add(q.Value.category);
            }
        }

        //Get all offline Difficulties
        foreach (string diff in gameManager.questionDatabase.availableDifficulties)
        {
            addedDifficulties.Add(diff);
        }

        for (int i = 0; i < 10; i++)
        {
            string searchingCategory = addedCategories[UnityEngine.Random.Range(0, addedCategories.Count)];
            string searchingDifficulty = addedDifficulties[UnityEngine.Random.Range(0, addedDifficulties.Count)];

            Debug.Log("Testing to find Question for category \"" + searchingCategory + "\" and difficulty \"" + searchingDifficulty + "\"");

            //Actual testing of function
            Question newQuestion = gameManager.GetRandomUnansweredQuestion(searchingCategory, searchingDifficulty);

            Assert.IsTrue(newQuestion.category == searchingCategory);
            Debug.Log("Question is of category \"" + searchingCategory + "\"");
            Assert.IsTrue(newQuestion.difficulty == searchingDifficulty);
            Debug.Log("Question is of difficulty \"" + searchingDifficulty + "\"");
            Assert.IsTrue(!newQuestion.correctlyAnswered);
            Debug.Log("Question has not been correctly answered, yet.");
        }
    }

    //Testing Non Existing Questions
    [Test]
    public void CheckQuestionCategoryAndDifficultyNonExistingQuestions()
    {
        Debug.Log("Start Testing Retrieval of non existing Questions");

        for (int i = 0; i < 1; i++)
        {
            Debug.Log("Testing to find Question for category \"NONE\" and difficulty \"NONE\"");

            //Actual testing of function
            Assert.Throws<Exception>(() => gameManager.GetRandomUnansweredQuestion("NONE", "NONE"));
            Debug.Log("No Questions Found. Exception raised.");
        }
    }

    //Testing Unavailable Questions
    [Test]
    public void CheckQuestionCategoryAndDifficultyUnavailableQuestions()
    {
        Debug.Log("Start Testing Retrieval of unavailable Questions");

        List<string> addedCategories = new List<string>();
        List<string> addedDifficulties = new List<string>();

        //Get all offline categories
        foreach (KeyValuePair<int, Question> q in gameManager.questionDatabase.allQuestions)
        {
            if (q.Value.correctlyAnswered)
                continue;

            if (!addedCategories.Contains(q.Value.category))
            {
                addedCategories.Add(q.Value.category);
            }
        }

        //Get all offline Difficulties
        foreach (string diff in gameManager.questionDatabase.availableDifficulties)
        {
            addedDifficulties.Add(diff);
        }

        for (int i = 0; i < 10; i++)
        {
            string searchingCategory = addedCategories[UnityEngine.Random.Range(0, addedCategories.Count)];
            string searchingDifficulty = addedDifficulties[UnityEngine.Random.Range(0, addedDifficulties.Count)];

            //Turning every question in this category difficulty pair to "already answered"
            foreach (int qID in gameManager.questionDatabase.questionIdsByCategoryAndDifficulty[searchingCategory][searchingDifficulty])
            {
                gameManager.questionDatabase.allQuestions[qID].correctlyAnswered = true;
            }

            Debug.Log("Testing to find UNAVAILABLE Question for category \"" + searchingCategory + "\" and difficulty \"" + searchingDifficulty + "\"");

            //Actual testing of function
            //Actual testing of function
            Assert.Throws<Exception>(() => gameManager.GetRandomUnansweredQuestion(searchingCategory, searchingDifficulty));
            Debug.Log("No Questions Found. Exception raised.");
        }
    }

    // Clean Up Environment after the Tests
    [TearDown]
    public void TearDown()
    {
      
    }

    private void getReferences()
    {
        gameManager = GameManager.Instance;
    }

}
