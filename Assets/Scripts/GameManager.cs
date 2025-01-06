using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MenuHandler menuHandler;
    public QuestionDatabase questionDatabase;
    public ViewChanger viewChanger;
    public MessageHandler messageHandler;
    public HighScoreHandler highScoreHandler;   

    public delegate void delegateWithQuestionData(Question q, int questionNumber);
    public delegateWithQuestionData nextQuestion;
    public delegateWithQuestionData correctlyAnswered;
    public delegateWithQuestionData incorrectlyAnswered;
    public delegateWithQuestionData noAnswerProvided;

    public delegate void emptyDelegate();
    public emptyDelegate gameStarted;
    public emptyDelegate gameEnded;
    public emptyDelegate highScoreAchieved;


    public Question currentQuestion;
    public string currentAnswer;

    public float startTime;

    public int questionNumber = 1;
    public bool gameHasStarted = false;

    public int startLives = 3;

    public int currentLives = 3;

    public int correctlyAnsweredQuestionCount = 0;
    public float totalGameTime = 0;

    public static GameManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void startGame()
    {
        currentLives = startLives;
        questionNumber = 0;
        correctlyAnsweredQuestionCount = 0;
        startNextQuestion();
        if (currentQuestion != null)
        {
          

            gameStarted?.Invoke();

            gameHasStarted = true;

            startTime = Time.realtimeSinceStartup;

           
        }

    }


    public void startNextQuestion()
    {
        if (currentLives <= 0)
        {
            GameOver();
            return;
        }


        questionNumber++;
        currentAnswer = "";
        currentQuestion = null;
        string category = System.Web.HttpUtility.HtmlEncode(menuHandler.categoryDropdown.options[menuHandler.categoryDropdown.value].text);
        string difficulty = menuHandler.difficultyDropdown.options[menuHandler.difficultyDropdown.value].text;

        Question newQuestion = null;
       
        try
        {
            newQuestion = GetRandomUnansweredQuestion(category, difficulty);
        }
        catch
        {
            //No more questions for this category and difficulty combination

            Debug.Log("No more questions");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                messageHandler.showMessage("No more Questions", "There are no more questions available for this category and difficulty combination. Try connecting to the internet to retrieve more questions.", null);
            }
            else
            {
                messageHandler.showMessage("No more Questions", "There are no more questions available for this category and difficulty combination.", null);
            }
            endGame();
            return;
        }

        if (newQuestion != null)
        {
            nextQuestion?.Invoke(newQuestion, questionNumber);
            currentQuestion=  newQuestion;
        }
    }

    public void checkAnswer()
    {
        //Check if the user has selected an answer
       if(currentAnswer == "")
        {
            //No answer was selected.
            messageHandler.showMessage("No Answer?", "Please select an answer.", null);
            noAnswerProvided?.Invoke(currentQuestion, questionNumber);
            return;
        }
       //Check for correctness
        if (currentQuestion.correct_answer == currentAnswer)
        {
            //The question was answered correctly
            correctlyAnswered?.Invoke(currentQuestion, questionNumber);
            correctlyAnsweredQuestionCount++;
            questionDatabase.allQuestions[currentQuestion.id].correctlyAnswered = true;
            questionDatabase.SaveQuestionToFile(questionDatabase.allQuestions[currentQuestion.id], false);
            startNextQuestion();
        }
        else
        {
            //The question was answered incorrectly
            currentLives--;
            incorrectlyAnswered?.Invoke(currentQuestion, questionNumber);
            messageHandler.showMessage("Incorrect!", "Correct Answer: " + System.Web.HttpUtility.HtmlDecode(currentQuestion.correct_answer), () => startNextQuestion());
        }
    }

    public void endGame()
    {
        totalGameTime = Time.realtimeSinceStartup - startTime;
        gameEnded?.Invoke();
        viewChanger.switchToView("GameOver");
        if (currentQuestion != null)
        {
            if (highScoreHandler.CheckForHighscore(currentQuestion.category, currentQuestion.difficulty, correctlyAnsweredQuestionCount, totalGameTime))
                HighScore();
        }
    }

    private void HighScore()
    {
        highScoreAchieved?.Invoke();
    }

    public void GameOver()
    {
        endGame();
    }

    public void logAnswer(string answer)
    {
        currentAnswer = answer;
    }
    public Question GetRandomUnansweredQuestion(string category, string difficulty)
    {

        if(!questionDatabase.questionIdsByCategoryAndDifficulty.ContainsKey(category))
            {
            throw new System.Exception("Category Non Existant");
        }
        if (!questionDatabase.questionIdsByCategoryAndDifficulty[category].ContainsKey(difficulty))
        {
            throw new System.Exception("Difficulty Non Existant");
        }

        //Get unanswered questions
        List<Question> unansweredQuestions = new List<Question>();
        foreach (int qIndex in questionDatabase.questionIdsByCategoryAndDifficulty[category][difficulty])
        {
           
            Question questionToCheck = questionDatabase.allQuestions[qIndex];
            if (questionToCheck.correctlyAnswered)
            {
                continue;
            }
            unansweredQuestions.Add(questionToCheck);
        }

        if (unansweredQuestions.Count <= 0)
        {
            //No more questions for this category and difficulty combination
            throw new System.Exception("No more questions");
        }

        //Get Random unanswered Question
       return unansweredQuestions[Random.Range(0, unansweredQuestions.Count)];
    }

    public Question GetRandomQuestion()
    {
        //Get Random  Question
        return questionDatabase.allQuestions[Random.Range(0, questionDatabase.allQuestions.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
