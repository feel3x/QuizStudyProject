using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{

    public TMP_Dropdown difficultyDropdown;
    public TMP_Dropdown categoryDropdown;

    public QuestionDatabase questionDatabase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        questionDatabase.updatedDatabase += AdjustDropdowns;
    }
    private void OnDisable()
    {
        questionDatabase.updatedDatabase -= AdjustDropdowns;
    }


    private void AdjustDropdowns()
    {
        List<TMP_Dropdown.OptionData> categoryOptions = new List<TMP_Dropdown.OptionData>();
        List<string> addedOptions = new List<string>();
        List<TMP_Dropdown.OptionData> difficultyOptions = new List<TMP_Dropdown.OptionData>();


        //Get all offline categories
        foreach (KeyValuePair<int, Question> q in questionDatabase.allQuestions)
        {

            if (!addedOptions.Contains(q.Value.category))
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
                newOption.text = System.Web.HttpUtility.HtmlDecode(q.Value.category);
                categoryOptions.Add(newOption);
               addedOptions.Add(q.Value.category);
            }
        }

        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(categoryOptions);

        foreach(string diff in questionDatabase.availableDifficulties)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
            newOption.text = diff;
            difficultyOptions.Add(newOption);
        }

        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(difficultyOptions);

  
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
