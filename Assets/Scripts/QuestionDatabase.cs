using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Unity.VisualScripting;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine.Rendering;

public class QuestionDatabase : MonoBehaviour
{
    //The subpath where the question files are saved
    public string savePath = "questions";

    //The minimum amount of questions that should be saved offline for every category and difficulty 
    public int minQuestionsPerCategoryAndDifficulty = 50;

    //New Questions are being retrieved when the amount of unanswered questions falls below this threshold
    public int onlineQuestionRetrievalThreshold = 2;

    //All downloaded Questions by their ID
    public Dictionary<int, Question> allQuestions = new Dictionary<int, Question>();

    public string[] availableDifficulties = new string[3] { "easy", "medium", "hard" };

    //All Ids of all downloaded questions sorted by category and difficulty
    public Dictionary<string, Dictionary<string, List<int>>> questionIdsByCategoryAndDifficulty = new Dictionary<string, Dictionary<string, List<int>>>();


    public delegate void databaseDelegate();
   public databaseDelegate updatedDatabase;


    //List of categories available on Open Trivia DB
    private List<Category> availableCategories = new List<Category>();  

    private List<int> exhaustedCategorieIds = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get the Open Trivia DB Token if there is none for this player
        if(!PlayerPrefs.HasKey("triviaToken"))
        {
          //  StartCoroutine(RetrieveTriviaToken());
        }
        UpdateQuestionDatabase();
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }


    //Coroutine to retrieve Trivia DB Token
    IEnumerator RetrieveTriviaToken()
    {
        WaitForSeconds waiter = new WaitForSeconds(5);
        while(Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield return waiter;
        }
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api_token.php?command=request"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    tokenResponse newTokenResponse = JsonUtility.FromJson<tokenResponse>(webRequest.downloadHandler.text);
                    if(newTokenResponse.response_message == "Token Generated Successfully!")
                    {
                        PlayerPrefs.SetString("triviaToken", newTokenResponse.token);
                        PlayerPrefs.Save();

                    }
                    break;
            }
        }
    }


    //Coroutine to retrive int amount of questions
    private bool connectionLocked = false;
    IEnumerator RetrieveQuestion(int amountNeeded, Category category, string difficulty)
    {
        //While the device is not connected to the internet, check every 5 seconds for a connection
        WaitForSeconds waiter = new WaitForSeconds(5);

        //Lock the connection that only one coroutine at a time can send a request.
        connectionLocked = true;
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield return waiter;
        }

        //The amount of questions in this category
        int categoryAmounts = 0;

        //Retrieve the amount of questions for the given category ID
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api_count.php?category=" + category.id))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    CategoryAmountResponse newQuestionAmountData = JsonUtility.FromJson<CategoryAmountResponse>(webRequest.downloadHandler.text);

                    //Save the amount of questions for the given difficulty in the variable.
                    if (difficulty == "easy")
                    {
                        categoryAmounts = newQuestionAmountData.category_question_count.total_easy_question_count;
                    }
                    else if (difficulty == "medium")
                    {
                        categoryAmounts = newQuestionAmountData.category_question_count.total_medium_question_count;
                    }
                    else
                    {
                        categoryAmounts = newQuestionAmountData.category_question_count.total_hard_question_count;
                    }
                    break;
            }

        }
        //Unlock the connection
        connectionLocked = false;

        //Check if OpenTriviaDB still has new questions for this category and difficulty combination
        if (questionIdsByCategoryAndDifficulty.ContainsKey(category.name) && questionIdsByCategoryAndDifficulty[category.name].ContainsKey(difficulty) && categoryAmounts <= questionIdsByCategoryAndDifficulty[category.name][difficulty].Count)
        {
            //Category is exhausted
            CategoryExhausted(category.id);
            yield break;
        }

        //Lock the connection once again
        connectionLocked = true;

        //Retrieve all the questions OpenTrivaDB has to offer for this category-difficulty combination
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api.php?amount=" + categoryAmounts + "&category=" + category.id + "&difficulty=" + difficulty))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            //Unlock Connection
            connectionLocked = false;
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    QuestionData newQuestionData = JsonUtility.FromJson<QuestionData>(webRequest.downloadHandler.text);
                    if (newQuestionData.response_code == 0)
                    {
                        //Successful retrieval of questions

                        //How many new questions have been added to files
                        int questionsAdded = 0;

                        //Check each question if it already exists in our files
                        foreach (Question q in newQuestionData.results)
                        {
                            if (CheckQuestionExistance(q))
                            {
                                //Question exists -> Move on
                                continue;
                            }

                            //Save new question to file
                            SaveQuestionToFile(q);

                            //Stop the loop when we have saved enough questions
                            if (questionsAdded > amountNeeded)
                            {
                                yield break;
                            }

                            //Increase added questions
                            questionsAdded++;
                        }

                        //No more questions available if this is reached
                        CategoryExhausted(category.id);
                    }
                    else
                    {
                        //API Error
                        Debug.LogError(": API Error: " + webRequest.downloadHandler.text);
                    }
                    break;
            }
        }

        //Unlock the connection
        connectionLocked = false;
    }

    //Update the Dictionaries
    public void UpdateQuestionDatabase()
    {
        LoadQuestionsFromFiles();
       StartCoroutine(CheckForSufficientQuestions());
        updatedDatabase?.Invoke();
    }


    //Saves category because there are no more questions for it online
    private void CategoryExhausted(int catId)
    {
        Debug.Log("Category Exhausted");
        if (!exhaustedCategorieIds.Contains(catId))
        {
            exhaustedCategorieIds.Add(catId);
        }
    }


    //Checks if question already exists in our database
    private bool CheckQuestionExistance(Question question)
    {
        foreach (KeyValuePair<int, Question> q in allQuestions)
        {
            if(q.Value.question == question.question && q.Value.category == question.category && q.Value.difficulty == question.difficulty)
            {
                Debug.Log(q.Value.id + " exists");
                return true;    
            }
        }
        return false;
    }

    //Check if there are enough questions in each category and difficulty, and start question retrieval if needed
    private IEnumerator CheckForSufficientQuestions()
    {

        //Wait if there is no internet connection
        WaitForSeconds waiter = new WaitForSeconds(5);
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield return waiter;
        }

        if (availableCategories.Count == 0)
        {

            //Check available cateogries on OpenTriviaDB
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api_category.php"))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(": Error: " + webRequest.error);
                        yield break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(": HTTP Error: " + webRequest.error);
                        yield break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                        CategoryResponse newCategoryResponse = JsonUtility.FromJson<CategoryResponse>(webRequest.downloadHandler.text);
                        availableCategories.Clear();
                        foreach (Category cat in newCategoryResponse.trivia_categories)
                        {
                            availableCategories.Add(cat);
                        }
                        break;
                }
            }
        }

        //Count all offline questions of each category and difficulty
        Dictionary<string, int> counterDict = new Dictionary<string, int>();
        foreach (Category cat in availableCategories)
        {
            if (exhaustedCategorieIds.Contains(cat.id))
                continue;
            foreach(string difficulty in availableDifficulties)
            {
                int countedQuestions = 0;
                foreach (KeyValuePair<int, Question> q in allQuestions)
                {
                    //dont count questions which have been answered already, or are of different category or difficulty
                    
                    if (System.Web.HttpUtility.HtmlDecode(q.Value.category) != cat.name || q.Value.difficulty != difficulty || q.Value.correctlyAnswered)
                    {
                        continue;
                    }
                    countedQuestions++;
                }
                Debug.Log(cat.name + " - " + difficulty + " - " + countedQuestions);
                //check if counted sufficient questions
                if(countedQuestions < onlineQuestionRetrievalThreshold)
                {
                    //retrieve more questions
                    StartCoroutine(RetrieveQuestion(minQuestionsPerCategoryAndDifficulty - countedQuestions, cat, difficulty));

                    while (connectionLocked)
                    {
                        yield return null;  
                    }
  
                    //Wait some time to give not overwhelm the server
                    yield return new WaitForSeconds(6);
                }
            }
        }
        LoadQuestionsFromFiles();

    }

    //Load all the questions into the dicitonaries from files
    private void LoadQuestionsFromFiles()
    {
       
        //Clear all Dictionaries
        allQuestions.Clear();
        questionIdsByCategoryAndDifficulty.Clear();

        //Retrieve Files
        string path = Path.Combine(Application.persistentDataPath, savePath);

        if (!Directory.Exists(path))
        {

            Debug.Log("installing new files");
            //Install Initial Offline Qustion Files
            UnityEngine.Object[] questionFiles = Resources.LoadAll<TextAsset>("questions");
            foreach (var f in questionFiles)
            {
                TextAsset newText = (TextAsset)f;
                string destination = Path.Combine(Application.persistentDataPath, savePath, f.name+".txt");

                //Save question File
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
                if (!Directory.Exists(Path.GetDirectoryName(destination)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                }
                File.WriteAllText(destination, newText.text);
            }


           
        }
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            //convert JsonText to question object
           string jsonString = File.ReadAllText(file.FullName);
            Question newQuestion = JsonUtility.FromJson<Question>(jsonString);  

            //Fill Dictionaries with question data
            allQuestions.Add(newQuestion.id, newQuestion);
            
            //Check if this category is not yet in the dict
            if (!questionIdsByCategoryAndDifficulty.ContainsKey(newQuestion.category))
            {
               
                Dictionary<string, List<int>> newDifficultyDictionary = new Dictionary<string, List<int>>();
                List<int> newIdList = new List<int>();
                newIdList.Add(newQuestion.id);
                newDifficultyDictionary.Add(newQuestion.difficulty, newIdList);
                questionIdsByCategoryAndDifficulty.Add(newQuestion.category, newDifficultyDictionary);
            }
            else
            {
                //check if this category already has a list of this difficulty
                if (!questionIdsByCategoryAndDifficulty[newQuestion.category].ContainsKey(newQuestion.difficulty))
                {
                    List<int> newIdList = new List<int>();
                    newIdList.Add(newQuestion.id);
                    questionIdsByCategoryAndDifficulty[newQuestion.category].Add(newQuestion.difficulty, newIdList);
                }
                else
                {
                   
                    questionIdsByCategoryAndDifficulty[newQuestion.category][newQuestion.difficulty].Add(newQuestion.id);
                }
            }
        }
        updatedDatabase?.Invoke();
    }


    public void SaveQuestionToFile(Question q, bool  newFile = true)
    {
        int questionId = q.id;
        if (newFile)
        {
            //Create a counter for QuestionIds
            if (!PlayerPrefs.HasKey("questionIdCounter"))
            {
                PlayerPrefs.SetInt("questionIdCounter", 2351);
                PlayerPrefs.Save();
            }
            questionId = PlayerPrefs.GetInt("questionIdCounter");
            q.id = questionId;
            q.amountAsked = 0;
            q.correctlyAnswered = false;
            q.timeToAnswer = "";
        }
        //Convert question Object to json
        string jsonString = JsonUtility.ToJson(q);
        string destination = Path.Combine( Application.persistentDataPath, savePath, questionId.ToString()+".txt");
       
        //Save question File
        if (File.Exists(destination))
        {
            File.Delete(destination);
        }
        if (!Directory.Exists(Path.GetDirectoryName(destination))){
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
        }
        File.WriteAllText(destination, jsonString );
        Debug.Log("Question with id "+ questionId + " saved.");
        if (newFile)
        {
            PlayerPrefs.SetInt("questionIdCounter", questionId + 1);
            PlayerPrefs.Save();
        }
       // UpdateQuestionDatabase();
    }

}

[System.Serializable]
public class QuestionData
{
    public int response_code;
    public Question[] results;
}

[System.Serializable]
public class Question
{
    public int id;
    public string type;
    public string difficulty;
    public string category;
    public string question;
    public string correct_answer;
    public string[] incorrect_answers;

    public int amountAsked;
    public bool correctlyAnswered;
    public string timeToAnswer;
}

[System.Serializable]
public struct CategoryResponse
{
    public Category[] trivia_categories;
}

[System.Serializable]
public struct Category
{
    public int id;
    public string name;
}

[System.Serializable]
public struct tokenResponse
{
    public string response_code;
    public string response_message;
    public string token;
}
[System.Serializable]
public struct CategoryAmountResponse
{
    public int category_id;
    public CategoryAmounts category_question_count;
}
[System.Serializable]
public struct CategoryAmounts
{
    public int total_question_count;
    public int total_easy_question_count;
    public int total_medium_question_count;
    public int total_hard_question_count;
}