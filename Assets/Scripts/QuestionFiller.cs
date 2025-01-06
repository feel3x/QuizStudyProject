
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionFiller : MonoBehaviour
{

    public GameObject questionView1;
    public GameObject questionView2;

    public GameObject answerPrefab;
    public GameObject nextButtonPrefab;

    private int currentlyFilled = 0;

    public GameManager gameManager;
    public ViewChanger viewChanger;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnEnable()
    {
        gameManager.nextQuestion += fillQuestionView;
        gameManager.gameStarted += resetFiller;
    }
    private void OnDisable()
    {
        gameManager.nextQuestion -= fillQuestionView;
        gameManager.gameStarted -= resetFiller;

    }

    public void resetFiller()
    {
        //currentlyFilled = 0;
    }

    public void fillQuestionView(Question q, int questionNumber)
    {
        if (currentlyFilled == 1)
        {
            //Prepare Questionview 2
            prepareQuestionView(questionView2, q, questionNumber);
            currentlyFilled = 2;
            viewChanger.switchToView("QuizScreen2");
        }
        else
        {
            //Prepare Questionview 1
            prepareQuestionView(questionView1, q, questionNumber);
            currentlyFilled = 1;
            viewChanger.switchToView("QuizScreen1");
        }
    }

    private void prepareQuestionView(GameObject questionView, Question q, int questionNumber)
    {
        //Fill the Number of the Question the game is currently on
        TMP_Text questionNumberText = questionView.transform.Find("Header").Find("QuestionNumber").GetComponent<TMP_Text>();
        questionNumberText.text = "Question " + questionNumber;

        //Fill the question Category indicator
        TMP_Text questionCategoryText = questionView.transform.Find("Header").Find("TopicIndicator").GetComponent<TMP_Text>();
        questionCategoryText.text = System.Web.HttpUtility.HtmlDecode(q.category);

        //Fill the question Text
        TMP_Text questionText = questionView.transform.Find("Question").Find("QuestionText").GetComponent<TMP_Text>();
        questionText.text = System.Web.HttpUtility.HtmlDecode(q.question);

        //Create answer buttons
        Transform answerParent = questionView.transform.Find("Answers").transform;


        //Keep track if the correct answer has already been spawned.
        bool correctAnswerSpawned = false;

        //Clear out previously spawned elements
        foreach (Transform go in answerParent.GetComponentsInChildren<Transform>())
        {
            if (go == answerParent)
                continue;
            Destroy(go.gameObject);
        }

        //Keeps track of how many answers have been spawned. 
        int limitCounter = 0;

        //Iterating through the incorrect answers of the Question q 
        foreach (string answer in q.incorrect_answers)
        {
            //In case of a faulty dataset. Limiting the overall maximum amount of possibly spawned answers to 4. 3 incorrect, and 1 correct one.
            if (limitCounter > 3)
            {
                break;
            }

            //Give a random chance of spawning the correct answer between any on the incorrect ones. The probability is 1/4 for 4 multiple choice answers or 1/2 for true/false questions.
            if (!correctAnswerSpawned && Random.value <= 1f / (q.incorrect_answers.Length + 1))
            {
                //Spawn correct answer
                SpawnAnswer(q.correct_answer, answerParent);
                limitCounter++;
                correctAnswerSpawned = true;
            }

            //Spawn an incorrect answer.
            SpawnAnswer(answer, answerParent);
            limitCounter++;
        }

        //If the correct answer has not yet been spawned randomly, then add it now
        if (!correctAnswerSpawned)
        {
            //Spawn correct answer
            SpawnAnswer(q.correct_answer, answerParent);
        }

        //Spawn the next button
        Button nextButton = Instantiate(nextButtonPrefab, answerParent).GetComponent<Button>();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => gameManager.checkAnswer());

    }
    private GameObject SpawnAnswer(string answerText, Transform parent)
    {
        GameObject newAnswer = Instantiate(answerPrefab, parent);
        Button newAnswerButton = newAnswer.GetComponent<Button>();
        newAnswerButton.GetComponentInChildren<TMP_Text>().text = System.Web.HttpUtility.HtmlDecode(answerText);
        newAnswerButton.onClick.RemoveAllListeners();
        newAnswerButton.onClick.AddListener(() => gameManager.logAnswer(answerText));

        return newAnswer;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
