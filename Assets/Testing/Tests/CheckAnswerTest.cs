using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CheckAnswerTest
{
    public GameManager gameManager;

    public bool correctAnswer;
    public bool incorrectAnswer;
    public bool noAnswer;

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
            addListeners();
            testInitialized = true;
        }
    }

    //Testing Correct Answers
    [Test]
    public void CheckAnswerTestCorrectAnswers()
    {

        Debug.Log("Testing Correct Answers");
        for (int i = 0; i < 10; i++)
        {
            Debug.Log("Starting Test #" + i);
            prepareTest();
            FillTest(0);

            //Test The Actual Function
            gameManager.checkAnswer();

            Assert.That(correctAnswer, Is.True);
        }
    }

    // Testing Incorrect Answers
    [Test]
    public void CheckAnswerTestIncorrectAnswers()
    {
        Debug.Log("Testing Incorrect Answers");
        for (int i = 0; i < 10; i++)
        {
            Debug.Log("Starting Test #" + i);
            prepareTest();
            FillTest(1);

            //Test The Actual Function
            gameManager.checkAnswer();

            Assert.That(incorrectAnswer, Is.True);
        }
    }

    // Testing No Answers
    [Test]
    public void CheckAnswerTestNoAnswers()
    {
        Debug.Log("Testing No Answers");
        for (int i = 0; i < 10; i++)
        {
            Debug.Log("Starting Test #" + i);
            prepareTest();
            FillTest();

            //Test The Actual Function
            gameManager.checkAnswer();

            Assert.That(noAnswer, Is.True);
        }
    }

    // Clean Up Environment after the Tests
    [TearDown]
    public void TearDown()
    {
        removeListeners();
    }

    private void prepareTest()
    {
        correctAnswer = false;
        incorrectAnswer = false;
        noAnswer = false;
    }
    private void getReferences()
    {
        gameManager = GameManager.Instance;
    }

    private void addListeners()
    {
        gameManager.correctlyAnswered += (Question q, int number) => { Debug.Log("Correct Answer to QuestionId " + q.id); correctAnswer = true; };
        gameManager.incorrectlyAnswered += (Question q, int number) => { Debug.Log("Incorrect Answer to QuestionId " + q.id); incorrectAnswer = true; };
        gameManager.noAnswerProvided += (Question q, int number) => { Debug.Log("No Answer to QuestionId " + q.id); noAnswer = true; };
    }
    private void removeListeners()
    {
        gameManager.correctlyAnswered -= (Question q, int number) => { correctAnswer = true; };
        gameManager.incorrectlyAnswered -= (Question q, int number) => { incorrectAnswer = true; };
        gameManager.noAnswerProvided -= (Question q, int number) => { noAnswer = true; };
    }

    private void FillTest(int type = -1)
    {
        gameManager.currentLives = 3;
        gameManager.currentQuestion = gameManager.GetRandomQuestion();

        switch (type)
        {
            case 0:
                gameManager.currentAnswer = gameManager.currentQuestion.correct_answer;
                break;
            case 1:
                gameManager.currentAnswer = gameManager.currentQuestion.incorrect_answers[Random.Range(0, gameManager.currentQuestion.incorrect_answers.Length)];
                break;
            default:
                gameManager.currentAnswer = "";
                break;
        }
    }
}
